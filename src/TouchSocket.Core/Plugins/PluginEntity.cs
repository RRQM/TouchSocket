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
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    internal sealed class PluginEntity
    {
        private readonly Func<object, PluginEventArgs, Task> m_invokeFunc;
        private readonly bool m_isDelegate;
        private readonly Method m_method;
        private readonly IPlugin m_plugin;
        private readonly Delegate m_sourceDelegate;

        public PluginEntity(Func<object, PluginEventArgs, Task> invokeFunc, Delegate sourceDelegate)
        {
            this.m_isDelegate = true;
            this.m_invokeFunc = invokeFunc;
            this.m_sourceDelegate = sourceDelegate;
        }

        public PluginEntity(Method method, IPlugin plugin)
        {
            this.m_isDelegate = false;
            this.m_method = method;
            this.m_plugin = plugin;
        }

        public bool IsDelegate => this.m_isDelegate;
        public Method Method => this.m_method;
        public IPlugin Plugin => this.m_plugin;

        public Delegate SourceDelegate => this.m_sourceDelegate;

        public Task Run(object sender, PluginEventArgs e)
        {
            if (this.m_isDelegate)
            {
                return this.m_invokeFunc.Invoke(sender, e);
            }
            else
            {
                return this.m_method.InvokeAsync(this.m_plugin, sender, e);
            }
        }
    }
}