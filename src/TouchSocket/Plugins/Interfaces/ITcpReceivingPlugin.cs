using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// ITcpReceivingPlugin
    /// </summary>
    public interface ITcpReceivingPlugin<in TClient>:IPlugin where TClient : ITcpClientBase
    {
        /// <summary>
        /// 在刚收到数据时触发，即在适配器之前。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnTcpReceiving(TClient client, ByteBlockEventArgs e);
    }

    /// <summary>
    /// ITcpReceivingPlugin
    /// </summary>
    public interface ITcpReceivingPlugin: ITcpReceivingPlugin<ITcpClientBase>
    { 
    
    }
}
