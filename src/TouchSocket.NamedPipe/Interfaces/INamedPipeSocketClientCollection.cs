using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// INamedPipeSocketClientCollection
    /// </summary>
    public interface INamedPipeSocketClientCollection
    {
        /// <summary>
        /// 集合长度
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 通过Id查找<see cref="INamedPipeSocketClient"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public INamedPipeSocketClient this[string id] { get; }

        /// <summary>
        /// 获取所有的客户端
        /// </summary>
        /// <returns></returns>
        IEnumerable<INamedPipeSocketClient> GetClients();

        /// <summary>
        /// 获取Id集合
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetIds();

        /// <summary>
        /// 根据Id判断SocketClient是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool SocketClientExist(string id);

        /// <summary>
        /// 尝试获取实例
        /// </summary>
        /// <param name="id"></param>
        /// <param name="socketClient"></param>
        /// <returns></returns>
        bool TryGetSocketClient(string id, out INamedPipeSocketClient socketClient);

        /// <summary>
        /// 尝试获取实例
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="id"></param>
        /// <param name="socketClient"></param>
        /// <returns></returns>
        bool TryGetSocketClient<TClient>(string id, out TClient socketClient) where TClient : INamedPipeSocketClient;
    }
}
