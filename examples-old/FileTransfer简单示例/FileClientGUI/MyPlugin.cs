using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Rpc.TouchRpc;

namespace FileClientGUI
{
    internal class MyPlugin : TouchRpcPluginBase<TcpTouchRpcClient>
    {
        protected override void OnFileTransfering(TcpTouchRpcClient client, FileOperationEventArgs e)
        {
            e.IsPermitOperation = true;
            base.OnFileTransfering(client, e);
        }
    }
}
