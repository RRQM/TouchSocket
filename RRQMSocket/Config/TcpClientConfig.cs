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
using System;

namespace RRQMSocket
{
    /// <summary>
    /// TcpClient配置
    /// </summary>
    public class TcpClientConfig : ClientConfig
    {
        /// <summary>
        /// 数据处理适配器
        /// </summary>
        [Obsolete("已放弃Config注入适配器，请在客户端使用数据处理适配器之前，任意时刻赋值，例如在构造函数、Connecting事件等")]
        public DataHandlingAdapter DataHandlingAdapter
        {
            get { return (DataHandlingAdapter)GetValue(DataHandlingAdapterProperty); }
            set { SetValue(DataHandlingAdapterProperty, value); }
        }

        /// <summary>
        /// 数据处理适配器，所需类型<see cref="RRQMSocket.DataHandlingAdapter"/>
        /// </summary>
        [Obsolete("已放弃Config注入适配器，请在客户端使用数据处理适配器之前，任意时刻赋值，例如在构造函数、Connecting事件等")]
        public static readonly DependencyProperty DataHandlingAdapterProperty =
            DependencyProperty.Register("DataHandlingAdapter", typeof(DataHandlingAdapter), typeof(TcpClientConfig), null);

        /// <summary>
        /// 远程IPHost
        /// </summary>
        public IPHost RemoteIPHost
        {
            get { return (IPHost)GetValue(RemoteIPHostProperty); }
            set { SetValue(RemoteIPHostProperty, value); }
        }

        /// <summary>
        /// 远程IPHost，所需类型<see cref="IPHost"/>
        /// </summary>
        public static readonly DependencyProperty RemoteIPHostProperty =
            DependencyProperty.Register("RemoteIPHost", typeof(IPHost), typeof(TcpClientConfig), null);

        /// <summary>
        /// 仅发送，即不开启接收线程，
        /// 同时不会感知断开操作。
        /// </summary>
        public bool OnlySend
        {
            get { return (bool)GetValue(OnlySendProperty); }
            set { SetValue(OnlySendProperty, value); }
        }

        /// <summary>
        /// 仅发送，即不开启接收线程，
        /// 同时不会感知断开操作，所需类型<see cref="bool"/>
        /// </summary>
        public static readonly DependencyProperty OnlySendProperty =
            DependencyProperty.Register("OnlySend", typeof(bool), typeof(TcpClientConfig), false);

        /// <summary>
        /// 在异步发送时，使用独立线程发送
        /// </summary>
        public bool SeparateThreadSend
        {
            get { return (bool)GetValue(SeparateThreadSendProperty); }
            set { SetValue(SeparateThreadSendProperty, value); }
        }

        /// <summary>
        /// 在异步发送时，使用独立线程发送，所需类型<see cref="bool"/>
        /// </summary>
        public static readonly DependencyProperty SeparateThreadSendProperty =
            DependencyProperty.Register("SeparateThreadSend", typeof(bool), typeof(TcpClientConfig), false);

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
            DependencyProperty.Register("ReceiveType", typeof(ReceiveType), typeof(TcpClientConfig), ReceiveType.IOCP);
    }
}