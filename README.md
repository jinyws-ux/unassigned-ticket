# ITCC 未分配工单监控

一个独立的 Outlook Web Add-in，用于在右侧任务窗格持续展示 **IT Control Center L2** 组内尚未分配处理人的 INC/WO 工单。

## 第一版功能

- Outlook 邮件阅读界面的可固定任务窗格
- 每 60 秒自动刷新，支持手动刷新
- 全部、INC、WO 类型筛选
- 按等待时间排序
- 加载、空结果、旧数据和首次失败状态
- 点击工单跳转 Helix（接口返回 `url` 时）
- Mock 数据与真实 API 可配置切换

## 数据边界

插件不直接访问 Helix，也不保存 `apikey`、JWT 或其他 Helix 凭据。后端负责 Token 管理、Helix 查询和字段清洗，插件只调用以下聚合接口：

```http
GET /api/v1/groups/itcc-l2/unassigned-tickets
```

预期响应见 [`docs/API_CONTRACT.md`](docs/API_CONTRACT.md)。

## 快速开始

要求：Node.js 20+、npm、Microsoft 365 Outlook 测试账号。

```bash
npm install
npm run validate
npm test
npm run build
npm start
```

`npm start` 会启动 HTTPS 开发服务器并侧载 `manifest.xml`。首次运行时可能提示安装本地开发证书。

只在浏览器中检查 UI：

```bash
npm run dev-server
```

然后访问 `https://localhost:3000/taskpane.html`。

## 接入真实后端

修改 [`public/config.js`](public/config.js)：

```javascript
window.__ITCC_CONFIG__ = {
  apiBaseUrl: "https://your-internal-api.example.com",
  endpointPath: "/api/v1/groups/itcc-l2/unassigned-tickets",
  refreshIntervalMs: 60000,
  requestTimeoutMs: 15000,
  useMockData: false
};
```

不要在这个文件中写任何 Helix Token、apikey 或账号密码。生产部署时还需要把 `manifest.xml` 中的 `https://localhost:3000` 替换成实际 HTTPS 地址。

## 常用命令

| 命令 | 用途 |
| --- | --- |
| `npm start` | 启动并侧载 Outlook 插件 |
| `npm stop` | 停止调试并移除侧载 |
| `npm run dev-server` | 仅启动 HTTPS 前端 |
| `npm run build` | 生成 `dist/` 生产文件 |
| `npm test` | 运行单元测试 |
| `npm run typecheck` | TypeScript 检查 |
| `npm run validate` | 校验 Outlook Manifest |

## 文档

- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)：模块职责与后续扩展边界
- [`docs/API_CONTRACT.md`](docs/API_CONTRACT.md)：插件与后端接口约定
- [`docs/DEPLOYMENT.md`](docs/DEPLOYMENT.md)：构建、部署和侧载说明
