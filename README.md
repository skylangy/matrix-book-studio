# matrix-book-studio (personal fork)

Audio book / matrix studio monorepo: **Book Studio** (.NET 9 + Angular 20) and **Matrix Audio**.

Originally from [GitLab andylang/matrix-book-studio](https://gitlab.com/andylang/matrix-book-studio). This copy is configured for local development on Windows.

## Quick start

See [LOCAL_SETUP.md](LOCAL_SETUP.md).

1. Start **Docker Desktop**
2. `docker compose -f "Book Studio/docker-compose.dev.yml" up -d`
3. Copy `Book Studio/MatrixBook.Server/appsettings.Local.json.example` → `appsettings.Local.json` and edit paths
4. `cd "Book Studio/MatrixBook.Server"` → `dotnet run --launch-profile https`

- Swagger: https://localhost:7110/swagger  
- UI (Angular via SPA proxy): https://localhost:8090  

## Publish to your GitHub

```powershell
cd E:\matrix-book-studio\matrix-book-studio-main
git init
git add .
git commit -m "Initial personal fork with local dev setup"

# Create empty repo on GitHub (e.g. matrix-book-studio), then:
git remote add origin https://github.com/skylangy/matrix-book-studio.git
git branch -M main
git push -u origin main
```

Do not commit `appsettings.Local.json` or real API keys. Use `appsettings.example.json` / `appsettings.Local.json.example` as templates.

## License

Respect the original project license if present; this fork is for personal use unless you add explicit licensing.
