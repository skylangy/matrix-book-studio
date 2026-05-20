# List of Raspberry Pi IP addresses
$piIPs = @( "192.168.7.173", "192.168.7.174", "192.168.7.175", "192.168.7.205", "192.168.7.186")

# Files to transfer
$filesToCopy = @("docker-compose.yml", ".env", "init_pi.sh", "update_docker.sh", "daemon.json")
$destinationPath = "/home/pi/scripts"

function logInfo {
    param (
        [string]$message
    )
    $timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    Write-Host "$timestamp [INFO] $message" -ForegroundColor Green
}

function logError {
    param (
        [string]$message
    )
    $timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    Write-Host "$timestamp [ERROR] $message" -ForegroundColor Red
}


# Loop through each IP and execute SCP
foreach ($ip in $piIPs) {
    logInfo "Transferring files to $ip..."
    $remotePath = "andy@$ip`:$destinationPath"  # Escape ':' with backtick

    foreach ($file in $filesToCopy) {
        $remotePath = "andy@$ip`:$destinationPath"

        logInfo "Execute command: scp -r -p $file $remotePath"
        # Execute SCP for each file
        logInfo "Copying $file to $remotePath"
        & scp -p $file $remotePath

        # Check if SCP succeeded
        if ($LASTEXITCODE -eq 0) {
            logInfo "Successfully copied $file to $ip" -ForegroundColor Green
        }
        else {
            logError "Failed to transfer $file to $ip" -ForegroundColor Red
        }
    }

    # Make init_pi.sh executable (after copying it)
    logInfo "Making init_pi.sh executable on $ip..."
    & ssh andy@$ip "sudo chmod +x $destinationPath/init_pi.sh"
    & ssh andy@$ip "sudo chmod +x $destinationPath/update_docker.sh"

    # Verify chmod was successful
    if ($LASTEXITCODE -eq 0) {
        logInfo "Successfully made init_pi.sh executable on $ip" -ForegroundColor Green
    }
    else {
        logInfo "Failed to make init_pi.sh executable on $ip" -ForegroundColor Red
    }
}

logInfo "File transfer completed!"
