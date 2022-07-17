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
using System.Net.Sockets;
using System.Text;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Plugins;
using TouchSocket.Sockets.Plugins;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 客户端扩展类
    /// </summary>
    public static class ClientExtension
    {
        /// <summary>
        /// 使用断线重连。
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="successCallback">成功回调函数</param>
        /// <param name="tryCount">尝试重连次数，设为-1时则永远尝试连接</param>
        /// <param name="printLog">是否输出日志。</param>
        /// <param name="sleepTime">失败时，停留时间</param>
        public static T UseReconnection<T>(this T client, int tryCount = 10, bool printLog = false, int sleepTime = 1000, Action<T> successCallback = null) where T : ITcpClient, IPlguinObject
        {
            client.PluginsManager.Add(new ReconnectionPlugin<T>(tryCount, printLog, sleepTime, successCallback));
            return client;
        }

        /// <summary>
        /// 获取相关信息。格式：
        ///<para>IPPort=IP:Port，ID=id，Protocol=Protocol</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string GetInfo<T>(this T client) where T : ISocketClient
        {
            return $"IP&Port={client.IP}:{client.Port}，ID={client.ID}，Protocol={client.Protocol}";
        }

        /// <summary>
        /// 安全性发送关闭报文
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="how"></param>
        public static void SafeShutdown<T>(this T client, SocketShutdown how = SocketShutdown.Both) where T : ITcpClientBase
        {
            try
            {
                client.Shutdown(how);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 获取IP和端口。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string GetIPPort<T>(this T client) where T : ITcpClientBase
        {
            return $"{client.IP}:{client.Port}";
        }

        #region 发送
        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        public static void Send<T>(this T client, string msg) where T : ISend
        {
            if (string.IsNullOrEmpty(msg))
            {
                throw new ArgumentException($"“{nameof(msg)}”不能为 null 或空。", nameof(msg));
            }

            client.Send(Encoding.UTF8.GetBytes(msg));
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        public static void SendAsync<T>(this T client, string msg) where T : ISend
        {
            if (string.IsNullOrEmpty(msg))
            {
                throw new ArgumentException($"“{nameof(msg)}”不能为 null 或空。", nameof(msg));
            }

            client.SendAsync(Encoding.UTF8.GetBytes(msg));
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="client"></param>
        /// <param name="id"></param>
        /// <param name="msg"></param>
        public static void Send<T>(this T client, string id, string msg) where T : IIDSender
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException($"“{nameof(id)}”不能为 null 或空。", nameof(id));
            }

            if (string.IsNullOrEmpty(msg))
            {
                throw new ArgumentException($"“{nameof(msg)}”不能为 null 或空。", nameof(msg));
            }

            client.Send(id, Encoding.UTF8.GetBytes(msg));
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="client"></param>
        /// <param name="id"></param>
        /// <param name="msg"></param>
        public static void SendAsync<T>(this T client, string id, string msg) where T : IIDSender
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException($"“{nameof(id)}”不能为 null 或空。", nameof(id));
            }

            if (string.IsNullOrEmpty(msg))
            {
                throw new ArgumentException($"“{nameof(msg)}”不能为 null 或空。", nameof(msg));
            }

            client.SendAsync(id, Encoding.UTF8.GetBytes(msg));
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
        public static bool TrySendAsync<T>(this T client, byte[] buffer, int offset, int length) where T : ISend
        {
            if (client.CanSend)
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
        public static bool TrySendAsync<T>(this T client, byte[] buffer) where T : ISend
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
        public static bool TrySendAsync<T>(this T client, ByteBlock byteBlock) where T : ISend
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
        public static bool TrySend<T>(this T client, byte[] buffer, int offset, int length) where T : ISend
        {
            if (client.CanSend)
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
        public static bool TrySend<T>(this T client, byte[] buffer) where T : ISend
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
        public static bool TrySend<T>(this T client, ByteBlock byteBlock) where T : ISend
        {
            return TrySend(client, byteBlock.Buffer, 0, byteBlock.Len);
        }
        #endregion 发送

        #region Udp广播
        /// <summary>
        /// 广播。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="port"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static void SendWithBroadcast<T>(this T client,int port,byte[] buffer,int offset,int length) where T : IUdpClientSender
        {
            //client.Send(new IPHost($"255.255.255.255:{port}").EndPoint,buffer,offset,length);
        }
        #endregion
    }
}
