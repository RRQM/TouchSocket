using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Smtp
{
    /// <summary>
    /// ISmtpCreateChannelPlugin
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public interface ISmtpCreateChannelPlugin<in TClient> : IPlugin where TClient : ISmtpActorObject
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
    /// ISmtpCreateChannelPlugin
    /// </summary>
    public interface ISmtpCreateChannelPlugin : ISmtpCreateChannelPlugin<ISmtpActorObject>
    {

    }
}
