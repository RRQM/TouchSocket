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
        /// 多线程数量依赖属性
        /// </summary>
        public static readonly DependencyProperty ThreadCountProperty =
            DependencyProperty.Register("ThreadCount", typeof(int), typeof(ServerConfig), 10);

        /// <summary>
        /// IP和端口号
        /// </summary>
        public IPHost BindIPHost
        {
            get { return (IPHost)GetValue(BindIPHostProperty); }
            set { SetValue(BindIPHostProperty, value); }
        }

        /// <summary>
        /// IP和端口号依赖属性
        /// </summary>
        public static readonly DependencyProperty BindIPHostProperty =
            DependencyProperty.Register("BindIPHost", typeof(IPHost), typeof(ServerConfig), null);


        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        /// <summary>
        /// 名称
        /// </summary>
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(ServerConfig), "RRQMServer");


    }
}