#!/usr/bin/env bash
set -euo pipefail

# ============================================================
# deploy_dotnet9_app.sh
#
# Deploy an ASP.NET Core (.NET 9) app on Ubuntu 24.04 using:
# - Kestrel (listening on localhost)
# - systemd service
# - Nginx reverse proxy
#
# Usage:
#   sudo ./deploy_dotnet9_app.sh <APP_NAME> [PORT] [PUBLISH_TARBALL] [DLL_NAME]
#
# Examples:
#   sudo ./deploy_dotnet9_app.sh myapp
#   sudo ./deploy_dotnet9_app.sh myapp 5005 /tmp/myapp.tar.gz MyApp.dll
#
# Notes:
# - This script expects a publish artifact (tar.gz) OR you can copy files later.
# - If you only have a folder, tar it first:
#     tar -czf /tmp/myapp.tar.gz -C ./publish .
# ============================================================

APP_NAME="${1:-}"
PORT="${2:-5000}"
PUBLISH_TARBALL="${3:-}"
DLL_NAME="${4:-}"   # optional; if empty we'll try to auto-detect a single *.dll
SERVER_NAMES="${5:-${APP_NAME}.imonas.com}"

if [[ -z "$APP_NAME" ]]; then
  echo "ERROR: APP_NAME is required."
  echo "Usage: sudo $0 <APP_NAME> [PORT] [PUBLISH_TARBALL] [DLL_NAME]"
  exit 1
fi

if [[ "$(id -u)" -ne 0 ]]; then
  echo "ERROR: run as root (use sudo)."
  exit 1
fi

echo "==> Updating apt and installing prerequisites..."
apt update -y
apt install -y software-properties-common ca-certificates curl gnupg nginx ufw

echo "==> Enabling firewall rules (SSH + Nginx Full)..."
ufw allow OpenSSH >/dev/null || true
ufw allow 'Nginx Full' >/dev/null || true
ufw --force enable >/dev/null || true

echo "==> Adding Canonical .NET backports PPA and installing ASP.NET Core Runtime 9.0..."
# On Ubuntu 24.04, .NET 9 is commonly provided via this backports PPA.
add-apt-repository -y ppa:dotnet/backports
apt update -y
apt install -y aspnetcore-runtime-9.0

echo "==> Creating service user and app directories..."
# Create a system user (no login shell) for the application
if ! id "${APP_NAME}" >/dev/null 2>&1; then
  useradd -r -s /usr/sbin/nologin "${APP_NAME}"
fi

APP_DIR="/var/www/${APP_NAME}"
ENV_DIR="/etc/${APP_NAME}"
mkdir -p "${APP_DIR}" "${ENV_DIR}"
chown -R "${APP_NAME}:${APP_NAME}" "${APP_DIR}"
chmod 750 "${APP_DIR}"

echo "==> Deploying published artifact (if provided)..."
if [[ -n "${PUBLISH_TARBALL}" ]]; then
  if [[ ! -f "${PUBLISH_TARBALL}" ]]; then
    echo "ERROR: PUBLISH_TARBALL not found at: ${PUBLISH_TARBALL}"
    exit 1
  fi

  # Clear old files and extract new publish output
  rm -rf "${APP_DIR:?}/"*
  tar -xzf "${PUBLISH_TARBALL}" -C "${APP_DIR}"
  chown -R "${APP_NAME}:${APP_NAME}" "${APP_DIR}"
fi

echo "==> Determining DLL to run..."
if [[ -z "${DLL_NAME}" ]]; then
  # Try to auto-detect: if exactly one *.dll exists at root, use it.
  DLL_COUNT=$(find "${APP_DIR}" -maxdepth 1 -type f -name "*.dll" | wc -l | tr -d ' ')
  if [[ "${DLL_COUNT}" -eq 1 ]]; then
    DLL_NAME=$(basename "$(find "${APP_DIR}" -maxdepth 1 -type f -name "*.dll")")
  else
    echo "ERROR: Could not auto-detect DLL (found ${DLL_COUNT} DLLs in ${APP_DIR})."
    echo "Provide DLL_NAME explicitly: sudo $0 ${APP_NAME} ${PORT} ${PUBLISH_TARBALL} MyApp.dll"
    exit 1
  fi
