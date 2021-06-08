using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.ByteManager;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// 通信器
    /// </summary>
    public class TcpRPCService : TokenService<RPCSocketClient>
    {
        internal Action<RPCSocketClient, ByteBlock> Received;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tcpSocketClient"></param>
        /// <param name="createOption"></param>
        protected override void OnCreateSocketCliect(RPCSocketClient tcpSocketClient, CreateOption createOption)
        {
            if (createOption.NewCreate)
            {
                tcpSocketClient.Logger = this.Logger;
                tcpSocketClient.DataHandlingAdapter = new FixedHeaderDataHandlingAdapter();
                tcpSocketClient.Received = this.Received;
            }
            tcpSocketClient.agreementHelper = new RRQMAgreementHelper(tcpSocketClient);
        }
    }
}
