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
using RRQMCore;
using RRQMCore.ByteManager;
using RRQMCore.Log;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 端口转发辅助
    /// </summary>
    public class NATSocketClient : SocketClient
    {
        private readonly System.Threading.ReaderWriterLockSlim m_lockSlim = new System.Threading.ReaderWriterLockSlim();

        private readonly List<ITcpClient> m_targetClients;

        internal Action<NATSocketClient, ITcpClient, ClientDisconnectedEventArgs> internalDis;
        internal Func<NATSocketClient, ITcpClient, ByteBlock, IRequestInfo, byte[]> internalTargetClientRev;

        /// <summary>
        /// 获取所有目标客户端
        /// </summary>
        /// <returns></returns>
        public ITcpClient[] GetTargetClients()
        {
            return this.m_targetClients.ToArray();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public NATSocketClient()
        {
            this.m_targetClients = new List<ITcpClient>();
        }

        /// <summary>
        /// 添加转发客户端。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public ITcpClient AddTargetClient(RRQMConfig config)
        {
            using WriteLock writeLock = new WriteLock(this.m_lockSlim);
            TcpClient tcpClient = new TcpClient();
            tcpClient.Disconnected += this.TcpClient_Disconnected;
            tcpClient.Received += this.TcpClient_Received;
            tcpClient.Setup(config);
            tcpClient.Connect();

            this.m_targetClients.Add(tcpClient);
            return tcpClient;
        }

        /// <summary>
        /// 添加转发客户端。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public Task<ITcpClient> AddTargetClientAsync(RRQMConfig config)
        {
            return Task.Run(() =>
             {
                 return this.AddTargetClient(config);
             });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (this.m_targetClients != null)
            {
                foreach (var socket in this.m_targetClients)
                {
                    socket.SafeDispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 发送数据到全部转发端。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void SendToTargetClient(byte[] buffer, int offset, int length)
        {
            using WriteLock writeLock = new WriteLock(this.m_lockSlim);
            foreach (var socket in this.m_targetClients)
            {
                try
                {
                    socket.Send(buffer, offset, length);
                }
                catch
                {
                }
            }
        }

        private void TcpClient_Disconnected(ITcpClientBase client, ClientDisconnectedEventArgs e)
        {
            using WriteLock writeLock = new WriteLock(this.m_lockSlim);
            this.m_targetClients.Remove((ITcpClient)client);
            this.internalDis?.Invoke(this, (ITcpClient)client, e);
        }

        private void TcpClient_Received(TcpClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.disposedValue)
            {
                return;
            }

            try
            {
                var data = this.internalTargetClientRev?.Invoke(this, client, byteBlock, requestInfo);
                if (data != null)
                {
                    this.Send(data);
                }
            }
            catch (Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, "在处理数据时发生错误", ex);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
    }
}