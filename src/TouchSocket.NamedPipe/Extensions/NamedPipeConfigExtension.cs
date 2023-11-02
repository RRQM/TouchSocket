using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// NamedPipeConfigExtension
    /// </summary>
    public static class NamedPipeConfigExtension
    {
        /// <summary>
        /// 数据处理适配器，默认为获取<see cref="NormalDataHandlingAdapter"/>
        /// 所需类型<see cref="Func{TResult}"/>
        /// </summary>
        public static readonly DependencyProperty<Func<SingleStreamDataHandlingAdapter>> NamedPipeDataHandlingAdapterProperty = DependencyProperty<Func<SingleStreamDataHandlingAdapter>>.Register("NamedPipeDataHandlingAdapter", () => { return new NormalDataHandlingAdapter(); });

        /// <summary>
        /// 直接单个配置命名管道监听的地址组。所需类型<see cref="Action"/>
        /// </summary>
        public static readonly DependencyProperty<Action<List<NamedPipeListenOption>>> NamedPipeListenOptionProperty = DependencyProperty<Action<List<NamedPipeListenOption>>>.Register("NamedPipeListenOption", null);

        /// <summary>
        /// 命名管道名称
        /// </summary>
        public static readonly DependencyProperty<string> PipeNameProperty = DependencyProperty<string>.Register("PipeName", null);

        /// <summary>
        /// 命名管道的服务主机名称。
        /// </summary>
        public static readonly DependencyProperty<string> PipeServerNameProperty = DependencyProperty<string>.Register("PipeServerName", ".");

        /// <summary>
        /// 设置(命名管道系)数据处理适配器。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetNamedPipeDataHandlingAdapter(this TouchSocketConfig config, Func<SingleStreamDataHandlingAdapter> value)
        {
            config.SetValue(NamedPipeDataHandlingAdapterProperty, value);
            return config;
        }

        /// <summary>
        /// 直接单个配置命名管道监听的地址组。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetNamedPipeListenOptions(this TouchSocketConfig config, Action<List<NamedPipeListenOption>> value)
        {
            config.SetValue(NamedPipeListenOptionProperty, value);
            return config;
        }

        /// <summary>
        /// 当管道在本机时，仅设置管道名称即可。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetPipeName(this TouchSocketConfig config, string value)
        {
            config.SetValue(PipeNameProperty, value);
            return config;
        }

        /// <summary>
        /// 设置命名管道的主机名称。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetPipeServer(this TouchSocketConfig config, string value)
        {
            config.SetValue(PipeServerNameProperty, value);
            return config;
        }

        #region Reconnection

        /// <summary>
        /// 使用命名管道断线重连。
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="pluginsManager"></param>
        /// <returns></returns>
        public static NamedPipeReconnectionPlugin<TClient> UseNamedPipeReconnection<TClient>(this IPluginsManager pluginsManager) where TClient : class, INamedPipeClient
        {
            var reconnectionPlugin = new NamedPipeReconnectionPlugin<TClient>();
            pluginsManager.Add(reconnectionPlugin);
            return reconnectionPlugin;
        }

        /// <summary>
        /// 使用命名管道断线重连。
        /// <para>该效果仅客户端在完成首次连接，且为被动断开时有效。</para>
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <param name="successCallback">成功回调函数</param>
        /// <param name="tryCount">尝试重连次数，设为-1时则永远尝试连接</param>
        /// <param name="printLog">是否输出日志。</param>
        /// <param name="sleepTime">失败时，停留时间</param>
        /// <returns></returns>
        public static NamedPipeReconnectionPlugin<INamedPipeClient> UseNamedPipeReconnection(this IPluginsManager pluginsManager, int tryCount = 10, bool printLog = false, int sleepTime = 1000, Action<INamedPipeClient> successCallback = null)
        {
            var reconnectionPlugin = new NamedPipeReconnectionPlugin<INamedPipeClient>();
            reconnectionPlugin.SetConnectAction(async client =>
            {
                var tryT = tryCount;
                while (tryCount < 0 || tryT-- > 0)
                {
                    try
                    {
                        if (client.Online)
                        {
                            return true;
                        }
                        else
                        {
                            await Task.Delay(1000);
                            await client.ConnectAsync();
                        }
                        successCallback?.Invoke(client);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        if (printLog)
                        {
                            client.Logger.Log(LogLevel.Error, client, "断线重连失败。", ex);
                        }
                        await Task.Delay(sleepTime);
                    }
                }
                return true;
            });
            pluginsManager.Add(reconnectionPlugin);
            return reconnectionPlugin;
        }

        /// <summary>
        /// 使用命名管道断线重连。
        /// <para>该效果仅客户端在完成首次连接，且为被动断开时有效。</para>
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <param name="sleepTime">失败时间隔时间</param>
        /// <param name="failCallback">失败时回调（参数依次为：客户端，本轮尝试重连次数，异常信息）。如果回调为null或者返回false，则终止尝试下次连接。</param>
        /// <param name="successCallback">成功连接时回调。</param>
        /// <returns></returns>
        public static NamedPipeReconnectionPlugin<INamedPipeClient> UseNamedPipeReconnection(this IPluginsManager pluginsManager, TimeSpan sleepTime,
            Func<INamedPipeClient, int, Exception, bool> failCallback = default,
            Action<INamedPipeClient> successCallback = default)
        {
            var reconnectionPlugin = new NamedPipeReconnectionPlugin<INamedPipeClient>();
            reconnectionPlugin.SetConnectAction(async client =>
            {
                var tryT = 0;
                while (true)
                {
                    try
                    {
                        if (client.Online)
                        {
                            return true;
                        }
                        else
                        {
                            await Task.Delay(1000);
                            await client.ConnectAsync();
                        }

                        successCallback?.Invoke(client);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await Task.Delay(sleepTime);
                        if (failCallback?.Invoke(client, ++tryT, ex) != true)
                        {
                            return true;
                        }
                    }
                }
            });
            pluginsManager.Add(reconnectionPlugin);
            return reconnectionPlugin;
        }
        #endregion
    }
}