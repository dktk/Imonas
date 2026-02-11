#!/usr/bin/env bash
set -euo pipefail

echo "==> Installing Docker Engine from Docker's official APT repository (Ubuntu)"

# 1) Remove conflicting old packages (safe)
echo "==> Removing old/conflicting Docker packages (if any)..."
sudo apt-get remove -y docker docker-engine docker.io containerd runc || true

# 2) Install prerequisites
echo "==> Installing prerequisites..."
sudo apt-get update
sudo apt-get install -y ca-certificates curl gnupg

# 3) Add Docker’s GPG key
echo "==> Adding Docker GPG key..."
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg

# 4) Add the Docker APT repo
echo "==> Adding Docker APT repository..."
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# 5) Install Docker Engine + Compose plugin
echo "==> Installing Docker Engine + CLI + containerd + Buildx + Compose plugin..."
sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# 6) Verify it works
echo "==> Verifying installation..."
sudo docker run --rm hello-world
docker version
docker compose version

# 7) Optional: run Docker without sudo
echo
echo "==> OPTIONAL (recommended): enable running docker without sudo for your user."
echo "    This will add your current user to the 'docker' group."
echo "    You may need to log out/in (or reboot) for it to fully apply."
read -r -p "Add user '$USER' to docker group now? [y/N]: " yn
case "${yn:-N}" in
  [Yy]*)
    sudo usermod -aG docker "$USER"
    echo "==> Added '$USER' to docker group."
    echo "==> You can run: newgrp docker   (or log out/in) to apply immediately."
    ;;
  *)
    echo "==> Skipping docker group change."
    ;;
esac

echo "==> Done."
