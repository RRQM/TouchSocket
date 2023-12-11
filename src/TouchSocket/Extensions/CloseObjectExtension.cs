using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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