//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
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
    public class WebSocketDmtpService : DisposableObject, IWebSocketDmtpService
    {
        #region SocketClient

        private readonly ConcurrentDictionary<string, WebSocketDmtpSocketClient> m_socketClients = new ConcurrentDictionary<string, WebSocketDmtpSocketClient>();

        /// <summary>
        /// 数量
        /// </summary>
        public int Count => this.m_socketClients.Count;

        /// <summary>
        /// 获取SocketClient
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WebSocketDmtpSocketClient this[string id]
        {
            get
            {
                this.TryGetSocketClient(id, out var t);
                return t;
            }
        }

        /// <summary>
        /// 获取所有的客户端
        /// </summary>
        /// <returns></returns>
        public IEnumerable<WebSocketDmtpSocketClient> GetClients()
        {
            return this.m_socketClients.Values;
        }

        /// <summary>
        /// 获取Id集合
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetIds()
        {
            return this.m_socketClients.Keys;
        }

        /// <summary>
        /// 根据Id判断SocketClient是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool SocketClientExist(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            if (this.m_socketClients.ContainsKey(id))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 尝试获取实例
        /// </summary>
        /// <param name="id"></param>
        /// <param name="socketClient"></param>
        /// <returns></returns>
        public bool TryGetSocketClient(string id, out WebSocketDmtpSocketClient socketClient)
        {
            if (string.IsNullOrEmpty(id))
            {
                socketClient = null;
                return false;
            }

            return this.m_socketClients.TryGetValue(id, out socketClient);
        }

        internal bool TryAdd(string id, WebSocketDmtpSocketClient socketClient)
        {
            return this.m_socketClients.TryAdd(id, socketClient);
        }

        internal bool TryRemove(string id, out WebSocketDmtpSocketClient socketClient)
        {
            if (string.IsNullOrEmpty(id))
            {
                socketClient = null;
                return false;
            }
            return this.m_socketClients.TryRemove(id, out socketClient);
        }

        #endregion SocketClient

        private long m_idCount;

        /// <summary>
        /// 创建一个基于WebSocket的Dmtp服务器。
        /// </summary>
        /// <param name="config"></param>
        public WebSocketDmtpService(TouchSocketConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            this.ThrowIfDisposed();

            this.BuildConfig(config);

            this.PluginsManager.Raise(nameof(ILoadingConfigPlugin.OnLoadingConfig), this, new ConfigEventArgs(config));
            this.LoadConfig(this.Config);
            this.PluginsManager.Raise(nameof(ILoadedConfigPlugin.OnLoadedConfig), this, new ConfigEventArgs(config));

            this.VerifyToken = config.GetValue(DmtpConfigExtension.VerifyTokenProperty);

            if (config.GetValue(TouchSocketConfigExtension.GetDefaultNewIdProperty) is Func<string> fun)
            {
                this.GetDefaultNewId = fun;
            }
            else
            {
                this.GetDefaultNewId = () => { return Interlocked.Increment(ref this.m_idCount).ToString(); };
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public TouchSocketConfig Config { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IContainer Container { get; private set; }

        /// <summary>
        /// 获取默认新Id。
        /// </summary>
        public Func<string> GetDefaultNewId { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPluginsManager PluginsManager { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string VerifyToken { get; private set; }

        /// <inheritdoc/>
        public void ResetId(string oldId, string newId)
        {
            if (string.IsNullOrEmpty(oldId))
            {
                throw new ArgumentException($"“{nameof(oldId)}”不能为 null 或空。", nameof(oldId));
            }

            if (string.IsNullOrEmpty(newId))
            {
                throw new ArgumentException($"“{nameof(newId)}”不能为 null 或空。", nameof(newId));
            }

            if (oldId == newId)
            {
                return;
            }
            if (this.m_socketClients.TryGetValue(oldId, out var socketClient))
            {
                socketClient.ResetId(newId);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketAspNetCoreResource.ClientNotFind.GetDescription(oldId));
            }
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
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var id = this.GetDefaultNewId();
                var client = new WebSocketDmtpSocketClient();
                if (!this.TryAdd(id, client))
                {
                    throw new Exception("Id重复");
                }
                client.InternalSetService(this);
                client.InternalSetConfig(this.Config);
                client.InternalSetContainer(this.Container);
                client.InternalSetId(id);
                client.InternalSetPluginsManager(this.PluginsManager);
                client.SetDmtpActor(this.CreateDmtpActor(client));
                await client.Start(webSocket, context);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="config"></param>
        protected virtual void LoadConfig(TouchSocketConfig config)
        {
        }

        private void BuildConfig(TouchSocketConfig config)
        {
            this.Config = config;

            if (!(config.GetValue(TouchSocketCoreConfigExtension.ContainerProperty) is IContainer container))
            {
                container = new Container();
            }

            if (!container.IsRegistered(typeof(ILog)))
            {
                container.RegisterSingleton<ILog, LoggerGroup>();
            }

            if (!(config.GetValue(TouchSocketCoreConfigExtension.PluginsManagerProperty) is IPluginsManager pluginsManager))
            {
                pluginsManager = new PluginsManager(container);
            }

            if (container.IsRegistered(typeof(IPluginsManager)))
            {
                pluginsManager = container.Resolve<IPluginsManager>();
            }
            else
            {
                container.RegisterSingleton<IPluginsManager>(pluginsManager);
            }

            if (config.GetValue(TouchSocketCoreConfigExtension.ConfigureContainerProperty) is Action<IContainer> actionContainer)
            {
                actionContainer.Invoke(container);
            }

            if (config.GetValue(TouchSocketCoreConfigExtension.ConfigurePluginsProperty) is Action<IPluginsManager> actionPluginsManager)
            {
                pluginsManager.Enable = true;
                actionPluginsManager.Invoke(pluginsManager);
            }
            this.Container = container;
            this.PluginsManager = pluginsManager;
        }

        private DmtpActor CreateDmtpActor(WebSocketDmtpSocketClient client)
        {
            return new SealedDmtpActor(true)
            {
                FindDmtpActor = this.OnServiceFindDmtpActor,
                Id = client.Id
            };
        }

        private Task<IDmtpActor> OnServiceFindDmtpActor(string id)
        {
            if (this.TryGetSocketClient(id, out var client))
            {
                return Task.FromResult(client.DmtpActor);
            }
            return Task.FromResult<IDmtpActor>(default);
        }
    }
}