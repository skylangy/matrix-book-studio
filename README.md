# Matrix Book Studio

全栈有声书 / 图书制作平台：支持图书管理、语音合成、影片合成与音频业务。

## 技术栈

| 模块 | 技术 |
|------|------|
| Book Studio 后端 | ASP.NET Core 9、SignalR、RavenDB |
| Book Studio 前端 | Angular 20、Bootstrap、Highcharts、Monaco Editor |
| Matrix Audio | .NET 9、Stripe 支付集成 |
| 基础设施 | Docker（RavenDB、Redis）、Edge TTS 脚本 |

## 功能概览

- **Book Studio**：图书库、章节编辑、Azure/Edge 语音合成、字幕与视频导出
- **Matrix Audio**：音频服务、订阅与支付
- **工具链**：PDF 提取、字幕脚本、库同步、系统托盘

## 本地运行

详见 [LOCAL_SETUP.md](LOCAL_SETUP.md)。

```powershell
cd "Book Studio"
docker compose -f docker-compose.dev.yml up -d
cd MatrixBook.Server
dotnet run --launch-profile https
```

- API / Swagger：https://localhost:7110/swagger  
- 前端（开发）：https://localhost:8090  

## 项目结构

```
Book Studio/          # 主业务：Server + Angular Client
Matrix Audio/         # 音频与支付服务
Scripts/              # Python / 浏览器 / TTS 辅助脚本
SpeechHub/            # 语音相关服务
```

## 配置说明

复制 `Book Studio/MatrixBook.Server/appsettings.Local.json.example` 为 `appsettings.Local.json`，按本机路径填写书籍目录与 API 密钥。示例配置见 `appsettings.example.json`。
