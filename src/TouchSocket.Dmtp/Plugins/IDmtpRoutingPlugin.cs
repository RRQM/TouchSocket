using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// IDmtpRoutingPlugin
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public interface IDmtpRoutingPlugin<in TClient> : IPlugin where TClient : IDmtpActorObject
    {
        /// <summary>
        /// 当需要转发路由包时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        Task OnDmtpRouting(TClient client, PackageRouterEventArgs e);


    }

    /// <summary>
    /// IDmtpRoutingPlugin
    /// </summary>
    public interface IDmtpRoutingPlugin : IDmtpRoutingPlugin<IDmtpActorObject>
    {

    }
}
