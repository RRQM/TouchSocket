using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;

namespace CustomDmtpActorConsoleApp.SimpleDmtpRpc
{
    class SimpleDmtpRpcFeature : PluginBase, IDmtpHandshakingPlugin, IDmtpReceivedPlugin
    {
        readonly Dictionary<string, MethodModel> m_pairs = new Dictionary<string, MethodModel>();
        public async Task OnDmtpHandshaking(IDmtpActorObject client, DmtpVerifyEventArgs e)
        {
            var actor = new SimpleDmtpRpcActor(client.DmtpActor)
            {
                TryFindMethod = this.TryFindMethod
            };
            client.DmtpActor.SetSimpleDmtpRpcActor(actor);
            await e.InvokeNext();
        }

        private MethodModel TryFindMethod(string methodName)
        {
            if (this.m_pairs.TryGetValue(methodName, out var methodModel))
            {
                return methodModel;
            }
            return default;
        }

        public void RegisterRpc(object server)
        {
            if (server is null)
            {
                throw new ArgumentNullException(nameof(server));
            }

            foreach (var item in server.GetType().GetMethods(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public))
            {
                m_pairs.Add(item.Name, new MethodModel(item, server));
            }
        }

        public async Task OnDmtpReceived(IDmtpActorObject client, DmtpMessageEventArgs e)
        {
            if (client.DmtpActor.GetSimpleDmtpRpcActor() is SimpleDmtpRpcActor actor)
            {
                if (await actor.InputReceivedData(e.DmtpMessage))
                {
                    e.Handled = true;
                    return;
                }
            }
            await e.InvokeNext();
        }
    }

}
