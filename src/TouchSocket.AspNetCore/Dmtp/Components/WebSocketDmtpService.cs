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
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp.AspNetCore
{

    /// <summary>
    /// WebSocketDmtpService 类，继承自 ConnectableService，并实现 IWebSocketDmtpService 接口。
    /// 该类用于处理WebSocket的连接和服务。
    /// </summary>
    public class WebSocketDmtpService : ConnectableService<WebSocketDmtpSessionClient>, IWebSocketDmtpService
    {
        /// <summary>
        /// 构造函数，初始化服务器状态为运行中。
        /// </summary>
        public WebSocketDmtpService()
        {
            this.m_serverState = ServerState.Running;
        }

        /// <inheritdoc/>
        /// <summary>
        /// 获取所有客户端的标识符。
        /// </summary>
        /// <returns>客户端标识符的集合。</returns>
        public override IEnumerable<string> GetIds()
        {
            return this.m_clients.Keys;
        }

        /// <inheritdoc/>
        /// <summary>
        /// 异步重置客户端标识符。
        /// </summary>
        /// <param name="sourceId">源客户端标识符。</param>
        /// <param name="targetId">目标客户端标识符。</param>
        /// <exception cref="ArgumentException">抛出异常，如果sourceId或targetId为空。</exception>
        /// <exception cref="ClientNotFindException">抛出异常，如果源客户端不存在。</exception>
        public override async Task ResetIdAsync(string sourceId, string targetId)
        {
            ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(sourceId, nameof(sourceId));
            ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(targetId, nameof(targetId));
            
            if (sourceId == targetId)
            {
                return;
            }
            if (this.m_clients.TryGetValue(sourceId, out var sessionClient))
            {
                await sessionClient.ResetIdAsync(targetId).ConfigureAwait(false);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketAspNetCoreResource.ClientNotFind.GetDescription(sourceId));
            }
        }

        /// <inheritdoc/>
        /// <summary>
        /// 检查指定标识符的客户端是否存在。
        /// </summary>
        /// <param name="id">要检查的客户端标识符。</param>
        /// <returns>如果客户端存在返回true，否则返回false。</returns>
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
        /// <param name="context">HTTP上下文对象。</param>
        /// <returns>异步任务。</returns>
        public async Task SwitchClientAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
                var id = this.GetNextNewId();
                var client = new WebSocketDmtpSessionClient();
                if (!this.m_clients.TryAdd(id, client))
                {
                    // 如果添加失败，抛出异常，提示该Id已经存在。
                    ThrowHelper.ThrowException(TouchSocketResource.IdAlreadyExists.Format(id));
                }
                client.InternalSetService(this);
                client.InternalSetConfig(this.Config);
                client.InternalSetContainer(this.Resolver);
                client.InternalSetId(id);
                client.InternalSetPluginManager(this.PluginManager);
                client.SetDmtpActor(this.CreateDmtpActor(client));
                await client.Start(webSocket, context).ConfigureAwait(false);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }

        #region Fields

        /// <summary>
        /// 客户端集合，用于存储和管理WebSocket客户端。
        /// </summary>
        private readonly InternalClientCollection<WebSocketDmtpSessionClient> m_clients = new InternalClientCollection<WebSocketDmtpSessionClient>();

        /// <summary>
        /// 服务器状态。
        /// </summary>
        private readonly ServerState m_serverState;

        #endregion Fields

        #region 属性

        /// <inheritdoc/>
        /// <summary>
        /// 获取客户端数量。
        /// </summary>
        public override int Count => this.m_clients.Count;

        /// <inheritdoc/>
        /// <summary>
        /// 获取服务器状态。
        /// </summary>
        public override ServerState ServerState => this.m_serverState;

        /// <inheritdoc/>
        /// <summary>
        /// 获取验证令牌。
        /// </summary>
        public string VerifyToken => this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyToken;

        /// <inheritdoc/>
        /// <summary>
        /// 获取客户端集合。
        /// </summary>
        public override IClientCollection<WebSocketDmtpSessionClient> Clients => this.m_clients;

        #endregion 属性

        /// <summary>
        /// 创建DmtpActor对象。
        /// </summary>
        /// <param name="client">关联的WebSocketDmtpSessionClient对象。</param>
        /// <returns>SealedDmtpActor对象。</returns>
        private SealedDmtpActor CreateDmtpActor(WebSocketDmtpSessionClient client)
        {
            return new SealedDmtpActor(true)
            {
                FindDmtpActor = this.OnServiceFindDmtpActor,
                Id = client.Id
            };
        }

        /// <summary>
        /// 服务查找DmtpActor的方法。
        /// </summary>
        /// <param name="id">DmtpActor的标识符。</param>
        /// <returns>DmtpActor对象或默认值。</returns>
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
        /// <summary>
        /// 异步清除所有客户端。
        /// </summary>
        /// <returns>异步任务。</returns>
        public override async Task ClearAsync()
        {
            foreach (var id in this.GetIds())
            {
                if (this.TryGetClient(id, out var client))
                {
                    await client.CloseAsync().ConfigureAwait(false);
                    client.SafeDispose();
                }
            }
        }

        /// <inheritdoc/>
        /// <summary>
        /// 异步启动服务。该操作不支持，并抛出异常。
        /// </summary>
        /// <returns>异步任务。</returns>
        /// <exception cref="NotSupportedException">抛出不支持异常。</exception>
        public override Task StartAsync()
        {
            throw new NotSupportedException("此服务的生命周期跟随主Host");
        }

        /// <inheritdoc/>
        /// <summary>
        /// 异步停止服务。该操作不支持，并抛出异常。
        /// </summary>
        /// <returns>异步任务。</returns>
        /// <exception cref="NotSupportedException">抛出不支持异常。</exception>
        public override Task StopAsync()
        {
            throw new NotSupportedException("此服务的生命周期跟随主Host");
        }

        /// <inheritdoc/>
        /// <summary>
        /// 创建新的WebSocketDmtpSessionClient对象。该操作未实现。
        /// </summary>
        /// <returns>新的WebSocketDmtpSessionClient对象。</returns>
        /// <exception cref="NotImplementedException">抛出未实现异常。</exception>
        protected override WebSocketDmtpSessionClient NewClient()
        {
            throw new NotImplementedException();
        }

        #endregion override

        #region internal

        /// <summary>
        /// 尝试添加客户端到集合中。
        /// </summary>
        /// <param name="id">客户端标识符。</param>
        /// <param name="sessionClient">WebSocketDmtpSessionClient对象。</param>
        /// <returns>如果添加成功返回true，否则返回false。</returns>
        internal bool TryAdd(string id, WebSocketDmtpSessionClient sessionClient)
        {
            return this.m_clients.TryAdd(id, sessionClient);
        }

        /// <summary>
        /// 尝试从集合中移除客户端。
        /// </summary>
        /// <param name="id">客户端标识符。</param>
        /// <param name="sessionClient">移除的WebSocketDmtpSessionClient对象。</param>
        /// <returns>如果移除成功返回true，否则返回false。</returns>
        internal bool TryRemove(string id, out WebSocketDmtpSessionClient sessionClient)
        {
            return this.m_clients.TryRemoveClient(id, out sessionClient);
        }

        #endregion internal
    }
}