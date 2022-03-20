using RRQMCore;
using RRQMCore.ByteManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 客户端扩展类
    /// </summary>
    public static class RRQMClientExtensions
    {
        /// <summary>
        /// 使用断线重连。
        /// <para>注意，使用断线重连时，如果是自定义适配器，应当在<see cref="ITcpClient.Connecting"/>事件中设置。</para>
        /// </summary>
        /// <param name="tcpClient">客户端</param>
        /// <param name="tryCount">尝试重连次数，设为-1时则永远尝试连接</param>
        /// <param name="printLog">是否输出日志。</param>
        public static T UseReconnection<T>(this T tcpClient, int tryCount = 10, bool printLog = false) where T : ITcpClient
        {
            if (tcpClient is null)
            {
                throw new ArgumentNullException(nameof(tcpClient));
            }

            tcpClient.Disconnected += (client, e) =>
            {
                if (e.Manual)
                {
                    return;
                }
                Task.Run(() =>
                {
                    int tryT = tryCount;
                    while (tryCount < 0 || tryT-- > 0)
                    {
                        try
                        {
                            tcpClient.Connect();
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (printLog)
                            {
                                tcpClient.Logger.Debug(RRQMCore.Log.LogType.Error, tcpClient, "断线重连失败。", ex);
                            }
                        }
                    }
                });
            };

            return tcpClient;
        }

        /// <summary>
        /// 使用断线重连。
        /// <para>注意，使用断线重连时，如果是自定义适配器，应当在<see cref="ITcpClient.Connecting"/>事件中设置。</para>
        /// </summary>
        /// <param name="tcpClient">客户端</param>
        /// <param name="verifyToken">验证Token</param>
        /// <param name="successCallback">成功回调函数</param>
        /// <param name="tryCount">尝试重连次数，设为-1时则永远尝试连接</param>
        /// <param name="printLog">是否输出日志。</param>
        public static T UseReconnection<T>(this T tcpClient, string verifyToken, int tryCount = 10, bool printLog = false, Action<T> successCallback = null) where T : ITokenClient
        {
            if (tcpClient is null)
            {
                throw new ArgumentNullException(nameof(tcpClient));
            }

            tcpClient.Disconnected += (client, e) =>
            {
                if (e.Manual)
                {
                    return;
                }
                Task.Run(() =>
                {
                    int tryT = tryCount;
                    while (tryCount < 0 || tryT-- > 0)
                    {
                        try
                        {
                            tcpClient.Connect(verifyToken);
                            successCallback?.Invoke(tcpClient);
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (printLog)
                            {
                                tcpClient.Logger.Debug(RRQMCore.Log.LogType.Error, tcpClient, "断线重连失败。", ex);
                            }
                        }
                    }
                });
            };

            return tcpClient;
        }

        #region 发送
        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        public static void Send(this ISenderBase sender, string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                throw new ArgumentException($"“{nameof(msg)}”不能为 null 或空。", nameof(msg));
            }

            sender.Send(Encoding.UTF8.GetBytes(msg));
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        public static void SendAsync(this ISenderBase sender, string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                throw new ArgumentException($"“{nameof(msg)}”不能为 null 或空。", nameof(msg));
            }

            sender.SendAsync(Encoding.UTF8.GetBytes(msg));
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="id"></param>
        /// <param name="msg"></param>
        public static void Send(this IIDSender sender, string id, string msg)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException($"“{nameof(id)}”不能为 null 或空。", nameof(id));
            }

            if (string.IsNullOrEmpty(msg))
            {
                throw new ArgumentException($"“{nameof(msg)}”不能为 null 或空。", nameof(msg));
            }

            sender.Send(id, Encoding.UTF8.GetBytes(msg));
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="id"></param>
        /// <param name="msg"></param>
        public static void SendAsync(this IIDSender sender, string id, string msg)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException($"“{nameof(id)}”不能为 null 或空。", nameof(id));
            }

            if (string.IsNullOrEmpty(msg))
            {
                throw new ArgumentException($"“{nameof(msg)}”不能为 null 或空。", nameof(msg));
            }

            sender.SendAsync(id, Encoding.UTF8.GetBytes(msg));
        }

        /// <summary>
        /// 尝试异步发送数据。
        /// <para>当客户端使用独立线程发送时，会永远返回True</para>
        /// </summary>
        /// <typeparam name="T">客户端类型</typeparam>
        /// <param name="client">客户端</param>
        /// <param name="buffer">数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <returns>是否完成发送</returns>
        public static bool TrySendAsync<T>(this T client, byte[] buffer, int offset, int length) where T : ITcpClientBase
        {
            if (client.Online)
            {
                try
                {
                    client.SendAsync(buffer, offset, length);
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }

        /// <summary>
        /// 尝试发送数据
        /// </summary>
        /// <typeparam name="T">客户端类型</typeparam>
        /// <param name="client">客户端</param>
        /// <param name="buffer">数据</param>
        /// <returns>是否完成发送</returns>
        public static bool TrySendAsync<T>(this T client, byte[] buffer) where T : ITcpClientBase
        {
            return TrySendAsync(client, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 尝试发送数据
        /// </summary>
        /// <typeparam name="T">客户端类型</typeparam>
        /// <param name="client">客户端</param>
        /// <param name="byteBlock">数据</param>
        /// <returns>是否完成发送</returns>
        public static bool TrySendAsync<T>(this T client, ByteBlock byteBlock) where T : ITcpClientBase
        {
            return TrySendAsync(client, byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <summary>
        /// 尝试发送数据
        /// </summary>
        /// <typeparam name="T">客户端类型</typeparam>
        /// <param name="client">客户端</param>
        /// <param name="buffer">数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <returns>是否完成发送</returns>
        public static bool TrySend<T>(this T client, byte[] buffer, int offset, int length) where T : ITcpClientBase
        {
            if (client.Online)
            {
                try
                {
                    client.Send(buffer, offset, length);
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }

        /// <summary>
        /// 尝试发送数据
        /// </summary>
        /// <typeparam name="T">客户端类型</typeparam>
        /// <param name="client">客户端</param>
        /// <param name="buffer">数据</param>
        /// <returns>是否完成发送</returns>
        public static bool TrySend<T>(this T client, byte[] buffer) where T : ITcpClientBase
        {
            return TrySend(client, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 尝试发送数据
        /// </summary>
        /// <typeparam name="T">客户端类型</typeparam>
        /// <param name="client">客户端</param>
        /// <param name="byteBlock">数据</param>
        /// <returns>是否完成发送</returns>
        public static bool TrySend<T>(this T client, ByteBlock byteBlock) where T : ITcpClientBase
        {
            return TrySend(client, byteBlock.Buffer, 0, byteBlock.Len);
        }
        #endregion 发送
    }
}
