$raspberryPis = @("192.168.7.173", "192.168.7.174", "192.168.7.175", "192.168.7.186", "192.168.7.205") # Pi IPs
$sshUser = "andy"

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

function rebootPi {
    foreach ($pi in $raspberryPis) {
        try {
            logInfo "Rebooting $pi"

            ssh $sshUser@$pi "sudo reboot"
            logInfo "$pi is rebooted"
        }
        catch {
            logError "Failed to reboot $pi`: $_"
        }
    }
}

rebootPi