fi

echo "==> Writing environment file..."
ENV_FILE="${ENV_DIR}/${APP_NAME}.env"
cat > "${ENV_FILE}" <<EOF
# Environment for ${APP_NAME}
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://127.0.0.1:${PORT}

# If your app is behind Nginx and you rely on scheme/remote IP, consider forwarded headers in code.
# Add your app config here, for example:
# ConnectionStrings__Default=Host=127.0.0.1;Port=5432;Database=${APP_NAME};Username=main-admin;Password=...
EOF
chown root:root "${ENV_FILE}"
chmod 600 "${ENV_FILE}"

echo "==> Creating systemd service..."
SERVICE_FILE="/etc/systemd/system/${APP_NAME}.service"
cat > "${SERVICE_FILE}" <<EOF
[Unit]
Description=${APP_NAME} (.NET 9)
After=network.target

[Service]
WorkingDirectory=${APP_DIR}
ExecStart=/usr/bin/dotnet ${APP_DIR}/${DLL_NAME}
Restart=always
RestartSec=5
User=${APP_NAME}
EnvironmentFile=${ENV_FILE}

# Basic hardening
NoNewPrivileges=true
PrivateTmp=true

[Install]
WantedBy=multi-user.target
EOF

systemctl daemon-reload
systemctl enable "${APP_NAME}.service"
systemctl restart "${APP_NAME}.service"

echo "==> Configuring Nginx reverse proxy..."
# Ensure websocket helper map exists (safe to add once)
NGINX_CONF="/etc/nginx/nginx.conf"
if ! grep -q "map \$http_upgrade \$connection_upgrade" "${NGINX_CONF}"; then
  # Insert map inside http {} block right after 'http {'
  sed -i '0,/http {/s/http {/&\n\n    map $http_upgrade $connection_upgrade {\n        default upgrade;\n        '\'''\''      close;\n    }\n/' "${NGINX_CONF}"
fi


SITE_AVAILABLE="/etc/nginx/sites-available/${APP_NAME}"
# cat > "${SITE_AVAILABLE}" <<EOF
# server {
#     listen 80;
#     server_name test.imonas.com;
# 
#     location / {
#         proxy_pass         http://127.0.0.1:${PORT};
#         proxy_http_version 1.1;
# 
#         proxy_set_header   Host              \$host;
#         proxy_set_header   X-Real-IP         \$remote_addr;
#         proxy_set_header   X-Forwarded-For   \$proxy_add_x_forwarded_for;
#         proxy_set_header   X-Forwarded-Proto \$scheme;
# 
#         proxy_set_header   Upgrade           \$http_upgrade;
#         proxy_set_header   Connection        \$connection_upgrade;
#     }
# }
# EOF


ln -sf "${SITE_AVAILABLE}" "/etc/nginx/sites-enabled/${APP_NAME}"
nginx -t
systemctl reload nginx


echo
echo "DONE."
echo "App:        ${APP_NAME}"
echo "DLL:        ${DLL_NAME}"
echo "Kestrel:    http://127.0.0.1:${PORT}"

PUBLIC_IP="$(curl -fsS https://api.ipify.org || curl -fsS https://ifconfig.me || echo "<unknown>")"
echo "Public:     http://${PUBLIC_IP}/"

echo
echo "Useful commands:"
echo "  systemctl status ${APP_NAME} --no-pager"
echo "  journalctl -u ${APP_NAME} -f"
echo
echo "Next steps (recommended):"
echo "  - Point a domain to ${PUBLIC_IP}"
echo "  - Install TLS via certbot (Nginx plugin)."
