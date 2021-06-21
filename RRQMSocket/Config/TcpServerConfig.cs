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
    /// Tcp服务配置
    /// </summary>
    public class TcpServerConfig : ServerConfig
    {
        /// <summary>
        /// 挂起连接队列的最大长度。默认为100
        /// </summary>
        public int Backlog
        {
            get { return (int)GetValue(BacklogProperty); }
            set { SetValue(BacklogProperty, value); }
        }

        /// <summary>
        /// 挂起连接队列的最大长度，所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty BacklogProperty =
            DependencyProperty.Register("Backlog", typeof(int), typeof(TcpServerConfig), 100);

        /// <summary>
        /// 最大可连接数，默认为10000
        /// </summary>
        public int MaxCount
        {
            get { return (int)GetValue(MaxCountProperty); }
            set { SetValue(MaxCountProperty, value); }
        }

        /// <summary>
        /// 最大可连接数，默认为10000，所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty MaxCountProperty =
            DependencyProperty.Register("MaxCount", typeof(int), typeof(TcpServerConfig), 10000);

       
        /// <summary>
        /// 获取或设置清理无数据交互的SocketClient，默认60。如果不想清除，可使用-1。
        /// </summary>
        public int ClearInterval
        {
            get { return (int)GetValue(ClearIntervalProperty); }
            set { SetValue(ClearIntervalProperty, value); }
        }

        /// <summary>
        /// 获取或设置清理无数据交互的SocketClient，默认60。如果不想清除，可使用-1。
        /// 所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty ClearIntervalProperty =
            DependencyProperty.Register("ClearInterval", typeof(int), typeof(TcpServerConfig), 60);
    }
}