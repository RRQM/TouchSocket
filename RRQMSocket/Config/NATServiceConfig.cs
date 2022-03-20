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
    /// 端口转发配置
    /// </summary>
    public class NATServiceConfig : TcpServiceConfig
    {
        /// <summary>
        /// 转发的目标地址集合。
        /// </summary>
        public IPHost[] TargetIPHosts
        {
            get { return (IPHost[])this.GetValue(TargetIPHostsProperty); }
            set { this.SetValue(TargetIPHostsProperty, value); }
        }

        /// <summary>
        /// 转发的目标地址集合，
        /// 所需类型<see cref="IPHost"/>数组
        /// </summary>
        public static readonly DependencyProperty TargetIPHostsProperty =
            DependencyProperty.Register("TargetIPHosts", typeof(IPHost[]), typeof(NATServiceConfig), null);


        /// <summary>
        /// 转发的类型
        /// </summary>
        public NATMode NATMode
        {
            get { return (NATMode)this.GetValue(NATModeProperty); }
            set { this.SetValue(NATModeProperty, value); }
        }

        /// <summary>
        /// 转发的类型，
        /// 所需类型<see cref="RRQMSocket.NATMode"/>
        /// </summary>
        public static readonly DependencyProperty NATModeProperty =
            DependencyProperty.Register("NATMode", typeof(NATMode), typeof(NATServiceConfig), NATMode.TwoWay);


    }
}