// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System.Numerics;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using UnityRpcProxy;
namespace UnityServerConsoleApp_2D.TouchServer;

/// <summary>
/// Web Socket
/// </summary>
public class Touch_JsonWebSocket_2D : BaseTouchServer
{
    private readonly JsonHttpService m_dmtpService = new JsonHttpService();
    public async Task StartService(int port)
    {
        var config = new TouchSocketConfig()//配置
            .SetListenIPHosts(port)

            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();//注册一个日志组

                //注册rpc服务
                a.AddRpcStore(store =>
                {
                    store.RegisterServer<UnityRpcStore>();
#if DEBUG
                    var code = store.GetProxyCodes("UnityRpcProxy_Json_HttpDmtp_2D", typeof(JsonRpcAttribute));
                    File.WriteAllText("../../../RPCStore/UnityRpcProxy_Json_HttpDmtp_2D.cs", code);
#endif
                });
            })
            .ConfigurePlugins(a =>
            {
                a.UseWebSocket()
                 .SetWSUrl("/ws");

                //启用json rpc插件
                a.UseWebSocketJsonRpc()
                .SetAllowJsonRpc((websocket, context) => true);//让所有请求WebSocket都加载JsonRpc插件


                a.Add<Touch_JsonWebSocket_Log_Plguin>();

            });

        await this.m_dmtpService.SetupAsync(config);
        await this.m_dmtpService.StartAsync();


        this.m_dmtpService.Logger.Info($"TCP_JsonWebSocket已启动，监听端口：{port}");
    }
}
/// <summary>
/// 状态日志打印插件
/// </summary>
internal class Touch_JsonWebSocket_Log_Plguin : PluginBase, IWebSocketHandshakedPlugin, IWebSocketClosedPlugin
{
    private readonly ILog m_log;
    public Touch_JsonWebSocket_Log_Plguin(ILog Log)
    {
        this.m_log = Log;

    }
    private static int s_iD;
    public async Task OnWebSocketClosed(IWebSocket webSocket, ClosedEventArgs e)
    {
        webSocket.Client.Logger.Info($"TCP_WebSocket:客户端{webSocket.Client.IP}已断开");
        if (webSocket.Client is JsonHttpSessionClient client)
        {
            this.m_log.Info("在线用户" + client.Service.Count);

            foreach (JsonHttpSessionClient clientItem in client.Service.GetClients())
            {
                //对已经在线的客户端通知他们有玩家退出
                if (clientItem != client)
                {
                    _ = clientItem.GetJsonRpcActionClient().OfflineAsync(client.ID);
                }

            }
        }
        await e.InvokeNext();
    }

    public async Task OnWebSocketHandshaked(IWebSocket webSocket, HttpContextEventArgs e)
    {

        if (webSocket.Client is JsonHttpSessionClient client)
        {
            this.m_log.Info($"TCP_WebSocket:客户端{webSocket.Client.IP}已连接");
            client.ID = ++s_iD;
            _ = Task.Run(async () =>
            {
                foreach (JsonHttpSessionClient clientItem in client.Service.GetClients())
                {
                    //对当前玩家返回已登陆玩家的数据
                    await client.GetJsonRpcActionClient().NewNPCAsync(clientItem.ID, clientItem.Postion);
                }
                foreach (JsonHttpSessionClient clientItem in client.Service.GetClients())
                {
                    //对已在线的玩家添加在线用户
                    await clientItem.GetJsonRpcActionClient().NewNPCAsync(client.ID, client.Postion);
                }
                await client.GetJsonRpcActionClient().PlayerLoginAsync(client.ID);
            });
            this.m_log.Info("在线用户" + client.Service.Count);
        }
        await e.InvokeNext();
    }
}

/// <summary>
/// 自定义HttpDmtpService
/// </summary>
internal class JsonHttpService : HttpDmtpService<JsonHttpSessionClient>
{
    protected override JsonHttpSessionClient NewClient()
    {
        return new JsonHttpSessionClient();

    }
}

/// <summary>
/// 自定义HttpDmtpSessionClient
/// </summary>
internal class JsonHttpSessionClient : HttpDmtpSessionClient
{
    public int ID { get; set; }
    /// <summary>
    /// 位置
    /// </summary>
    public Vector3 Postion { get; set; }

    public JsonHttpSessionClient()
    {


    }


}
