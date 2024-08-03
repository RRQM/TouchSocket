using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Sockets;
using TouchSocket.Core;

namespace TouchSocket.SerialPorts
{
    public interface ISerialPortSession: IClient, IPluginObject, ISetupConfigObject, IOnlineClient, IConnectableClient, IClosableClient
    {
    }
}
