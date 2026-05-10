# TouchSocket.SocketIo

TouchSocket 框架的 Socket.IO 协议组件，提供完整的 Socket.IO 客户端实现。

## 协议支持

- 兼容 Socket.IO 3.x 和 4.x 协议
- 支持 Engine.IO v3 和 v4 协议

## 传输方式

| 传输方式 | 说明 |
|---|---|
| WebSocket | 高性能双向通信 |
| HTTP 长轮询 | 兼容性回退方案 |
| 自动升级 | 默认先通过 HTTP 长轮询握手，成功后自动升级为 WebSocket |

## 核心特性

- **Emit/Ack 模型** — `EmitAsync` 发送事件，`EmitWithAckAsync` 发送并等待 Ack 响应
- **命名空间** — 支持连接到指定 Namespace（如 `/chat`）
- **二进制附件** — 完整支持 `_placeholder` 占位符机制
- **自定义 Query** — 握手时可携带自定义查询参数
- **插件扩展** — 通过 `ISocketIoEventPlugin` 拦截事件，`ISocketIoHandshakedPlugin` 监听握手
- **可替换序列化器** — 默认内置 System.Text.Json，支持通过 `ISocketIoSerializer` 自定义

## 快速使用

```csharp
var client = new SocketIoClient();
client.Setup(new TouchSocketConfig()
    .SetRemoteIPHost("http://localhost:3000")
    .Configure<SocketIoOption>(opt =>
    {
        opt.EIO = EngineIoVersion.V4;
        opt.Transport = EngineIoTransportType.WebSocket;
        opt.Namespace = "/chat";
    }));

await client.ConnectAsync();

// 发送事件
await client.EmitAsync("message", "hello");

// 发送并等待 Ack
var response = await client.EmitWithAckAsync("query", new object[] { "id" }, 5000);
var result = response.GetValue<string>(0);
```

## 支持的目标框架

- net462
- netstandard2.0
- netstandard2.1
- net6.0
- net8.0
- net10.0

## 文档

详细说明文档：[https://touchsocket.net/](https://touchsocket.net/)
