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
    /// 监听配置
    /// </summary>
    public class TcpListenOption
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 监听地址
        /// </summary>
        public IPHost IpHost { get; set; }

        /// <summary>
        /// 发送超时时间
        /// </summary>
        public int SendTimeout { get; set; }

        /// <summary>
        /// 是否使用地址复用
        /// </summary>
        public bool ReuseAddress { get; set; }

        /// <summary>
        /// Tcp处理并发连接时最大半连接队列
        /// </summary>
        public int Backlog { get; set; } = 100;

        /// <summary>
        /// 禁用延迟发送
        /// </summary>
        public bool? NoDelay { get; set; }

        /// <summary>
        /// 是否使用ssl加密
        /// </summary>
        public bool UseSsl => this.ServiceSslOption != null;

        /// <summary>
        /// 用于Ssl加密的证书
        /// </summary>
        public ServiceSslOption ServiceSslOption { get; set; }

        /// <summary>
        /// 配置Tcp适配器
        /// </summary>
        public Func<SingleStreamDataHandlingAdapter> Adapter { get; set; } =
            () => new NormalDataHandlingAdapter();
    }
}