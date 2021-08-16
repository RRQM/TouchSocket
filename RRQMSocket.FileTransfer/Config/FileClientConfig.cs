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
using RRQMSocket.RPC.RRQMRPC;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件客户端配置
    /// </summary>
    public class FileClientConfig : TcpRpcClientConfig
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public FileClientConfig()
        {
            this.BufferLength = 64 * 1024;
        }

        /// <summary>
        /// 默认接收文件的存放目录
        /// </summary>
        public string ReceiveDirectory
        {
            get { return (string)GetValue(ReceiveDirectoryProperty); }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                SetValue(ReceiveDirectoryProperty, value);
            }
        }

        /// <summary>
        /// 默认接收文件的存放目录, 所需类型<see cref="string"/>
        /// </summary>
        public static readonly DependencyProperty ReceiveDirectoryProperty =
            DependencyProperty.Register("ReceiveDirectory", typeof(string), typeof(FileClientConfig), string.Empty);

        /// <summary>
        /// 单次请求超时时间 min=5000,max=60*1000 ms
        /// </summary>
        public int Timeout
        {
            get { return (int)GetValue(TimeoutProperty); }
            set
            {
                SetValue(TimeoutProperty, value);
            }
        }

        /// <summary>
        /// 单次请求超时时间 min=5000,max=60*1000 ms, 所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty TimeoutProperty =
            DependencyProperty.Register("Timeout", typeof(int), typeof(FileClientConfig), 10*1000);

        /// <summary>
        /// 数据包尺寸
        /// </summary>
        public int PacketSize
        {
            get { return (int)GetValue(PacketSizeProperty); }
            set { SetValue(PacketSizeProperty, value); }
        }

        /// <summary>
        /// 数据包尺寸, 所需类型<see cref="int"/>
        /// </summary>
        [RRQMCore.Range]
        public static readonly DependencyProperty PacketSizeProperty =
            DependencyProperty.Register("PacketSize", typeof(int), typeof(FileClientConfig), 1024 * 1024);
    }
}