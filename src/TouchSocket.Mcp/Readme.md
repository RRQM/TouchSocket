# TouchSocket.Mcp

这是一个提供 MCP (Model Context Protocol) 服务器和客户端的组件库。

## 功能

- 支持 stdio、Streamable HTTP 和命名管道传输协议
- 支持 Tools、Resources、Prompts 功能
- 基于特性标注的 API 风格（`[McpTool]`、`[McpResource]`、`[McpPrompt]`）

## 命名管道

命名管道两端使用 UTF-8 JSON 消息，每条消息以 `\n` 结尾，组件自动处理消息分包和粘包。
JSON 序列化选项的 `WriteIndented` 必须保持为 `false`（默认值）。
这是 TouchSocket 的自定义 MCP 传输，两端需要使用相同的消息格式。

```csharp
using TouchSocket.Core;
using TouchSocket.Mcp;
using TouchSocket.NamedPipe;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

using var service = new McpNamedPipeService();
await service.SetupAsync(new TouchSocketConfig()
    .SetPipeName("touchsocket-mcp")
    .SetMcpServerOptions(options => options.ServerInfo.Name = "MyMcpServer")
    .ConfigureContainer(container => container.AddRpcStore(store => store.RegisterServer<MyTools>())));
await service.StartAsync();

using var client = new McpNamedPipeClient();
await client.SetupAsync(new TouchSocketConfig()
    .SetPipeName("touchsocket-mcp")
    .SetMcpClientOptions(options => options.Timeout = TimeSpan.FromSeconds(10)));
await client.ConnectAsync();
await client.InitializeAsync();
var tools = await client.ListAllToolsAsync();
var result = await client.CallToolAsync("Echo", new Dictionary<string, object> { ["text"] = "Hello MCP" });

await client.CloseAsync();
await service.StopAsync();

public class MyTools : SingletonRpcServer
{
    [McpTool]
    public string Echo(string text) => text;
}
```

服务端工具中可通过 `IMcpCallContext.Caller` 获取 `McpNamedPipeSessionClient`。
每个连接有独立的 MCP 会话；客户端断线重连后需再次调用 `InitializeAsync()`。
请求支持取消，连接关闭时会取消仍在等待响应的请求。

完整示例：`SimpleTest` 的 `12.1` 命令。

## 文档

https://touchsocket.net/
