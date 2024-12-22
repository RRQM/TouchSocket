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
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// 基于Tcp协议的JsonRpc功能插件
    /// </summary>
    [PluginOption(Singleton = true)]
    public sealed class TcpJsonRpcParserPlugin : JsonRpcParserPluginBase, ITcpConnectedPlugin, ITcpReceivedPlugin
    {
        /// <summary>
        /// 基于Tcp协议的JsonRpc功能插件
        /// </summary>
        /// <param name="rpcServerProvider"></param>
        public TcpJsonRpcParserPlugin(IRpcServerProvider rpcServerProvider) : base(rpcServerProvider)
        {
            this.AllowJsonRpc = (client) =>
            {
                return Task.FromResult(true);
            };
        }

        /// <summary>
        /// 经过判断是否标识当前的客户端为JsonRpc
        /// </summary>
        /// <param name="allowJsonRpc"></param>
        /// <returns></returns>
        public TcpJsonRpcParserPlugin SetAllowJsonRpc(Func<ITcpSession, Task<bool>> allowJsonRpc)
        {
            this.AllowJsonRpc = allowJsonRpc;
            return this;
        }

        /// <summary>
        /// 经过判断是否标识当前的客户端为JsonRpc
        /// </summary>
        /// <param name="allowJsonRpc"></param>
        /// <returns></returns>
        public TcpJsonRpcParserPlugin SetAllowJsonRpc(Func<ITcpSession, bool> allowJsonRpc)
        {
            this.AllowJsonRpc = (client) =>
            {
                return Task.FromResult(allowJsonRpc(client));
            };
            return this;
        }



        /// <summary>
        /// 经过判断是否标识当前的客户端为JsonRpc
        /// </summary>
        public Func<ITcpSession, Task<bool>> AllowJsonRpc { get; set; }

        /// <summary>
        /// 自动转换协议
        /// </summary>
        [Obsolete("此配置已被弃用，如果需要筛选客户端，请使用SetAllowJsonRpc方法实现", true)]
        public bool AutoSwitch { get; set; } = true;

        /// <summary>
        /// 不需要自动转化协议。
        /// <para>仅当服务器是Tcp时生效。才会解释为jsonRpc。</para>
        /// </summary>
        /// <returns></returns>
        [Obsolete("此配置已被弃用，如果需要筛选客户端，请使用SetAllowJsonRpc方法实现", true)]
        public TcpJsonRpcParserPlugin NoSwitchProtocol()
        {
            this.AutoSwitch = false;
            return this;
        }

        /// <inheritdoc/>
        public async Task OnTcpConnected(ITcpSession client, ConnectedEventArgs e)
        {
            if (client is IClientSender clientSender)
            {
                if (this.AllowJsonRpc != null)
                {
                    if (await this.AllowJsonRpc.Invoke(client).ConfigureAwait(false))
                    {
                        var jsonRpcActor = new JsonRpcActor()
                        {
                            SerializerConverter = this.SerializerConverter,
                            Resolver = client.Resolver,
                            SendAction = (data) => clientSender.SendAsync(data),
                            Logger = client.Logger
                        };

                        jsonRpcActor.SetRpcServerProvider(this.RpcServerProvider, this.ActionMap);
                        client.SetValue(JsonRpcClientExtension.JsonRpcActorProperty, jsonRpcActor);
                    }
                }
            }

            await e.InvokeNext().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
        {
            if (client.TryGetValue(JsonRpcClientExtension.JsonRpcActorProperty, out var jsonRpcActor))
            {
                var jsonRpcMemory = ReadOnlyMemory<byte>.Empty;
                if (e.RequestInfo is IJsonRpcRequestInfo requestInfo)
                {
                    jsonRpcMemory = requestInfo.GetJsonRpcMemory();
                }
                else if (e.RequestInfo is JsonPackage jsonPackage)
                {
                    jsonRpcMemory = jsonPackage.Data;
                }
                else if (e.ByteBlock != null)
                {
                    jsonRpcMemory = e.ByteBlock.Memory;
                }

                if (jsonRpcMemory.IsEmpty)
                {
                    return;
                }
                await jsonRpcActor.InputReceiveAsync(jsonRpcMemory, new TcpJsonRpcCallContext(client)).ConfigureAwait(false);
                return;
            }
            await e.InvokeNext().ConfigureAwait(false);
        }
    }
}