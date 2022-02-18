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
using RRQMCore.Dependency;

namespace RRQMSocket
{
    /// <summary>
    /// UDP服务器配置
    /// </summary>
    public class UdpSessionConfig : ServiceConfig
    {
        /// <summary>
        /// 远程IPHost
        /// </summary>
        public IPHost RemoteIPHost
        {
            get => (IPHost)this.GetValue(RemoteIPHostProperty);
            set => this.SetValue(RemoteIPHostProperty, value);
        }

        /// <summary>
        /// 远程IPHost，所需类型<see cref="IPHost"/>
        /// </summary>
        public static readonly DependencyProperty RemoteIPHostProperty =
            DependencyProperty.Register("RemoteIPHost", typeof(IPHost), typeof(TcpClientConfig), null);

        /// <summary>
        /// UDP绑定
        /// </summary>
        public IPHost BindIPHost
        {
            get => (IPHost)this.GetValue(BindIPHostProperty);
            set => this.SetValue(BindIPHostProperty, value);
        }

        /// <summary>
        /// UDP绑定值，
        /// 所需类型<see cref="IPHost"/>
        /// </summary>
        public static readonly DependencyProperty BindIPHostProperty =
            DependencyProperty.Register("BindIPHost", typeof(IPHost), typeof(UdpSessionConfig), null);
    }
}