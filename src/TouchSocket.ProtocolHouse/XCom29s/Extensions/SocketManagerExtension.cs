using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.ProtocolHouse;

public static class SocketManagerExtension
{
    public static ReconnectionPlugin<ITcpClient> UseSocketReconnection(this IPluginManager pluginsManager, CommClientSocket<IPlugin> clientSocket, TimeSpan sleepTime,
        Func<ITcpClient, int, Exception, bool> failCallback = default,
        Action<ITcpClient> successCallback = default)
    {
        ReconnectionPlugin<ITcpClient> reconnectionPlugin = null!;
        if (sleepTime.TotalMilliseconds > 0)
        {
            reconnectionPlugin = pluginsManager.UseReconnection(sleepTime, (client, n, ex) =>
            {
                var result = failCallback?.Invoke(client, n, ex);
                return !clientSocket.IsStoppable || (bool)result!;//全部返回false时会退出重连循环
            }, successCallback)
                    .UsePolling()
                    .SetActionForCheck(async (client, failCount) =>
                    {
                        bool? result = false;
                        result = clientSocket.IsStoppable || (bool)(client?.Online!);//返回true不会再次连接
                        if (clientSocket.IsStoppable) await Task.Delay(10);
                        if ((bool)client?.Online!)
                        {
                            //发送心跳包
                            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}]正在发送心跳包[{client.RemoteIPHost.EndPoint.ToString()}]......");
                        }
                        return await Task.FromResult(result);
                    });
        }
        return reconnectionPlugin;
    }
}