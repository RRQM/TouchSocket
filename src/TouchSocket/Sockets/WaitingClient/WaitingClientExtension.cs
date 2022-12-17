using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// WaitingClientExtensions
    /// </summary>
    public static class WaitingClientExtension
    {
        /// <summary>
        /// WaitingClient
        /// </summary>
        public static readonly IDependencyProperty<object> WaitingClientProperty =
            DependencyProperty<object>.Register("WaitingClient", typeof(WaitingClientExtension), null);

        /// <summary>
        /// 获取可等待的客户端。
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="waitingOptions"></param>
        /// <returns></returns>
        public static IWaitingClient<TClient> GetWaitingClient<TClient>(this TClient client, WaitingOptions waitingOptions = WaitingOptions.AllAdapter) where TClient : IClient, IDefaultSender, ISender
        {
            if (client.GetValue(WaitingClientProperty) is IWaitingClient<TClient> c1)
            {
                c1.WaitingOptions = waitingOptions;
                return c1;
            }

            WaitingClient<TClient> waitingClient = new WaitingClient<TClient>(client, waitingOptions);
            client.SetValue(WaitingClientProperty, waitingClient);
            return waitingClient;
        }
    }
}
