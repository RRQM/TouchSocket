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

using System.Net.Sockets;

namespace RRQMSocket
{
    /// <summary>
    /// 客户端连接事件。
    /// </summary>
    public class ClientConnectingEventArgs : MesEventArgs
    {
        private Socket socket;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="socket"></param>
        public ClientConnectingEventArgs(Socket socket)
        {
            this.socket = socket;
        }

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        public DataHandlingAdapter DataHandlingAdapter { get; set; }

        /// <summary>
        /// 新初始化的通信器
        /// </summary>
        public Socket Socket
        {
            get { return socket; }
        }
    }
}