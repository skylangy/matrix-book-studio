
# Define logInfo function
function logInfo {
    param (
        [string]$message
    )
    $timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    Write-Host "$timestamp [INFO] $message" -ForegroundColor Green
}

# Define logWarn function
function logWarn {
    param (
        [string]$message
    )
    $timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    Write-Host "$timestamp [WARN] $message" -ForegroundColor Yellow
}

function Test-LastExitCode {
    param (
        [string]$successMessage,
        [string]$errorMessage
    )

    if ($LASTEXITCODE -eq 0) {
        logInfo $successMessage
    }
    else {
        logError $errorMessage
        throw
    }
}

# Define logError function
function logError {
    param (
        [string]$message
    )
    $timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    Write-Host "$timestamp [ERROR] $message" -ForegroundColor Red
}

function invokeWithRetry {
    param (
        [ScriptBlock]$Action,
        [int]$MaxRetries = 3,
        [int]$DelaySeconds = 2
    )

    $retryCount = 0

    while ($retryCount -lt $MaxRetries) {
        try {
            &$Action
            return
        }
        catch {
            $retryCount++
            Write-Host "Attempt $retryCount failed: $_" -ForegroundColor Red
            if ($retryCount -lt $MaxRetries) {
                Write-Host "Retrying in $DelaySeconds seconds..." -ForegroundColor Yellow
                Start-Sleep -Seconds $DelaySeconds
            }
            else {
                Write-Host "All retries failed. Exiting." -ForegroundColor Red
                throw
            }
        }
    }
}

# Configuration
$raspberryPis = @("192.168.7.173", "192.168.7.174", "192.168.7.175", "192.168.7.186", "192.168.7.205") # Pi IPs
$sshUser = "andy"                                # SSH username
$remoteScriptFolder = "/home/pi/scripts"       # Folder containing docker-compose.yml

function updateAppImages {
    logInfo "Starting build matrix audio..."
    dotnet publish -c Release Matrix.Audio.Server/Matrix.Audio.Server.csproj /p:PublishProfile=Matrix.Audio.Server/Properties/PublishProfiles/FolderProfile.pubxml

    Test-LastExitCode -successMessage "Successfully built Matrix Audio Server." -errorMessage "Failed to build Matrix Audio Server."

    logInfo "Starting update docker image..."
    docker buildx build --platform linux/arm64 -t 192.168.7.1:8961/matrixaudio:latest --push .

    Test-LastExitCode -successMessage "Successfully updated Docker image." -errorMessage "Failed to update Docker image."

    # Loop through each Raspberry Pi
    foreach ($pi in $raspberryPis) {
        logInfo "============================================================="
        logInfo "Connecting to Raspberry Pi: $pi"

        try {
            # Step 1: Navigate to the folder and stop the existing containers
            ssh $sshUser@$pi "cd $remoteScriptFolder && sudo docker-compose down"
            logInfo "Stopped running containers on $pi"

            # Step 2: Pull the latest images
            logInfo "Removing old images on $pi"
            ssh $sshUser@$pi "cd $remoteScriptFolder && sudo docker image rm -f 192.168.7.1:8961/matrixaudio:latest"
            logInfo "Removed old images on $pi"

            logInfo "Pulling latest images on $pi"
            ssh $sshUser@$pi "cd $remoteScriptFolder && sudo docker pull 192.168.7.1:8961/matrixaudio:latest"
            logInfo "Pulled latest images on $pi"

            logInfo "Pulling latest images on $pi"
            ssh $sshUser@$pi "cd $remoteScriptFolder && sudo docker-compose pull"
            logInfo "Pulled latest images on $pi"

            # Step 3: Start the containers
            logInfo "Starting containers on $pi"
            ssh $sshUser@$pi "cd $remoteScriptFolder && sudo docker-compose up -d"
            logInfo "Started containers on $pi"

        }
        catch {
            logError "Error connecting or running commands on $pi`: $_"
        }
    }
    logInfo "Docker Compose deployment completed for all Raspberry Pis."
}

try {
    invokeWithRetry -Action { updateAppImages } -MaxRetries 3 -DelaySeconds 5
}
catch {
    Write-Host "Operation failed after all retries." -ForegroundColor Red
}