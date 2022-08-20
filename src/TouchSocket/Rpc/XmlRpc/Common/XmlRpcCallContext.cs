using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Http;

namespace TouchSocket.Rpc.XmlRpc
{
    internal class XmlRpcCallContext : IXmlRpcCallContext
    {
        private CancellationTokenSource m_tokenSource;

        public object Caller { get; internal set; }

        public MethodInstance MethodInstance { get; internal set; }

        public CancellationTokenSource TokenSource
        {
            get
            {
                if (this.m_tokenSource == null)
                {
                    this.m_tokenSource = new CancellationTokenSource();
                }
                return this.m_tokenSource;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public HttpContext HttpContext { get; internal set; }

        public string XmlString { get; internal set; }
    }
}
