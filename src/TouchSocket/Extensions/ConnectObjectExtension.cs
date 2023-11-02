using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// ConnectObjectExtension
    /// </summary>
    public static class ConnectObjectExtension
    {
        #region 连接

        /// <inheritdoc cref="IConnectObject.Connect(int, CancellationToken)"/>
        public static void Connect(this IConnectObject client, int timeout = 5000)
        {
            client.Connect(timeout,CancellationToken.None);
        }

        /// <inheritdoc cref="IConnectObject.ConnectAsync(int, CancellationToken)"/>
        public static async Task ConnectAsync(this IConnectObject client, int timeout = 5000)
        {
            await client.ConnectAsync(timeout, CancellationToken.None);
        }

        /// <summary>
        /// 尝试连接。不会抛出异常。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static Result TryConnect(this IConnectObject client, int timeout = 5000)
        {
            try
            {
                client.Connect(timeout);
                return new Result(ResultCode.Success);
            }
            catch (Exception ex)
            {
                return new Result(ResultCode.Exception, ex.Message);
            }
        }

        /// <summary>
        /// 尝试连接。不会抛出异常。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<Result> TryConnectAsync(this IConnectObject client, int timeout = 5000) 
        {
            try
            {
                await client.ConnectAsync(timeout);
                return new Result(ResultCode.Success);
            }
            catch (Exception ex)
            {
                return new Result(ResultCode.Exception, ex.Message);
            }
        }

        #endregion 连接
    }
}
