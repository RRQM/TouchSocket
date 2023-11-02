using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 具有连接动作的对象
    /// </summary>
    public interface IConnectObject
    {
        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="timeout">最大等待时间</param>
        /// <param name="token">可取消令箭</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="Exception"></exception>
        void Connect(int timeout, CancellationToken token);

        /// <summary>
        /// 异步连接
        /// </summary>
        /// <param name="timeout">最大等待时间</param>
        /// <param name="token">可取消令箭</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="Exception"></exception>
        Task ConnectAsync(int timeout, CancellationToken token);
    }
}
