# Matrix Book Studio

A full-stack platform for audiobook and digital book production: library management, speech synthesis, video composition, and audio services.

## Tech Stack

| Area | Technologies |
|------|----------------|
| Book Studio backend | ASP.NET Core 9, SignalR, RavenDB |
| Book Studio frontend | Angular 20, Bootstrap, Highcharts, Monaco Editor |
| Matrix Audio | .NET 9, Stripe payments |
| Infrastructure | Docker (RavenDB, Redis), Edge TTS scripts |

## Features

- **Book Studio** — Book library, chapter editing, Azure/Edge TTS, subtitles and video export
- **Matrix Audio** — Audio services, subscriptions, and payments
- **Tooling** — PDF extraction, subtitle scripts, library sync, system tray app

## Run Locally

See [LOCAL_SETUP.md](LOCAL_SETUP.md).

```powershell
cd "Book Studio"
docker compose -f docker-compose.dev.yml up -d
cd MatrixBook.Server
dotnet run --launch-profile https
```

- API / Swagger: https://localhost:7110/swagger  
- Frontend (dev): https://localhost:8090  

## Project Layout

```
Book Studio/          # Core app: API server + Angular client
Matrix Audio/         # Audio and payment services
Scripts/              # Python, browser, and TTS helper scripts
SpeechHub/            # Speech-related services
```

## Configuration

Copy `Book Studio/MatrixBook.Server/appsettings.Local.json.example` to `appsettings.Local.json` and set book paths and API keys for your machine. See `appsettings.example.json` for a template.
