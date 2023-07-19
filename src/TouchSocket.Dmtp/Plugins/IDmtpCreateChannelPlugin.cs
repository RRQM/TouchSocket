using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// IDmtpCreateChannelPlugin
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public interface IDmtpCreateChannelPlugin<in TClient> : IPlugin where TClient : IDmtpActorObject
    {
        /// <summary>
        /// 在完成握手连接时。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnCreateChannel(TClient client, CreateChannelEventArgs e);
    }

    /// <summary>
    /// IDmtpCreateChannelPlugin
    /// </summary>
    public interface IDmtpCreateChannelPlugin : IDmtpCreateChannelPlugin<IDmtpActorObject>
    {

    }
}
