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
using System;

namespace RRQMSocket.Helper
{
    /// <summary>
    /// 客户端辅助类
    /// </summary>
    public static class ClientHelper
    {
        /// <summary>
        /// 使用断线重连。
        /// <para>注意，使用断线重连时，如果是自定义适配器，应当在<see cref="ITcpClient.Connecting"/>事件中设置。</para>
        /// </summary>
        /// <param name="tcpClient">客户端</param>
        /// <param name="tryCount">尝试重连次数，设为-1时则永远尝试连接</param>
        /// <param name="printLog">是否输出日志。</param>
        public static ITcpClient UseReconnection(this ITcpClient tcpClient, int tryCount = 10, bool printLog = false)
        {
            if (tcpClient is null)
            {
                throw new ArgumentNullException(nameof(tcpClient));
            }

           
            tcpClient.Disconnected += (client, e) =>
            {
                int tryT = tryCount;
                while (tryCount < 0 || tryT-- > 0)
                {
                    try
                    {
                        client.Connect();
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
            };

            return tcpClient;
        }
    }
}