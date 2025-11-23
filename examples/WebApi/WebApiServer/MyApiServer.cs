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

using System;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.WebApi;

namespace WebApiServerApp;

public class MyApiServer : SingletonRpcServer
{
    private readonly ILog m_logger;

    public MyApiServer(ILog logger)
    {
        this.m_logger = logger;
    }

    #region WebApi调用上下文WebSocket升级

    [Router("/[api]/[action]")]
    [WebApi(Method = HttpMethodType.Get)]
    public async Task ConnectWS(IWebApiCallContext callContext)
    {
        if (callContext.Caller is HttpSessionClient sessionClient)
        {
            var result = await sessionClient.SwitchProtocolToWebSocketAsync(callContext.HttpContext);
            if (!result.IsSuccess)
            {
                Console.WriteLine(result.Message);
                return;
            }

            this.m_logger.Info("WS通过WebApi连接");
            var webSocket = sessionClient.WebSocket;

            webSocket.AllowAsyncRead = true;

            while (true)
            {
                using (var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
                {
                    using (var receiveResult = await webSocket.ReadAsync(tokenSource.Token))
                    {
                        if (receiveResult.IsCompleted)
                        {
                            //webSocket已断开
                            return;
                        }

                        //webSocket数据帧
                        var dataFrame = receiveResult.DataFrame;

                        //此处可以处理数据
                    }
                }

            }
        }
    }

    #endregion WebApi调用上下文WebSocket升级
}
