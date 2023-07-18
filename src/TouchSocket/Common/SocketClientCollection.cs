using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace TouchSocket.Sockets
{
    [DebuggerDisplay("Count={Count}")]
    internal class SocketClientCollection : ConcurrentDictionary<string, ISocketClient>, ISocketClientCollection
    {
        public IEnumerable<ISocketClient> GetClients()
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

        public bool TryGetSocketClient(string id, out ISocketClient socketClient)
        {
            if (string.IsNullOrEmpty(id))
            {
                socketClient = null;
                return false;
            }

            return this.TryGetValue(id, out socketClient);
        }

        public bool TryGetSocketClient<TClient>(string id, out TClient socketClient) where TClient : ISocketClient
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

        internal bool TryAdd(ISocketClient socketClient)
        {
            return this.TryAdd(socketClient.Id, socketClient);
        }

        internal bool TryRemove<TClient>(string id, out TClient socketClient) where TClient : ISocketClient
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