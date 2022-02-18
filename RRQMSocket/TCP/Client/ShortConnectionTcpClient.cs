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
//using RRQMCore.ByteManager;
//using RRQMCore.Exceptions;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace RRQMSocket
//{
//    /// <summary>
//    /// 短连接TCP客户端
//    /// </summary>
//    public class ShortConnectionTcpClient : BaseSocket, ITcpClientBase, IWaitSender
//    {
//        private string ip;
//        private int port;
//        private Socket socket;
//        private DataHandlingAdapter dataHandlingAdapter;

//        /// <summary>
//        /// <inheritdoc/>
//        /// </summary>
//        public string IP => ip;

//        /// <summary>
//        /// <inheritdoc/>
//        /// </summary>
//        public int Port => port;

//        /// <summary>
//        /// 主同信器
//        /// </summary>
//        public Socket MainSocket => this.socket;

//        /// <summary>
//        /// <inheritdoc/>
//        /// </summary>
//        public string Name => $"{this.ip}:{this.port}";

//        /// <summary>
//        /// <inheritdoc/>
//        /// </summary>
//        public bool Online => this.socket == null ? false : socket.Connected;

//        /// <summary>
//        /// <inheritdoc/>
//        /// </summary>
//        public DataHandlingAdapter DataHandlingAdapter => this.dataHandlingAdapter;

//        /// <summary>
//        /// <inheritdoc/>
//        /// </summary>
//        public BytePool BytePool => BytePool.Default;

//#pragma warning disable CS1591
//        public byte[] SendThenReturn(byte[] buffer, int offset, int length, CancellationToken token = default)
//        {
//            throw new NotImplementedException();
//        }

//        public byte[] SendThenReturn(byte[] buffer, CancellationToken token = default)
//        {
//            throw new NotImplementedException();
//        }

//        public byte[] SendThenReturn(ByteBlock byteBlock, CancellationToken token = default)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<byte[]> SendThenReturnAsync(byte[] buffer, int offset, int length, CancellationToken token = default)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<byte[]> SendThenReturnAsync(byte[] buffer, CancellationToken token = default)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<byte[]> SendThenReturnAsync(ByteBlock byteBlock, CancellationToken token = default)
//        {
//            throw new NotImplementedException();
//        }
//#pragma warning restore CS1591

//        public void Connect(EndPoint endPoint)
//        {
//            this.socket = new Socket();
//        }

//        public void SetDataHandlingAdapter(DataHandlingAdapter adapter)
//        {
//            if (adapter == null)
//            {
//                throw new RRQMException("数据处理适配器为空");
//            }

//            adapter.BytePool = BytePool;
//            adapter.Logger = this.Logger;
//            adapter.ReceivedCallBack = this.HandleReceivedData;
//            adapter.SendCallBack = this.Sent;
//            this.dataHandlingAdapter = adapter;
//        }

//        private void HandleReceivedData(ByteBlock byteBlock, object obj)
//        {
//        }

//        private void Sent(byte[] buffer, int offset, int length, bool isAsync)
//        {
//            if (!this.Online)
//            {
//                throw new RRQMNotConnectedException("该实例已断开");
//            }
//            if (isAsync)
//            {
//                this.socket.BeginSend(buffer, offset, length, SocketFlags.None, null, null);
//            }
//            else
//            {
//                while (length > 0)
//                {
//                    int r = MainSocket.Send(buffer, offset, length, SocketFlags.None);
//                    if (r == 0 && length > 0)
//                    {
//                        throw new RRQMException("发送数据不完全");
//                    }
//                    offset += r;
//                    length -= r;
//                }
//            }
//        }
//    }
//}