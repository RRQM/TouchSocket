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

using System.ComponentModel;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Mcp;
using TouchSocket.Rpc;

namespace McpServerConsoleApp;

internal class Program
{
    static async Task Main(string[] args)
    {
        await CreateHttpMcpServiceAsync();
        Console.ReadKey();
    }

    static async Task CreateHttpMcpServiceAsync()
    {
        #region Mcp创建Http服务器
        var service = new HttpService();

        await service.SetupAsync(new TouchSocketConfig()
            .SetListenIPHosts(7789)
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
                a.AddRpcStore(store =>
                {
                    store.RegisterServer<MyMcpService>();
                });
            })
            .ConfigurePlugins(a =>
            {
                a.UseMcpHttpPlugin(options =>
                {
                    options.Path = "/mcp";
                    options.ServerOptions.ServerInfo = new McpImplementationInfo
                    {
                        Name = "MyMcpServer",
                        Version = "1.0.0"
                    };
                });
            }));

        await service.StartAsync();
        #endregion

        service.Logger.Info("Mcp Http服务器已启动，地址：http://127.0.0.1:7789/mcp");
    }

    static async Task CreateStdioMcpServerAsync()
    {
        #region Mcp创建stdio服务器
        var server = new McpStdioServer();

        await server.SetupAsync(new TouchSocketConfig()
            .ConfigureContainer(a =>
            {
                a.AddRpcStore(store =>
                {
                    store.RegisterServer<MyMcpService>();
                });
            }));

        await server.StartAsync();

        await Task.Delay(Timeout.Infinite);
        #endregion
    }
}

#region Mcp定义服务
/// <summary>
/// MCP 服务示例，包含工具、资源和提示模板的定义。
/// </summary>
public class MyMcpService : SingletonRpcServer
{
    /// <summary>
    /// 计算两个整数之和。
    /// </summary>
    [McpTool]
    [Description("计算两个整数之和")]
    public int Add(
        [Description("第一个整数")] int a,
        [Description("第二个整数")] int b)
    {
        return a + b;
    }

    /// <summary>
    /// 获取当前服务器时间。
    /// </summary>
    [McpTool]
    [Description("获取当前服务器的日期和时间")]
    public string GetServerTime()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// 获取服务器基本信息资源（精确 URI）。
    /// </summary>
    [McpResource("config://server/info")]
    [Description("获取服务器基本信息")]
    public string GetServerInfo()
    {
        return "TouchSocket MCP Server 1.0";
    }

    /// <summary>
    /// 根据键名获取配置项（URI 模板）。
    /// </summary>
    [McpResource("config://server/{key}")]
    [Description("根据键名获取服务器配置项")]
    public string GetConfig(
        [Description("配置键名，例如：version、author")] string key)
    {
        var configs = new Dictionary<string, string>
        {
            ["version"] = "1.0.0",
            ["author"] = "TouchSocket",
            ["license"] = "MIT"
        };

        return configs.TryGetValue(key, out var value) ? value : $"未找到键 '{key}' 对应的配置项";
    }

    /// <summary>
    /// 生成代码审查提示模板。
    /// </summary>
    [McpPrompt]
    [Description("生成用于代码审查的提示消息")]
    public McpPromptMessage[] CodeReview(
        [Description("编程语言，例如：csharp、python、javascript")] string language,
        [Description("需要审查的代码内容")] string code)
    {
        return
        [
            new McpPromptMessage
            {
                Role = "user",
                Content = new McpTextContent
                {
                    Text = $"请审查以下 {language} 代码，指出潜在问题并提供改进建议：\n\n```{language}\n{code}\n```"
                }
            }
        ];
    }
}
#endregion
