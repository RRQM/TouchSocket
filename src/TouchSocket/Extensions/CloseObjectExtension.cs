//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

namespace TouchSocket.Sockets
{
    /// <summary>
    /// CloseObjectExtension
    /// </summary>
    public static class CloseObjectExtension
    {
        /// <summary>
        /// <inheritdoc cref="ICloseObject.Close(string)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        public static void Close<T>(this T client) where T : ICloseObject
        {
            client.Close(string.Empty);
        }

        /// <summary>
        /// 安全性关闭。不会抛出异常。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        public static void SafeClose<T>(this T client, string msg) where T : ICloseObject
        {
            try
            {
                if (client == null)
                {
                    return;
                }
                if (client is IOnlineClient onlineClient)
                {
                    if (onlineClient.Online)
                    {
                        client.Close(msg);
                    }
                }
                else if (client is IHandshakeObject handshakeObject)
                {
                    if (handshakeObject.IsHandshaked)
                    {
                        client.Close(msg);
                    }
                }
                else
                {
                    client.Close(msg);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        ///  安全性关闭。不会抛出异常。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        public static void SafeClose<T>(this T client) where T : ICloseObject
        {
            SafeClose(client);
        }
    }
}