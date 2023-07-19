using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// IDmtpHandshakedPlugin
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public interface IDmtpHandshakingPlugin<in TClient>:IPlugin where TClient : IDmtpActorObject
    {
        /// <summary>
        /// 在验证Token时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnDmtpHandshaking(TClient client, DmtpVerifyEventArgs e);
    }

    /// <summary>
    /// IDmtpHandshakingPlugin
    /// </summary>
    public interface IDmtpHandshakingPlugin : IDmtpHandshakingPlugin<IDmtpActorObject>
    {

    }
}
