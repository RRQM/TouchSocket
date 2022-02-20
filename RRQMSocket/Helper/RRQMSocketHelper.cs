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
using System;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.Helper
{
    /// <summary>
    /// 扩展辅助类
    /// </summary>
    public static class RRQMSocketHelper
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
        /// 转为ResultCode
        /// </summary>
        /// <param name="channelStatus"></param>
        /// <returns></returns>
        public static ResultCode ToResultCode(this ChannelStatus channelStatus)
        {
            switch (channelStatus)
            {
                case ChannelStatus.Default:
                    return ResultCode.Default;

                case ChannelStatus.Overtime:
                    return ResultCode.Overtime;

                case ChannelStatus.Cancel:
                    return ResultCode.Canceled;

                case ChannelStatus.Completed:
                    return ResultCode.Success;

                case ChannelStatus.Moving:
                case ChannelStatus.Disposed:
                default:
                    return ResultCode.Error;
            }
        }
    }
}
