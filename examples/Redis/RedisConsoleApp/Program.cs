//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using TouchSocket.Core;
using TouchSocket.Redis;
using TouchSocket.Sockets;

namespace RedisConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var service = await CreateRedisService();
        var client = await CreateRedisClient();

        await StringOperations(client);
        await BatchOperations(client);
        await IncrDecrOperations(client);
        await KeyOperations(client);
        await ListOperations(client);
        await SortedSetOperations(client);
        await CustomCommandDemo(client);

        Console.ReadKey();
    }

    private static async Task<RedisService> CreateRedisService()
    {
        #region Redis创建服务端

        var service = new RedisService();
        await service.SetupAsync(new TouchSocketConfig()
            .SetListenIPHosts(6379)
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
            }));
        await service.StartAsync();
        service.Logger.Info("Redis服务端已启动，监听端口：6379");
        return service;

        #endregion Redis创建服务端
    }

    private static async Task<RedisService> CreateRedisServiceWithAuth()
    {
        #region Redis服务端认证配置

        var service = new RedisService();
        await service.SetupAsync(new TouchSocketConfig()
            .SetListenIPHosts(6380)
            .SetRedisServerOption(option =>
            {
                // 旧式 requirepass 密码认证
                option.Password = "123456";
                // Redis 6+ ACL 用户名认证
                // option.UserName = "default";
            })
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
            }));
        await service.StartAsync();
        return service;

        #endregion Redis服务端认证配置
    }

    private static async Task<RedisClient> CreateRedisClient()
    {
        #region Redis创建客户端

        var client = new RedisClient();
        await client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("127.0.0.1:6379")
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
            }));
        await client.ConnectAsync();
        client.Logger.Info("Redis客户端已连接");
        return client;

        #endregion Redis创建客户端
    }

    private static async Task<RedisClient> CreateRedisClientWithAuth()
    {
        #region Redis客户端认证

        var client = new RedisClient();
        await client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("127.0.0.1:6380")
            .SetRedisClientOption(option =>
            {
                // Redis 旧式 requirepass 只设置 Password
                option.Password = "123456";
                // Redis 6+ ACL 需要用户名时再设置 UserName
                // option.UserName = "default";
            })
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
            }));
        await client.ConnectAsync();
        return client;

        #endregion Redis客户端认证
    }

    private static async Task StringOperations(RedisClient client)
    {
        #region Redis字符串写入读取

        // 写入字符串
        await client.SetAsync("name", "TouchSocket");
        await client.StringSetAsync("version", "4.3.1");

        // 读取字符串
        var name = await client.GetStringAsync("name");
        var version = await client.StringGetAsync("version");

        client.Logger.Info($"name={name}, version={version}");

        // 以字节数组读取
        var bytes = await client.GetBytesAsync("name");
        client.Logger.Info($"name bytes length={bytes?.Length}");

        #endregion Redis字符串写入读取
    }

    private static async Task BatchOperations(RedisClient client)
    {
        #region Redis批量操作

        // 批量写入
        await client.MSetAsync(new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" },
            { "key3", "value3" }
        });

        // 批量读取
        var values = await client.MGetAsync(["key1", "key2", "key3"]);
        client.Logger.Info($"MGET: {string.Join(", ", values)}");

        #endregion Redis批量操作
    }

    private static async Task IncrDecrOperations(RedisClient client)
    {
        #region Redis自增自减

        await client.SetAsync("counter", "0");

        // INCR 自增1
        var newVal = await client.StringIncrementAsync("counter");
        client.Logger.Info($"INCR counter={newVal}");

        // INCRBY 自增指定值
        newVal = await client.StringIncrementAsync("counter", 5);
        client.Logger.Info($"INCRBY counter={newVal}");

        // DECR 自减1
        newVal = await client.StringDecrementAsync("counter");
        client.Logger.Info($"DECR counter={newVal}");

        // DECRBY 自减指定值
        newVal = await client.StringDecrementAsync("counter", 2);
        client.Logger.Info($"DECRBY counter={newVal}");

        #endregion Redis自增自减
    }

    private static async Task KeyOperations(RedisClient client)
    {
        #region Redis键操作

        await client.SetAsync("tempKey", "hello");

        // 判断键是否存在
        var existsCount = await client.ExistsAsync("tempKey");
        client.Logger.Info($"EXISTS tempKey={existsCount}");

        // 设置过期时间（秒）
        var expireResult = await client.ExpireAsync("tempKey", 60);
        client.Logger.Info($"EXPIRE tempKey={expireResult}");

        // 获取剩余生存时间（秒）
        var ttl = await client.TtlAsync("tempKey");
        client.Logger.Info($"TTL tempKey={ttl}s");

        // 获取匹配模式的键
        var keys = await client.KeysAsync("key*");
        client.Logger.Info($"KEYS key*: {string.Join(", ", keys)}");

        // 获取数据库键数量
        var dbSize = await client.DbSizeAsync();
        client.Logger.Info($"DBSIZE={dbSize}");

        // 删除键
        var delResult = await client.KeyDeleteAsync("tempKey");
        client.Logger.Info($"DEL tempKey={delResult}");

        // 批量删除键
        var delCount = await client.DelAsync("key1", "key2", "key3");
        client.Logger.Info($"DEL count={delCount}");

        // 清空当前数据库
        await client.FlushDbAsync();
        client.Logger.Info("FLUSHDB 完成");

        #endregion Redis键操作
    }

    private static async Task ListOperations(RedisClient client)
    {
        #region Redis列表操作

        // 向列表头部插入元素
        var len = await client.ListLeftPushAsync("mylist", "c", "b", "a");
        client.Logger.Info($"LPUSH mylist 长度={len}");

        // 向列表尾部追加元素
        len = await client.ListRightPushAsync("mylist", "d", "e");
        client.Logger.Info($"RPUSH mylist 长度={len}");

        // 获取列表长度
        var listLen = await client.ListLengthAsync("mylist");
        client.Logger.Info($"LLEN mylist={listLen}");

        // 获取列表范围（0到-1表示全部）
        var items = await client.ListRangeAsync("mylist", 0, -1);
        client.Logger.Info($"LRANGE mylist: {string.Join(", ", items)}");

        #endregion Redis列表操作
    }

    private static async Task SortedSetOperations(RedisClient client)
    {
        #region Redis有序集合操作

        // 添加有序集合成员
        await client.SortedSetAddAsync("ranking", "Alice", 100.0);
        await client.SortedSetAddAsync("ranking", "Bob", 85.0);
        await client.SortedSetAddAsync("ranking", "Charlie", 95.0);

        // 批量添加
        await client.SortedSetAddAsync("ranking", new Dictionary<string, double>
        {
            { "Dave", 78.0 },
            { "Eve", 92.0 }
        });

        // 获取有序集合长度
        var zcard = await client.SortedSetLengthAsync("ranking");
        client.Logger.Info($"ZCARD ranking={zcard}");

        // 删除成员
        var zrem = await client.SortedSetRemoveAsync("ranking", "Dave");
        client.Logger.Info($"ZREM ranking Dave={zrem}");

        #endregion Redis有序集合操作
    }

    private static async Task CustomCommandDemo(RedisClient client)
    {
        #region Redis执行自定义命令

        // 使用 ExecuteAsync 直接发送任意 Redis 命令
        var pong = await client.PingAsync();
        client.Logger.Info($"PING 响应: {pong.AsString()}");

        var echo = await client.EchoAsync("Hello TouchSocket");
        client.Logger.Info($"ECHO: {echo}");

        // 自定义命令：使用字符串参数方式
        var response = await client.ExecuteAsync("SET", "custom", "data");
        client.Logger.Info($"自定义 SET: {response.AsString()}");

        var getResponse = await client.ExecuteAsync("GET", "custom");
        client.Logger.Info($"自定义 GET: {getResponse.AsString()}");

        #endregion Redis执行自定义命令
    }

    private static async Task<RedisClient> CreateRedisClientWithReconnection()
    {
        #region Redis断线重连

        var client = new RedisClient();
        await client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("127.0.0.1:6379")
            .ConfigurePlugins(a =>
            {
                a.UseReconnection<RedisClient>(options =>
                {
                    // 固定间隔重连策略，每3秒尝试一次，最多重试5次（-1为无限）
                    options.UseSimple(TimeSpan.FromSeconds(3), maxRetryCount: 5);
                });
            })
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
            }));
        await client.ConnectAsync();
        return client;

        #endregion Redis断线重连
    }
}

/// <summary>
/// 展示预构建 RedisValue 命令以降低热路径分配的性能优化模式。
/// </summary>
internal class RedisPerformanceDemo
{
    #region Redis预构建命令降低分配

    // 在类字段中预构建命令对象，避免每次调用都分配新对象
    private static readonly RedisValue s_pingCommand = RedisValue.Command("PING");
    private static readonly RedisValue s_getCommand = RedisValue.Command("GET", "mykey");
    private static readonly RedisValue s_setCommand = RedisValue.Command("SET", "mykey", "myvalue");
    private static readonly RedisValue s_incrCommand = RedisValue.Command("INCR", "counter");

    public static async Task RunAsync(RedisClient client)
    {
        // 直接复用预构建命令，热路径零额外分配
        await client.ExecuteAsync(s_pingCommand);
        await client.ExecuteAsync(s_setCommand);
        await client.ExecuteAsync(s_getCommand);
        await client.ExecuteAsync(s_incrCommand);
    }

    #endregion Redis预构建命令降低分配
}