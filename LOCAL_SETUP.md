# 本地开发指南

## 环境要求

- .NET 9 SDK
- Node.js 20+
- Docker Desktop

## 1. 启动 Docker

```powershell
docker version
```

## 2. 启动 RavenDB 与 Redis

```powershell
cd "E:\matrix-book-studio\matrix-book-studio-main\Book Studio"
docker compose -f docker-compose.dev.yml up -d
```

RavenDB Studio：http://localhost:8080

## 3. 本地配置

```powershell
copy "MatrixBook.Server\appsettings.Local.json.example" "MatrixBook.Server\appsettings.Local.json"
mkdir E:\matrix-book-studio\books -Force
mkdir E:\matrix-book-studio\videos -Force
```

按需修改 `appsettings.Local.json` 中的路径与密钥。

## 4. 运行应用

```powershell
cd MatrixBook.Server
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --launch-profile https
```

也可在 Visual Studio 中打开 `matrix-book-studio.sln`，启动 **MatrixBook.Server**。
