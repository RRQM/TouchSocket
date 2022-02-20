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
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// 服务器配置
    /// </summary>
    public class ServiceConfig : RRQMConfig
    {
        /// <summary>
        /// 多线程数量，该值等效于<see cref="ThreadPool.SetMinThreads(int, int)"/>
        /// </summary>
        public int ThreadCount
        {
            get { return (int)this.GetValue(ThreadCountProperty); }
            set { this.SetValue(ThreadCountProperty, value); }
        }

        /// <summary>
        /// 多线程数量，该值等效于<see cref="ThreadPool.SetMinThreads(int, int)"/>
        /// 所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty ThreadCountProperty =
            DependencyProperty.Register("ThreadCount", typeof(int), typeof(ServiceConfig), 10);

        /// <summary>
        /// 服务名称，用于标识，无实际意义
        /// </summary>
        public string ServerName
        {
            get { return (string)this.GetValue(ServerNameProperty); }
            set { this.SetValue(ServerNameProperty, value); }
        }

        /// <summary>
        /// 服务名称，用于标识，无实际意义，所需类型<see cref="string"/>
        /// </summary>
        public static readonly DependencyProperty ServerNameProperty =
            DependencyProperty.Register("ServerName", typeof(string), typeof(ServiceConfig), "RRQMServer");
    }
}