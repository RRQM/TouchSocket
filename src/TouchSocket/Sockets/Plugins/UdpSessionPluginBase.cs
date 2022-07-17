//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using TouchSocket.Core;
using TouchSocket.Core.Log;
using TouchSocket.Core.Plugins;

namespace TouchSocket.Sockets.Plugins
{
    /// <summary>
    /// Udp插件实现类
    /// </summary>
    public class UdpSessionPluginBase : DisposableObject, IUdpSessionPlugin
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ILog Logger { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPluginsManager PluginsManager { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void IUdpSessionPlugin.OnReceivedData(IUdpSession client, UdpReceivedDataEventArgs e)
        {
            this.OnReceivedData(client, e);
        }

        /// <summary>
        /// 收到数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnReceivedData(IUdpSession client, UdpReceivedDataEventArgs e)
        {

        }
    }
}
