# 部署说明

## 服务器配置问题

如果遇到错误：`Failed to load module script: Expected a JavaScript-or-Wasm module script but the server responded with a MIME type of "text/html"`

这是因为使用 Vue Router 的 history 模式时，服务器需要配置将所有路由请求回退到 `index.html`。

## Apache 服务器配置

如果使用 Apache 服务器，请确保将 `.htaccess` 文件部署到 `dist` 目录中。

`.htaccess` 文件已经包含在项目中，它会：
- 将所有不存在的文件/目录请求重定向到 `index.html`
- 确保 SPA 路由正常工作

## Nginx 服务器配置

如果使用 Nginx 服务器，请参考 `nginx.conf.example` 文件进行配置。

关键配置：
```nginx
location / {
    try_files $uri $uri/ /index.html;
}
```

这将确保所有路由请求都回退到 `index.html`。

## 构建和部署

1. 构建项目：
```bash
npm run build:test  # 测试环境
npm run build:prod  # 生产环境
```

2. 将 `dist` 目录中的文件部署到服务器

3. 确保服务器配置正确（Apache 需要 `.htaccess`，Nginx 需要相应配置）

4. 如果部署在子路径下，请设置环境变量 `VITE_BASE_URL` 为相应的路径（例如：`/subpath/`）

