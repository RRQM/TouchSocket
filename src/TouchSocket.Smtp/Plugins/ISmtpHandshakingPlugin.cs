using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Smtp
{
    /// <summary>
    /// ISmtpHandshakedPlugin
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public interface ISmtpHandshakingPlugin<in TClient>:IPlugin where TClient : ISmtpActorObject
    {
        /// <summary>
        /// 在验证Token时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnSmtpHandshaking(TClient client, SmtpVerifyEventArgs e);
    }

    /// <summary>
    /// ISmtpHandshakingPlugin
    /// </summary>
    public interface ISmtpHandshakingPlugin : ISmtpHandshakingPlugin<ISmtpActorObject>
    {

    }
}
