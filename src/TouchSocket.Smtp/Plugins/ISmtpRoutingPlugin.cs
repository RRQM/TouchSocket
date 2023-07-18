using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Smtp
{
    /// <summary>
    /// ISmtpRoutingPlugin
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public interface ISmtpRoutingPlugin<in TClient>:IPlugin where TClient: ISmtpActorObject
    {
        /// <summary>
        /// 当需要转发路由包时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        Task OnSmtpRouting(TClient client, PackageRouterEventArgs e);


    }

    /// <summary>
    /// ISmtpRoutingPlugin
    /// </summary>
    public interface ISmtpRoutingPlugin : ISmtpRoutingPlugin<ISmtpActorObject>
    {

    }
}
