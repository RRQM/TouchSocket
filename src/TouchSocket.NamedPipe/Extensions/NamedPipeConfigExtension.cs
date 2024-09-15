//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
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
        public static readonly DependencyProperty<Func<SingleStreamDataHandlingAdapter>> NamedPipeDataHandlingAdapterProperty = new("NamedPipeDataHandlingAdapter", null
            );

        /// <summary>
        /// 直接单个配置命名管道监听的地址组。所需类型<see cref="Action"/>
        /// </summary>
        public static readonly DependencyProperty<Action<List<NamedPipeListenOption>>> NamedPipeListenOptionProperty = new("NamedPipeListenOption", null);

        /// <summary>
        /// 命名管道名称
        /// </summary>
        public static readonly DependencyProperty<string> PipeNameProperty = new("PipeName", null);

        /// <summary>
        /// 命名管道的服务主机名称。
        /// </summary>
        public static readonly DependencyProperty<string> PipeServerNameProperty = new("PipeServerName", ".");

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
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public static ReconnectionPlugin<TClient> UseNamedPipeReconnection<TClient>(this IPluginManager pluginManager) where TClient : INamedPipeClient
        {
            var reconnectionPlugin = new NamedPipeReconnectionPlugin<TClient>();
            pluginManager.Add(reconnectionPlugin);
            return reconnectionPlugin;
        }

        /// <summary>
        /// 使用命名管道断线重连。
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public static ReconnectionPlugin<INamedPipeClient> UseNamedPipeReconnection(this IPluginManager pluginManager)
        {
            var reconnectionPlugin = new NamedPipeReconnectionPlugin<INamedPipeClient>();
            pluginManager.Add(reconnectionPlugin);
            return reconnectionPlugin;
        }

        
        #endregion Reconnection
    }
}