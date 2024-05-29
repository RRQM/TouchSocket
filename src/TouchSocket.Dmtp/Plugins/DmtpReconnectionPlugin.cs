using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Sockets;
using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
    internal class DmtpReconnectionPlugin<TClient> :ReconnectionPlugin<TClient> where TClient :IDmtpClient
    {
        public override Func<TClient, int, Task<bool?>> ActionForCheck { get; set; }

        public DmtpReconnectionPlugin()
        {
            this.ActionForCheck = (c, i) =>
            {
                return Task.FromResult<bool?>(c.Online);
            };
        }

        protected override void Loaded(IPluginManager pluginManager)
        {
            base.Loaded(pluginManager);
            pluginManager.Add<TClient, ClosedEventArgs>(typeof(IDmtpClosedPlugin), this.OnClosed);
        }

        private async Task OnClosed(TClient client, ClosedEventArgs e)
        {
            await e.InvokeNext().ConfigureFalseAwait();
            _ = Task.Run(async () =>
            {
                if (e.Manual)
                {
                    return;
                }

                while (true)
                {
                    if (await this.ActionForConnect.Invoke(client).ConfigureFalseAwait())
                    {
                        return;
                    }
                }
            });
        }
    }
}
