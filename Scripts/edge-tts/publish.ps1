$ImageName = "edge-tts-api"
$ContainerName = "edge-tts-api"
$DockerfilePath = "."
$Port = 8965  #
$LogFile = "edge-tts-log.txt"

# Function to log messages with a timestamp
function Write-Log {
    param (
        [string]$Message
    )

    $Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $LogMessage = "$Timestamp - $Message"
    Write-Host $LogMessage
    Add-Content -Path $LogFile -Value "$LogMessage`n"
}

# Log the start of the process
Write-Log "Starting Edge-TTS Docker container setup..."

Write-Log "Building Docker image $ImageName..."

docker build -t $ImageName $DockerfilePath
Write-Log "Docker image $ImageName built."

# Check if the container is already running and stop/remove it if necessary
$existing = docker ps -aq -f name=$ContainerName
if ($existing) {
    Write-Log "♻️ Stopping and removing existing container..."
    docker stop $ContainerName | Out-Null
    docker rm $ContainerName | Out-Null
    Write-Log "Existing container '$ContainerName' stopped and removed."
}

# Run the Docker container
Write-Log "Starting container '$ContainerName' on port $Port..."

# $PortMapping = "$Port`:8000"
# docker run -d --name $ContainerName -p $PortMapping $ImageName
docker buildx build --platform linux/arm64 -t 192.168.7.218:8961/matrix-edge-tts-api:latest --push .

Write-Log "`Edge-TTS API is published to registry"

# docker pull 192.168.7.1:8961/matrix-edge-tts-api:latest
# docker run -d -p 8965:8000 192.168.7.1:8961/matrix-edge-tts-api:latest
