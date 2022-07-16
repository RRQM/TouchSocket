using System.Threading.Tasks;
using TouchSocket.Core.ByteManager;
using TouchSocket.Sockets;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// WebSocketServerExtensions
    /// </summary>
    public static class WebSocketServerExtensions
    {
        /// <summary>
        /// 转化Protocol协议标识为<see cref="Protocol.WebSocket"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="httpContext">Http上下文</param>
        public static bool SwitchProtocolToWebSocket<TClient>(this TClient client, HttpContext httpContext) where TClient : HttpSocketClient
        {
            if (client.Protocol == Protocol.WebSocket)
            {
                return true;
            }
            if (client.Protocol == Protocol.Http)
            {
                if (WSTools.TryGetResponse(httpContext.Request, httpContext.Response))
                {
                    HttpContextEventArgs args = new HttpContextEventArgs(new HttpContext(httpContext.Request, httpContext.Response))
                    {
                        IsPermitOperation = true
                    };
                    client.PluginsManager?.Raise<IWebSocketPlugin>("OnHandshaking", client, args);

                    if (args.IsPermitOperation)
                    {
                        client.SetDataHandlingAdapter(new WebSocketDataHandlingAdapter() { MaxPackageSize = client.MaxPackageSize });
                        client.Protocol = Protocol.WebSocket;
                        client.SetValue(WebSocketServerPlugin.HandshakedProperty, true);//设置握手状态

                        using (ByteBlock byteBlock = new ByteBlock())
                        {
                            args.Context.Response.Build(byteBlock);
                            client.DefaultSend(byteBlock);
                        }
                        client.PluginsManager?.Raise<IWebSocketPlugin>("OnHandshaked", client, new HttpContextEventArgs(httpContext));
                        return true;
                    }
                    else
                    {
                        args.Context.Response.SetStatus("403", "Forbidden");
                        using (ByteBlock byteBlock = new ByteBlock())
                        {
                            args.Context.Response.Build(byteBlock);
                            client.DefaultSend(byteBlock);
                        }

                        client.Close("主动拒绝WebSocket连接");
                    }
                }
                else
                {
                    client.Close("WebSocket连接协议不正确");
                }
            }
            return false;
        }

        /// <summary>
        /// 转化Protocol协议标识为<see cref="Protocol.WebSocket"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="httpContext">Http上下文</param>
        public static Task<bool> SwitchProtocolToWebSocketAsync<TClient>(this TClient client, HttpContext httpContext) where TClient : HttpSocketClient
        {
            return Task.Run(() =>
             {
                 return SwitchProtocolToWebSocket(client, httpContext);
             });
        }
    }
}