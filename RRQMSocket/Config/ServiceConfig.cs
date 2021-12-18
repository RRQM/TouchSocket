//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
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
    public class ServiceConfig : RRQMConfig
    {
        /// <summary>
        /// 接收类型
        /// </summary>
        public ReceiveType ReceiveType
        {
            get { return (ReceiveType)GetValue(ReceiveTypeProperty); }
            set { SetValue(ReceiveTypeProperty, value); }
        }

        /// <summary>
        /// 接收类型，所需类型<see cref="RRQMSocket. ReceiveType"/>
        /// </summary>
        public static readonly DependencyProperty ReceiveTypeProperty =
            DependencyProperty.Register("ReceiveType", typeof(ReceiveType), typeof(ServiceConfig), ReceiveType.IOCP);

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
            DependencyProperty.Register("ThreadCount", typeof(int), typeof(ServiceConfig), 10);

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
            DependencyProperty.Register("ServerName", typeof(string), typeof(ServiceConfig), "RRQMServer");
    }
}