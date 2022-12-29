using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Rpc.TouchRpc;

namespace FileServiceGUI
{
    internal class MyPlugin:TouchRpcPluginBase<TcpTouchRpcSocketClient>
    {
        protected override void OnFileTransfering(TcpTouchRpcSocketClient client, FileOperationEventArgs e)
        {
            base.OnFileTransfering(client, e);
        }
    }
}
