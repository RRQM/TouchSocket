using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 命名管道客户端接口
    /// </summary>
    public interface INamedPipeClient: INamedPipeClientBase
    {
        /// <summary>
        /// 成功连接到服务器
        /// </summary>
        ConnectedEventHandler<INamedPipeClient> Connected { get; set; }

        /// <summary>
        /// 准备连接的时候
        /// </summary>
        ConnectingEventHandler<INamedPipeClient> Connecting { get; set; }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="Exception"></exception>
        INamedPipeClient Connect(int timeout = 5000);

        /// <summary>
        /// 异步连接服务器
        /// </summary>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="Exception"></exception>
        Task<INamedPipeClient> ConnectAsync(int timeout = 5000);

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="config"></param>
        /// <exception cref="Exception"></exception>
        INamedPipeClient Setup(TouchSocketConfig config);
    }
}
