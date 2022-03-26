//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMSocket.Http;
using RRQMSocket.Http.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.WebSocket.Plugins
{
    /// <summary>
    /// WS插件基类
    /// </summary>
    public class WebSocketPluginBase : HttpPluginBase, IWebSocketPlugin
    {

        /// <summary>
        /// 表示在即将握手连接时。
        /// <para>在此处拒绝操作，则会返回403 Forbidden。</para>
        /// <para>也可以向<see cref="HttpContextEventArgs.Response"/>注入更多信息。</para>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnHandshaking(ITcpClientBase client, HttpContextEventArgs e)
        {

        }

        /// <summary>
        /// 表示完成握手后。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnHandshaked(ITcpClientBase client, HttpContextEventArgs e)
        {

        }


        /// <summary>
        /// 处理WS数据帧。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnHandleWSDataFrame(ITcpClientBase client, WSDataFrameEventArgs e)
        {

        }

        void IWebSocketPlugin.OnHandshaking(ITcpClientBase client, HttpContextEventArgs e)
        {
            this.OnHandshaking(client, e);
        }

        void IWebSocketPlugin.OnHandshaked(ITcpClientBase client, HttpContextEventArgs e)
        {
            this.OnHandshaked(client, e);
        }

        void IWebSocketPlugin.OnHandleWSDataFrame(ITcpClientBase client, WSDataFrameEventArgs e)
        {
            this.OnHandleWSDataFrame(client, e);
        }
    }
}
