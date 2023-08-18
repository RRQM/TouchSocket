using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// IDmtpHandshakedPlugin
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public interface IDmtpHandshakedPlugin<in TClient> : IPlugin where TClient : IDmtpActorObject
    {
        /// <summary>
        /// 在完成握手连接时。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnDmtpHandshaked(TClient client, DmtpVerifyEventArgs e);
    }

    /// <summary>
    /// IDmtpHandshakedPlugin
    /// </summary>
    public interface IDmtpHandshakedPlugin : IDmtpHandshakedPlugin<IDmtpActorObject>
    {

    }
}
