## Use the official .NET 9.0 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

## Copy the project file and restore dependencies
COPY ["Matrix.Audio.Server/Matrix.Audio.Server.csproj", "Matrix.Audio.Server/"]
RUN dotnet restore "Matrix.Audio.Server/Matrix.Audio.Server.csproj"

# Copy the published files from your local machine to the container's /app directory
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

# Copy the published application files into the Docker image
COPY "Matrix.Audio.Server/bin/Release/net9.0/publish/" .

# Set the entry point to the ASP.NET Core app
ENTRYPOINT ["dotnet", "Matrix.Audio.Server.dll"]
