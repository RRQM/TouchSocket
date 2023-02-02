using TouchSocket.Core;
using System.Threading.Tasks;
using System;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// IServicePlugin
    /// </summary>
    public interface IServicePlugin : IPlugin
    {
        /// <summary>
        /// 当服务器执行<see cref="IService.Start"/>后时。
        /// <para>
        /// 注意：此处并不表示服务器成功启动，具体状态请看<see cref="ServiceStateEventArgs.ServerState"/>
        /// </para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [AsyncRaiser]
        void OnStarted(object sender, ServiceStateEventArgs e);

        /// <summary>
        /// 当服务器执行<see cref="IService.Start"/>后时。
        /// <para>
        /// 注意：此处并不表示服务器成功启动，具体状态请看<see cref="ServiceStateEventArgs.ServerState"/>
        /// </para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        Task OnStartedAsync(object sender, ServiceStateEventArgs e);

        /// <summary>
        /// 当服务器调用<see cref="IService.Stop"/>或者<see cref="IDisposable.Dispose"/>时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [AsyncRaiser]
        void OnStoped(object sender, ServiceStateEventArgs e);

        /// <summary>
        /// 当服务器调用<see cref="IService.Stop"/>或者<see cref="IDisposable.Dispose"/>时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        Task OnStopedAsync(object sender, ServiceStateEventArgs e);
    }
}