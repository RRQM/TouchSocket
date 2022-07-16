using System;
using TouchSocket.Sockets;
using TouchSocket.Sockets.Plugins;

namespace TouchSocket.Core.Plugins
{
    /// <summary>
    /// IPluginsManagerExtension
    /// </summary>
    public static class PluginsManagerExtension
    {
        /// <summary>
        /// 使用断线重连。
        /// <para>该效果仅客户端在完成首次连接，且为被动断开时有效。</para>
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <param name="successCallback">成功回调函数</param>
        /// <param name="tryCount">尝试重连次数，设为-1时则永远尝试连接</param>
        /// <param name="printLog">是否输出日志。</param>
        /// <param name="sleepTime">失败时，停留时间</param>
        /// <returns></returns>
        public static IPluginsManager UseReconnection(this IPluginsManager pluginsManager, int tryCount = 10, bool printLog = false, int sleepTime = 1000, Action<ITcpClient> successCallback = null)
        {
            pluginsManager.Add(new ReconnectionPlugin<ITcpClient>(tryCount, printLog, sleepTime, successCallback));
            return pluginsManager;
        }
    }
}
