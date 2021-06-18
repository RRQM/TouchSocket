//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Dependency;
using System.Net;

namespace RRQMSocket
{
    /// <summary>
    /// UDP服务器配置
    /// </summary>
    public class UdpSessionConfig : ServerConfig
    {
        /// <summary>
        /// 默认远程节点
        /// </summary>
        public EndPoint DefaultRemotePoint
        {
            get { return (EndPoint)GetValue(DefaultRemotePointProperty); }
            set { SetValue(DefaultRemotePointProperty, value); }
        }

        /// <summary>
        /// 默认远程节点, 所需类型<see cref="EndPoint"/>
        /// </summary>
        public static readonly DependencyProperty DefaultRemotePointProperty =
            DependencyProperty.Register("DefaultRemotePoint", typeof(EndPoint), typeof(UdpSessionConfig), null);

        /// <summary>
        /// 使用绑定
        /// </summary>
        public bool UseBind
        {
            get { return (bool)GetValue(UseBindProperty); }
            set { SetValue(UseBindProperty, value); }
        }

        /// <summary>
        /// 使用绑定, 所需类型<see cref="bool"/>
        /// </summary>
        public static readonly DependencyProperty UseBindProperty =
            DependencyProperty.Register("UseBind", typeof(bool), typeof(UdpSessionConfig), false);
    }
}