$raspberryPis = @("192.168.7.173", "192.168.7.174", "192.168.7.175", "192.168.7.186", "192.168.7.205") # Pi IPs
$sshUser = "andy"                                # SSH username
$remoteScriptFolder = "/home/pi/scripts"         # Folder containing docker-compose.yml

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

function updateDockerConfig {
    foreach ($pi in $raspberryPis) {
        try {
            logInfo "Updating Docker configuration on $pi"

            ssh $sshUser@$pi "cd $remoteScriptFolder && sudo docker-compose down"
            logInfo "Docker containers stopped on $pi"

            ssh $sshUser@$pi "cd $remoteScriptFolder && sudo ./update_docker.sh"
            logInfo "Docker configuration updated on $pi"

            ssh $sshUser@$pi "cd $remoteScriptFolder && sudo docker-compose up -d"
            logInfo "Docker containers started on $pi"
        }
        catch {
            logError "Failed to update Docker configuration on $pi`: $_"
        }
    }
}

updateDockerConfig