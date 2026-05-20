# Local Development

## Prerequisites

- .NET 9 SDK
- Node.js 20+
- Docker Desktop

## 1. Start Docker

```powershell
docker version
```

## 2. Start RavenDB and Redis

```powershell
cd "E:\matrix-book-studio\matrix-book-studio-main\Book Studio"
docker compose -f docker-compose.dev.yml up -d
```

RavenDB Studio: http://localhost:8080

## 3. Local configuration

```powershell
copy "MatrixBook.Server\appsettings.Local.json.example" "MatrixBook.Server\appsettings.Local.json"
mkdir E:\matrix-book-studio\books -Force
mkdir E:\matrix-book-studio\videos -Force
```

Edit paths and secrets in `appsettings.Local.json` as needed.

## 4. Run the application

```powershell
cd MatrixBook.Server
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --launch-profile https
```

Alternatively, open `matrix-book-studio.sln` in Visual Studio and run **MatrixBook.Server**.
