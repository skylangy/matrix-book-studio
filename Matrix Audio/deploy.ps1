# Configuration Variables
$APP_NAME = "matrixbookstudio"
$REGISTRY_URL = "http://192.168.7.218:8961"
$IMAGE_NAME = "$APP_NAME"
$DOCKERFILE_PATH = ".\Dockerfile"  # Update with your Dockerfile path if necessary
$PROJECT_PATH = "./MatrixBook.Server"  # Path to your ASP.NET project
$PROFILE_PATH = "./MatrixBook.Server/Properties/PublishProfiles/FolderProfile.pubxml"  # Path to your publish profile

# Function to check for errors after each step
function Check-Error {
    if ($LASTEXITCODE -ne 0) {
        Write-Host "An error occurred, exiting..."
        exit $LASTEXITCODE
    }
}

# Step 1: Publish the application using .NET CLI
Write-Host "Publishing the application..."
dotnet publish $PROJECT_PATH -c Release -p:PublishProfile=FolderProfile
Check-Error  # Exit on failure

# Step 2: Build and Tag Docker image
Write-Host "Building Docker image..."
docker-compose build --no-cache
Check-Error  # Exit on failure

# Step 3: Push the Docker image to the registry
Write-Host "Pushing Docker image to registry..."
docker-compose push
Check-Error  # Exit on failure

Write-Host "Application successfully published and Docker image pushed to registry."
