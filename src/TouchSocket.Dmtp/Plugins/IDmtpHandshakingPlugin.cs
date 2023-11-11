using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// IDmtpHandshakingPlugin
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public interface IDmtpHandshakingPlugin<in TClient> : IPlugin where TClient : IDmtpActorObject
    {
        /// <summary>
        /// 在Dmtp建立握手连接之前。
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