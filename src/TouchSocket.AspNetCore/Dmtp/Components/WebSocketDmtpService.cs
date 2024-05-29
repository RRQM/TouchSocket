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

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp.AspNetCore
{
    /// <summary>
    /// WebSocketDmtpService
    /// </summary>
    public class WebSocketDmtpService : ConnectableService<WebSocketDmtpSessionClient>, IWebSocketDmtpService
    {
        /// <summary>
        /// 获取默认新Id。
        /// </summary>
        public Func<string> GetDefaultNewId { get; private set; }

        /// <inheritdoc/>
        public override IEnumerable<string> GetIds()
        {
            return this.m_clients.Keys;
        }

        /// <inheritdoc/>
        public override async Task ResetIdAsync(string sourceId, string targetId)
        {
            if (string.IsNullOrEmpty(sourceId))
            {
                throw new ArgumentException($"“{nameof(sourceId)}”不能为 null 或空。", nameof(sourceId));
            }

            if (string.IsNullOrEmpty(targetId))
            {
                throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
            }

            if (sourceId == targetId)
            {
                return;
            }
            if (this.m_clients.TryGetValue(sourceId, out var socketClient))
            {
                await socketClient.ResetIdAsync(targetId).ConfigureFalseAwait();
            }
            else
            {
                throw new ClientNotFindException(TouchSocketAspNetCoreResource.ClientNotFind.GetDescription(sourceId));
            }
        }

        /// <inheritdoc/>
        public override bool ClientExists(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            if (this.m_clients.ContainsKey(id))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 从WebSocket获取新客户端。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task SwitchClientAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureFalseAwait();
                var id = this.GetDefaultNewId();
                var client = new WebSocketDmtpSessionClient();
                if (!this.m_clients.TryAdd(id, client))
                {
                    throw new Exception("Id重复");
                }
                client.InternalSetService(this);
                client.InternalSetConfig(this.Config);
                client.InternalSetContainer(this.Resolver);
                client.InternalSetId(id);
                client.InternalSetPluginManager(this.PluginManager);
                client.SetDmtpActor(this.CreateDmtpActor(client));
                await client.Start(webSocket, context).ConfigureFalseAwait();
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }

        #region Fields

        private readonly InternalClientCollection<WebSocketDmtpSessionClient> m_clients = new InternalClientCollection<WebSocketDmtpSessionClient>();

        private long m_idCount;
        private int m_maxCount;
        private ServerState m_serverState;

        #endregion Fields

        #region 属性

        /// <inheritdoc/>
        public override int Count => this.m_clients.Count;

        /// <inheritdoc/>
        public override ServerState ServerState => this.m_serverState;

        /// <inheritdoc/>
        public string VerifyToken => this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyToken;

        public override IClientCollection<WebSocketDmtpSessionClient> Clients => this.m_clients;

        #endregion 属性

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            base.LoadConfig(config);
            if (config.GetValue(TouchSocketConfigExtension.GetDefaultNewIdProperty) is Func<string> fun)
            {
                this.GetDefaultNewId = fun;
            }
            else
            {
                this.GetDefaultNewId = () => { return Interlocked.Increment(ref this.m_idCount).ToString(); };
            }
        }

        private DmtpActor CreateDmtpActor(WebSocketDmtpSessionClient client)
        {
            return new SealedDmtpActor(true)
            {
                FindDmtpActor = this.OnServiceFindDmtpActor,
                Id = client.Id
            };
        }

        private Task<IDmtpActor> OnServiceFindDmtpActor(string id)
        {
            if (this.TryGetClient(id, out var client))
            {
                return Task.FromResult(client.DmtpActor);
            }
            return Task.FromResult<IDmtpActor>(default);
        }

        #region override

        /// <inheritdoc/>
        public override async Task ClearAsync()
        {
            foreach (var id in this.GetIds())
            {
                if (this.TryGetClient(id, out var client))
                {
                    await client.CloseAsync().ConfigureFalseAwait();
                    client.SafeDispose();
                }
            }
        }

        /// <inheritdoc/>
        public override Task StartAsync()
        {
            throw new NotSupportedException("此服务的生命周期跟随主Host");
        }

        /// <inheritdoc/>
        public override Task StopAsync()
        {
            throw new NotSupportedException("此服务的生命周期跟随主Host");
        }

        /// <inheritdoc/>
        protected override WebSocketDmtpSessionClient NewClient()
        {
            throw new NotImplementedException();
        }

        #endregion override

        #region internal

        internal bool TryAdd(string id, WebSocketDmtpSessionClient socketClient)
        {
            return this.m_clients.TryAdd(id, socketClient);
        }

        internal bool TryRemove(string id, out WebSocketDmtpSessionClient socketClient)
        {
            return this.m_clients.TryRemove(id, out socketClient);
        }

        #endregion internal
    }
}