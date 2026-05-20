#!/bin/bash

set -e # Exit on any error

update_docker_config() {
    DOCKER_CONFIG="/etc/docker/daemon.json"
    INSECURE_REGISTRY=("192.168.7.1:8961" "192.168.5.186:5000")
    SCRIPT_PATH="/home/pi/scripts"

    cd $SCRIPT_PATH
    sudo cp daemon.json $DOCKER_CONFIG
    sudo systemctl restart docker
}

main() {
    update_docker_config
}

# Execute the main function
main
