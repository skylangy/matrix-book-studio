
# Steps to initialize a new raspberry pi

## Install Raspberry OS Lite
## Get IP Address
## Manually install dependencies

* Install SSH client
  ```
  sudo apt update
  sudo apt install openssh-client

  sudo apt install openssh-server
  sudo systemctl enable ssh
  sudo systemctl start ssh

* Create folders
  ```
  sudo mkdir /home/pi/scripts
  sudo chown andy /home/pi/scripts # Allow writing

* Copy files by run `copy_init.ps1`
  ```
  .\copy_init.ps1

* Share folders from main Respberry PI (With SSD)
  ```
  sudo nano /etc/exports
  # Append below with correct IP address
  /mnt/data/books  192.168.7.175(rw,sync,no_subtree_check)

* Run init in Raspberry PI
  ```
  cd /home/pi/scripts
  ./init_pi.sh