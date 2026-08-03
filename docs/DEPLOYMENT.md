# 构建与部署

## 本地检查

```bash
npm install
npm run typecheck
npm test
npm run validate
npm run build
```

## Outlook侧载

```bash
npm start
```

默认清单使用 `https://localhost:3000`。首次启动时根据提示安装Office开发证书。

停止并移除侧载：

```bash
npm stop
```

## 内部部署

1. 将 `npm run build` 生成的 `dist/` 部署到公司内可访问的HTTPS站点。
2. 设置生产版 `config.js`，关闭Mock并填写聚合后端URL。
3. 把 `manifest.xml` 中所有 `https://localhost:3000` 替换为生产HTTPS地址。
4. 再次执行 `npm run validate`。
5. 通过Microsoft 365管理中心集中部署清单。

## 回滚

- 前端静态文件和Manifest按版本归档。
- 回滚时同时恢复对应版本的 `dist/` 和 `manifest.xml`。
- `config.js` 独立保留，避免回滚代码时恢复错误的API地址。

