using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 插件泛型基础事件委托
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TEventArgs"></typeparam>
    /// <param name="client"></param>
    /// <param name="e"></param>
    public delegate void PluginEventHandler<TClient,TEventArgs>(TClient client, TEventArgs e) where TEventArgs:PluginEventArgs;

    /// <summary>
    /// TouchSocket基础泛型事件委托
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TEventArgs"></typeparam>
    /// <param name="client"></param>
    /// <param name="e"></param>
    public delegate void TouchSocketEventHandler<TClient,TEventArgs>(TClient client, TEventArgs e) where TEventArgs:TouchSocketEventArgs;
}
