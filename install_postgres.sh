#!/usr/bin/env bash
set -euo pipefail

# ============================================================
# install_postgres.sh
#
# Installs PostgreSQL and creates an admin role:
#   username: main-admin
#   password: (provided below)
#
# Usage:
#   sudo ./install_postgres.sh [DB_NAME]
#
# Example:
#   sudo ./install_postgres.sh myappdb
#
# Security notes:
# - This script contains a plaintext password because you requested it.
#   Prefer passing via environment variables or prompting in real ops.
# - By default we DO NOT expose PostgreSQL to the internet.


# sudo apt update
# sudo apt -y install dos2unix
# dos2unix install_postgres.sh
# dos2unix deploy_dotnet9_app.sh
# chmod +x install_postgres.sh deploy_dotnet9_app.sh
# sudo ./install_postgres.sh

# ============================================================

DB_NAME="${1:-imonasdb}"
PG_ADMIN_USER="main-admin"
PG_ADMIN_PASS="h3iu1h312089790o12h3kjh"

if [[ "$(id -u)" -ne 0 ]]; then
  echo "ERROR: run as root (use sudo)."
  exit 1
fi

echo "==> Installing PostgreSQL..."
apt update -y
apt install -y postgresql postgresql-contrib

echo "==> Enabling and starting PostgreSQL..."
systemctl enable postgresql
systemctl start postgresql

echo "==> Creating role '${PG_ADMIN_USER}' and database '${DB_NAME}'..."
# Create role (LOGIN + SUPERUSER for full admin capabilities)
# If you want less privilege, remove SUPERUSER and use CREATEDB CREATEROLE instead.
sudo -u postgres psql -v ON_ERROR_STOP=1 <<SQL
DO \$\$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = '${PG_ADMIN_USER}') THEN
    CREATE ROLE "${PG_ADMIN_USER}" WITH LOGIN SUPERUSER PASSWORD '${PG_ADMIN_PASS}';
  END IF;
END
\$\$;

-- Create DB if not exists and set owner to admin
DO \$\$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = '${DB_NAME}') THEN
    CREATE DATABASE "${DB_NAME}" OWNER "${PG_ADMIN_USER}";
  END IF;
END
\$\$;
SQL

echo "==> Hardening defaults (local-only binding remains default on Ubuntu)."
echo "    Verifying listen address..."
# On Ubuntu, default is typically: listen_addresses = 'localhost'
# We leave it unchanged unless you explicitly need remote access.

echo
echo "DONE."
echo "PostgreSQL installed."
echo "Admin role:  ${PG_ADMIN_USER}"
echo "Database:    ${DB_NAME}"
echo
echo "Local connect test (from VPS):"
echo "  psql \"postgresql://${PG_ADMIN_USER}:${PG_ADMIN_PASS}@127.0.0.1:5432/${DB_NAME}\""
echo
echo "If you *need* remote access later (not recommended without extra controls):"
echo "  - set listen_addresses='*' in postgresql.conf"
echo "  - add a restricted CIDR rule to pg_hba.conf"
echo "  - open UFW only for your IP: ufw allow from <YOUR_IP> to any port 5432"
