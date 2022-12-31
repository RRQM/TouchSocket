//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets
{
    internal class WaitingClient<TClient> : IWaitingClient<TClient> where TClient : IClient, IDefaultSender, ISender
    {
        private readonly TClient m_client;

        private readonly WaitData<ResponsedData> m_waitData;

        private WaitingOptions m_waitingOptions;

        public WaitingClient(TClient client, WaitingOptions waitingOptions)
        {
            m_client = client ?? throw new ArgumentNullException(nameof(client));
            m_waitData = new WaitData<ResponsedData>();
            m_waitingOptions = waitingOptions;
        }

        public bool CanSend
        {
            get
            {
                if (m_client is ITcpClientBase tcpClient)
                {
                    return tcpClient.Online;
                }
                else if (m_client is IUdpSession)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public TClient Client => m_client;

        public WaitingOptions WaitingOptions { get => m_waitingOptions; set => m_waitingOptions = value; }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public ResponsedData SendThenResponse(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default)
        {
            lock (this)
            {
                try
                {
                    m_waitData.Reset();

                    if (m_waitingOptions == WaitingOptions.AllAdapter || m_waitingOptions == WaitingOptions.WaitAdapter)
                    {
                        m_client.OnHandleReceivedData += OnHandleReceivedData;
                    }
                    else
                    {
                        m_client.OnHandleRawBuffer += OnHandleRawBuffer;
                    }

                    if (m_waitingOptions == WaitingOptions.AllAdapter || m_waitingOptions == WaitingOptions.SendAdapter)
                    {
                        m_client.Send(buffer, offset, length);
                    }
                    else
                    {
                        m_client.DefaultSend(buffer, offset, length);
                    }

                    m_waitData.SetCancellationToken(token);
                    switch (m_waitData.Wait(timeout))
                    {
                        case WaitDataStatus.SetRunning:
                            return m_waitData.WaitResult;

                        case WaitDataStatus.Overtime:
                            throw new TimeoutException();
                        case WaitDataStatus.Canceled:
                            {
                                return default;
                            }
                        case WaitDataStatus.Default:
                        case WaitDataStatus.Disposed:
                        default:
                            throw new Exception(TouchSocketStatus.UnknownError.GetDescription());
                    }
                }
                finally
                {
                    if (m_waitingOptions == WaitingOptions.AllAdapter || m_waitingOptions == WaitingOptions.WaitAdapter)
                    {
                        m_client.OnHandleReceivedData -= OnHandleReceivedData;
                    }
                    else
                    {
                        m_client.OnHandleRawBuffer -= OnHandleRawBuffer;
                    }
                }
            }
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public ResponsedData SendThenResponse(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return SendThenResponse(buffer, 0, buffer.Length, timeout, token);
        }

        /// <summary>
        /// 发送流中的有效数据
        /// </summary>
        /// <param name="byteBlock">数据块载体</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public ResponsedData SendThenResponse(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return SendThenResponse(byteBlock.Buffer, 0, byteBlock.Len, timeout, token);
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public Task<ResponsedData> SendThenResponseAsync(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return EasyTask.Run(() =>
            {
                return SendThenResponse(buffer, offset, length, timeout, token);
            });
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public Task<ResponsedData> SendThenResponseAsync(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return EasyTask.Run(() =>
            {
                return SendThenResponse(buffer, timeout, token);
            });
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="byteBlock">数据块载体</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public Task<ResponsedData> SendThenResponseAsync(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return EasyTask.Run(() =>
            {
                return SendThenResponse(byteBlock, timeout, token);
            });
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public byte[] SendThenReturn(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return SendThenResponse(buffer, offset, length, timeout, token).Data;
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public byte[] SendThenReturn(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return SendThenReturn(buffer, 0, buffer.Length, timeout, token);
        }

        /// <summary>
        /// 发送流中的有效数据
        /// </summary>
        /// <param name="byteBlock">数据块载体</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public byte[] SendThenReturn(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return SendThenReturn(byteBlock.Buffer, 0, byteBlock.Len, timeout, token);
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public Task<byte[]> SendThenReturnAsync(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return EasyTask.Run(() =>
            {
                return SendThenReturn(buffer, offset, length, timeout, token);
            });
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public Task<byte[]> SendThenReturnAsync(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return EasyTask.Run(() =>
            {
                return SendThenReturn(buffer, timeout, token);
            });
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="byteBlock">数据块载体</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public Task<byte[]> SendThenReturnAsync(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return EasyTask.Run(() =>
            {
                return SendThenReturn(byteBlock, timeout, token);
            });
        }

        private bool OnHandleRawBuffer(ByteBlock byteBlock)
        {
            ResponsedData responsedData = new ResponsedData(byteBlock.ToArray(), null);
            return !m_waitData.Set(responsedData);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        private bool OnHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            ResponsedData responsedData;
            if (byteBlock != null)
            {
                responsedData = new ResponsedData(byteBlock.ToArray(), requestInfo);
            }
            else
            {
                responsedData = new ResponsedData(null, requestInfo);
            }

            return !m_waitData.Set(responsedData);
        }
    }
}