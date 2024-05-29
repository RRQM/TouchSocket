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
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 适用于可连接客户端的连接工厂。
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public abstract class ConnectableClientFactory<TClient> : ClientFactory<TClient> where TClient : class, IClient, IConnectableClient
    {
        /// <summary>
        /// 连接超时设定
        /// </summary>
        public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// 获取传输的客户端配置
        /// </summary>
        public Func<TouchSocketConfig> GetConfig { get; set; }

        /// <summary>
        /// 获取配置。
        /// </summary>
        /// <returns></returns>
        protected virtual TouchSocketConfig OnGetConfig()
        {
            return this.GetConfig?.Invoke();
        }
    }
}