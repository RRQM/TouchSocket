# TouchSocket.Redis

TouchSocket.Redis 提供基于 RESP 协议的 Redis 客户端，以及基于内存的 Redis 兼容 TCP 服务端。

当前服务端已支持 `PING`、`ECHO`、`GET`、`SET`、`DEL`、`EXISTS`、`INCR`、`DECR`、`MGET`、`MSET`、`EXPIRE`、`TTL`、`KEYS`、`DBSIZE`、`FLUSHDB`、`SELECT` 和 `QUIT`。

协议写入尽量使用 `IBytesWriter` 与 `Span<byte>` 路径。接收侧 bulk string 仅在数据需要离开 adapter 接收缓冲区继续存活时复制。

## 客户端

```csharp
var client = new RedisClient();
await client.SetupAsync(new TouchSocketConfig()
    .SetRemoteIPHost("127.0.0.1:6379"));
await client.ConnectAsync();

await client.SetAsync("name", "TouchSocket");
var value = await client.GetStringAsync("name");
```

### 客户端认证
```csharp
var client = new RedisClient();
await client.SetupAsync(new TouchSocketConfig()
    .SetRemoteIPHost("127.0.0.1:6379")
    .SetRedisClientOption(option =>
    {
        // Redis 旧式 requirepass 只设置 Password。
        option.Password = "123456";

        // Redis 6+ ACL 需要用户名时再设置 UserName。
        // option.UserName = "default";
    }));
await client.ConnectAsync();
```

## 服务端

```csharp
var service = new RedisService();
await service.SetupAsync(new TouchSocketConfig()
    .SetListenIPHosts(6379));
await service.StartAsync();
```
