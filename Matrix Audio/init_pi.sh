#!/bin/bash

set -e # Exit on any error

prepare() {
    sudo apt update
    sudo apt upgrade -y

    # Check if curl is installed, and install it if not
    if ! command -v curl &>/dev/null; then
        echo "[INFO] curl is not installed. Installing curl..."
        sudo apt install -y curl
    else
        echo "[INFO] curl is already installed."
    fi
}

# Function to install Docker
install_docker() {
    echo "[INFO] Installing Docker..."

    DOCKER_CONFIG="/etc/docker/daemon.json"
    INSECURE_REGISTRY="192.168.7.218:8961"

    # Step 1: Install Docker if not already installed
    if ! command -v docker >/dev/null 2>&1; then
        echo "[INFO] Installing Docker..."
        curl -fsSL https://get.docker.com -o get-docker.sh
        sudo sh get-docker.sh
        sudo usermod -aG docker $USER
        sudo systemctl enable docker
        sudo systemctl start docker
    else
        echo "[INFO] Docker is already installed."
    fi

    # Step 2: Configure Docker daemon.json for insecure registry
    echo "[INFO] Configuring Docker for insecure registry..."

    if ! command -v jq &>/dev/null; then
        echo "[INFO] jq is not installed. Installing jq..."

        sudo apt-get update
        sudo apt-get install -y jq
        sudo apt -y autoremove

        echo "[INFO] jq installed successfully!"
    fi

    if [ ! -f "$DOCKER_CONFIG" ]; then
        # Create daemon.json if it doesn't exist
        echo "[INFO] { \"insecure-registries\": [\"$INSECURE_REGISTRY\"] }" | sudo tee "$DOCKER_CONFIG"
    else
        # Update existing daemon.json to include the registry
        echo "[INFO] Updating existing Docker configuration..."

        sudo cp daemon.json $DOCKER_CONFIG

        echo "[INFO] Docker configuration updated successfully!"
    fi

    # Step 3: Restart Docker to apply the new configuration
    echo "[INFO] Restarting Docker..."
    sudo systemctl restart docker

    echo "[INFO] Docker installed and setup successfully!"
}

# Function to install Docker Compose
install_docker_compose() {
    echo "[INFO] Installing Docker Compose..."

    sudo sudo apt install -y docker-compose

    echo "[INFO] Docker Compose installed successfully!"
}

# Function to install the NFS client
install_nfs_client() {
    echo "[INFO] Installing NFS client..."

    if ! dpkg -l | grep -q nfs-common; then
        sudo apt-get install -y nfs-common
        echo "[INFO] NFS client installed successfully!"
    else
        echo "[INFO] NFS client is already installed."
    fi

    # Create the mount point
    MOUNT_POINT="/mnt/data/books"
    echo "[INFO] Creating mount point at $MOUNT_POINT..."
    sudo mkdir -p $MOUNT_POINT
    echo "[INFO] Mount point created successfully!"

    # NFS Server and Exported Path
    NFS_SERVER="192.168.7.174:/mnt/data/books"

    # Add entry to /etc/fstab if not already present
    FSTAB_ENTRY="$NFS_SERVER $MOUNT_POINT nfs defaults 0 0"
    if ! grep -qs "$FSTAB_ENTRY" /etc/fstab; then
        echo "[INFO] Adding NFS mount to /etc/fstab..."
        echo "$FSTAB_ENTRY" | sudo tee -a /etc/fstab

        cat /etc/fstab
    else
        echo "[INFO] NFS mount already exists in /etc/fstab."
    fi

    # Mount the NFS share
    echo "[INFO] Mounting NFS share..."
    sudo mount -a

    # Verify the mount
    if mountpoint -q $MOUNT_POINT; then
        echo "[INFO] NFS share successfully mounted at $MOUNT_POINT."
    else
        echo "[INFO] Failed to mount NFS share."
        exit 1
    fi

    echo "[INFO] NFS client installed successfully!"
}

# Function to pull Docker images
pull_docker_image() {
    echo "[INFO] Pulling Matrix Audio Docker images..."
    sudo docker pull 192.168.7.1:8961/matrixaudio:latest
    echo "[INFO] Docker images pulled successfully!"
}

start_app() {
    echo "[INFO] Starting Matrix Audio..."
    CONTAINER_NAME="matrixaudio"
    if sudo docker ps --filter "name=$CONTAINER_NAME" --format '{{.Names}}' | grep -w $CONTAINER_NAME >/dev/null; then
        echo "[INFO] Stopping existing $CONTAINER_NAME..."
        sudo docker stop $CONTAINER_NAME
    else
        echo "[INFO] No running container named $CONTAINER_NAME found."
    fi

    sudo docker-compose up -d
    sudo docker ps
    echo "[INFO] Matrix Audio started successfully!"
}

# Main script logic
main() {
    echo "[INFO] Initializing Raspberry Pi for Docker setup..."
    prepare
    install_docker
    install_docker_compose
    install_nfs_client
    pull_docker_image
    start_app
    echo "[INFO] Raspberry Pi initialization complete!"
}

# Execute the main function
main
