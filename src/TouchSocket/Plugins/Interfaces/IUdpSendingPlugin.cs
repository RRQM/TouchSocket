using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    public interface IUdpSendingPlugin:IPlugin
    {
        /// <summary>
        /// 当即将发送数据时，调用该方法在适配器之后，接下来即会发送数据。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnUdpSending(IUdpSessionBase client, UdpSendingEventArgs e);
    }
}
