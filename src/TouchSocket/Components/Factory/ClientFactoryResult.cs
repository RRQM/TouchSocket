using System;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// ClientFactoryResult
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public readonly struct ClientFactoryResult<TClient> : IDisposable where TClient : IClient
    {
        private readonly Action<TClient> m_action;

        /// <summary>
        /// ClientFactoryResult
        /// </summary>
        public ClientFactoryResult(TClient client, Action<TClient> action)
        {
            this.Client = client;
            this.m_action = action;
        }

        /// <summary>
        /// 客户端
        /// </summary>
        public TClient Client { get; }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            this.m_action.Invoke(this.Client);
        }
    }
}