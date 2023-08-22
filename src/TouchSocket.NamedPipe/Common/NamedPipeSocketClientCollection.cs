using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.NamedPipe
{
    internal class NamedPipeSocketClientCollection: ConcurrentDictionary<string, INamedPipeSocketClient>, INamedPipeSocketClientCollection
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
