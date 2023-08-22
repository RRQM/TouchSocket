using System;
using System.Collections.Generic;
using TouchSocket.Core;

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
    }
}