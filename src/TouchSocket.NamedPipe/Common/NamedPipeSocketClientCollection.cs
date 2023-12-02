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

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TouchSocket.NamedPipe
{
    internal class NamedPipeSocketClientCollection : ConcurrentDictionary<string, INamedPipeSocketClient>, INamedPipeSocketClientCollection
    {
        public IEnumerable<INamedPipeSocketClient> GetClients()
        {
            return this.Values;
        }

        public IEnumerable<string> GetIds()
        {
            return this.Keys;
        }

        public bool SocketClientExist(string id)
        {
            return string.IsNullOrEmpty(id) ? false : this.ContainsKey(id);
        }

        public bool TryGetSocketClient(string id, out INamedPipeSocketClient socketClient)
        {
            if (string.IsNullOrEmpty(id))
            {
                socketClient = null;
                return false;
            }

            return this.TryGetValue(id, out socketClient);
        }

        public bool TryGetSocketClient<TClient>(string id, out TClient socketClient) where TClient : INamedPipeSocketClient
        {
            if (string.IsNullOrEmpty(id))
            {
                socketClient = default;
                return false;
            }

            if (this.TryGetValue(id, out var client))
            {
                socketClient = (TClient)client;
                return true;
            }
            socketClient = default;
            return false;
        }

        internal bool TryAdd(INamedPipeSocketClient socketClient)
        {
            return this.TryAdd(socketClient.Id, socketClient);
        }

        internal bool TryRemove<TClient>(string id, out TClient socketClient) where TClient : INamedPipeSocketClient
        {
            if (string.IsNullOrEmpty(id))
            {
                socketClient = default;
                return false;
            }

            if (this.TryRemove(id, out var client))
            {
                socketClient = (TClient)client;
                return true;
            }
            socketClient = default;
            return false;
        }
    }
}