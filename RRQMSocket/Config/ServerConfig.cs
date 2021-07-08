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

namespace RRQMSocket
{
    /// <summary>
    /// 服务器配置
    /// </summary>
    public class ServerConfig : RRQMConfig
    {
        /// <summary>
        /// 多线程数量
        /// </summary>
        public int ThreadCount
        {
            get { return (int)GetValue(ThreadCountProperty); }
            set { SetValue(ThreadCountProperty, value); }
        }

        /// <summary>
        /// 多线程数量依赖属性，所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty ThreadCountProperty =
            DependencyProperty.Register("ThreadCount", typeof(int), typeof(ServerConfig), 10);

        /// <summary>
        /// 监听IP和端口号组
        /// </summary>
        public IPHost[] ListenIPHosts
        {
            get { return (IPHost[])GetValue(ListenIPHostsProperty); }
            set { SetValue(ListenIPHostsProperty, value); }
        }

        /// <summary>
        /// IP和端口号依赖属性，所需类型<see cref="IPHost"/>数组
        /// </summary>
        public static readonly DependencyProperty ListenIPHostsProperty =
            DependencyProperty.Register("ListenIPHosts", typeof(IPHost[]), typeof(ServerConfig), null);

        /// <summary>
        /// 名称
        /// </summary>
        public string ServerName
        {
            get { return (string)GetValue(ServerNameProperty); }
            set { SetValue(ServerNameProperty, value); }
        }

        /// <summary>
        /// 名称，所需类型<see cref="string"/>
        /// </summary>
        public static readonly DependencyProperty ServerNameProperty =
            DependencyProperty.Register("ServerName", typeof(string), typeof(ServerConfig), "RRQMServer");

        /// <summary>
        /// 独立线程接收
        /// </summary>
        public bool SeparateThreadReceive
        {
            get { return (bool)GetValue(SeparateThreadReceiveProperty); }
            set { SetValue(SeparateThreadReceiveProperty, value); }
        }

        /// <summary>
        /// 独立线程接收，
        /// 所需类型<see cref="bool"/>
        /// </summary>
        public static readonly DependencyProperty SeparateThreadReceiveProperty =
            DependencyProperty.Register("SeparateThreadReceive", typeof(bool), typeof(ServerConfig), true);

    }
}