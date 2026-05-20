# 上传到 GitHub（skylangy）

仓库已本地初始化并完成首次提交，远程地址：

`https://github.com/skylangy/matrix-book-studio.git`

你的 GitHub 主页目前没有公开仓库，需要**先登录 GitHub CLI，再创建并推送**。

## 方法一：GitHub CLI（推荐）

在 PowerShell 中执行：

```powershell
cd E:\matrix-book-studio\matrix-book-studio-main

# 1. 登录（浏览器授权，只需一次）
gh auth login
# 选：GitHub.com → HTTPS → Login with a web browser

# 2. 创建公开仓库并推送（仓库名 matrix-book-studio）
gh repo create matrix-book-studio --public --source=. --remote=origin --push
```

若远程已存在同名空仓库，只需：

```powershell
git push -u origin main
```

## 方法二：网页创建仓库

1. 打开 https://github.com/new  
2. Repository name：`matrix-book-studio`  
3. 选 **Public**，**不要**勾选 “Add a README”  
4. 创建后执行：

```powershell
cd E:\matrix-book-studio\matrix-book-studio-main
git push -u origin main
```

## 已处理的安全项

- `appsettings.Local.json` 已加入 `.gitignore`，不会上传
- `appsettings.json` 里的 Azure Key 已替换为占位符
- 真实密钥请只放在本机 `appsettings.Local.json`

## 登录后让我继续

在终端完成 `gh auth login` 后，告诉我一声，我可以帮你执行 `gh repo create` 和 `git push`。
