# Local setup (personal fork)

## Prerequisites

- .NET 9 SDK (you have .NET 10 SDK; project targets net9.0)
- Node.js 20+
- Docker Desktop (for RavenDB + Redis)

## 1. Start Docker Desktop

Docker license must be active and the engine running. Verify:

```powershell
docker version
```

## 2. Start infrastructure

```powershell
cd "E:\matrix-book-studio\matrix-book-studio-main\Book Studio"
docker compose -f docker-compose.dev.yml up -d
```

RavenDB Studio: http://localhost:8080

## 3. Local configuration

Copy the example and adjust paths:

```powershell
copy "Book Studio\MatrixBook.Server\appsettings.Local.json.example" "Book Studio\MatrixBook.Server\appsettings.Local.json"
```

`appsettings.Local.json` is gitignored. Do not commit API keys.

Create book folders if needed:

```powershell
mkdir E:\matrix-book-studio\books -Force
mkdir E:\matrix-book-studio\videos -Force
```

## 4. Run the app

```powershell
cd "Book Studio\MatrixBook.Server"
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --launch-profile https
```

- API/Swagger: https://localhost:7110/swagger
- Angular dev server (via SPA proxy): https://localhost:8090

Or open `matrix-book-studio.sln` in Visual Studio and run **MatrixBook.Server**.

## Notes

- Original `appsettings.json` contains paths and keys from the upstream author; use `appsettings.Local.json` for your machine.
- Edge TTS (`Scripts/edge-tts`) is optional; speech features need that service or Azure keys in local config.
- Full Docker app image (`matrix.studio/latest`) is not required for local development.
