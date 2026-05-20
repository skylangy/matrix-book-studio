function logInfo {
    param (
        [string]$message
    )
    $timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    Write-Host "$timestamp [INFO] $message" -ForegroundColor Green
}

$APP_NAME = "matrixbookstudio"
$IMAGE_NAME = "$APP_NAME"
$DOCKERFILE_PATH = ".\Dockerfile"  # Update with your Dockerfile path if necessary
$PROJECT_PATH = "./MatrixBook.Server"  # Path to your ASP.NET project
$PROFILE_PATH = "./MatrixBook.Server/Properties/PublishProfiles/FolderProfile.pubxml"  # Path to your publish profile


logInfo "Starting build matrix studio solution..."
# dotnet publish $PROJECT_PATH -c Release -p:PublishProfile=FolderProfile

logInfo "Starting update docker image..."
#docker buildx build --platform linux/arm64 -t 192.168.7.218:8961/matrixaudio:latest --push .
docker build -t matrix.studio/latest .