//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Run;
using RRQMSocket.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.Helper;

namespace RRQMSocket.WebSocket
{
    /// <summary>
    /// WebSocket服务器
    /// </summary>
    public class WSService<TClient> : TcpService<TClient> where TClient:WSSocketClient,new () 
    {
        

        private static byte[] GetResponse(HttpRequest httpRequest)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("HTTP/1.1 101 Switching Protocols");
            stringBuilder.AppendLine("Connection: Upgrade");
            stringBuilder.AppendLine("Upgrade: websocket");
            stringBuilder.AppendLine($"Sec-WebSocket-Accept: {WSTools.CalculateBase64Key(httpRequest.GetHeader("Sec-WebSocket-Key"),httpRequest.Encoding)}");
            stringBuilder.AppendLine();
            return httpRequest.Encoding.GetBytes(stringBuilder.ToString());
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        protected override void PreviewConnecting(TClient socketClient)
        {
            Task.Run(() =>
            {
                WaitData<byte[]> waitData = new WaitData<byte[]>();
                Task.Run(() =>
                {
                    byte[] buffer = new byte[1024];
                    int r = socketClient.MainSocket.Receive(buffer);
                    if (r > 0)
                    {
                        byte[] data = new byte[r];

                        Array.Copy(buffer, data, r);
                        waitData.Set(data);
                    }
                });

                switch (waitData.Wait(3000))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            byte[] data = waitData.WaitResult;
                            Http.HttpRequest httpRequest = new Http.HttpRequest();
                            httpRequest.ReadHeaders(data, 0, data.Length);
                            if (httpRequest.GetHeader("Upgrade").ToLower() != "websocket")
                            {
                                return;
                            }
                            if (httpRequest.GetHeader("Connection").ToLower() != "upgrade")
                            {
                                return;
                            }

                            if (string.IsNullOrEmpty(httpRequest.GetHeader("Sec-WebSocket-Key")))
                            {
                                return;
                            }
                            socketClient.SetValue(WebSocketDataHandlingAdapter.WebSocketVersionProperty, httpRequest.GetHeader("Sec-WebSocket-Version"));
                            ClientOperationEventArgs clientArgs = new ClientOperationEventArgs();
                            clientArgs.ID = GetDefaultNewID();
                            this.OnConnecting(socketClient, clientArgs);
                            if (clientArgs.IsPermitOperation)
                            {
                                MakeClientReceive(socketClient, clientArgs.ID);
                                socketClient.MainSocket.Send(GetResponse(httpRequest));

                                this.OnConnected(socketClient, new MesEventArgs("新客户端连接"));
                            }
                            else
                            {
                                socketClient.MainSocket.Dispose();
                            }
                        }
                        break;

                    case WaitDataStatus.Overtime:
                    case WaitDataStatus.Canceled:
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            socketClient.Dispose();
                            break;
                        }
                }
            });
        }
    }
}
