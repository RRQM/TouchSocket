using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace FileServiceGUI
{
    class MyPlugin : TouchRpcPluginBase<TcpTouchRpcSocketClient>
    {
        protected override void OnFileTransfering(TcpTouchRpcSocketClient client, FileOperationEventArgs e)
        {
            e.IsPermitOperation = true;//运行操作

            //有可能是上传，也有可能是下载
            client.Logger.Info($"有客户端请求传输文件，ID={client.ID}，请求类型={e.TransferType}，请求文件名={e.ResourcePath}");
        }

        protected override void OnFileTransfered(TcpTouchRpcSocketClient client, FileTransferStatusEventArgs e)
        {
            //传输结束，但是不一定成功，需要从e.Result判断状态。
            client.Logger.Info($"客户端传输文件结束，ID={client.ID}，请求类型={e.TransferType}，文件名={e.ResourcePath}，请求状态={e.Result}");
        }

        protected override void OnHandshaked(TcpTouchRpcSocketClient client, VerifyOptionEventArgs e)
        {
            client.Logger.Info($"有客户端成功验证，ID={client.ID}");
        }

        protected override void OnDisconnected(TcpTouchRpcSocketClient client, ClientDisconnectedEventArgs e)
        {
            client.Logger.Info($"有客户端断开，ID={client.ID}");
            base.OnDisconnected(client, e);
        }
    }
}
