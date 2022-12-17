//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using TouchSocket.Core;
//using TouchSocket.Core;

//namespace TouchSocket.Sockets
//{
//    /// <summary>
//    /// IServicePlugin
//    /// </summary>
//    public interface IServicePlugin:IPlugin
//    {
//        /// <summary>
//        /// 当服务器成功启动时
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        [AsyncRaiser]
//        void OnStart(object sender, TouchSocketEventArgs e);

//        /// <summary>
//        /// 当服务器成功启动时
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        Task OnStartAsync(object sender, TouchSocketEventArgs e);

//        /// <summary>
//        /// 当服务器停止、释放时
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        [AsyncRaiser]
//        void OnStop(object sender, TouchSocketEventArgs e);

//        /// <summary>
//        /// 当服务器停止、释放时
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        Task OnStopAsync(object sender, TouchSocketEventArgs e);
//    }
//}
