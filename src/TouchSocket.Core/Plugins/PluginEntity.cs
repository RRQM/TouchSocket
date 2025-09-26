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

namespace TouchSocket.Core;

internal sealed class PluginEntity
{
    private readonly Func<object, PluginEventArgs, Task> m_invokeFunc;
    private readonly bool m_isDelegate;
    private readonly Method m_method;
    private readonly Type m_pluginType;
    private readonly PluginManager m_pluginManager;
    private readonly IPlugin m_plugin;
    private readonly Delegate m_sourceDelegate;
    private readonly bool m_fromIoc;

    public PluginEntity(Func<object, PluginEventArgs, Task> invokeFunc, Delegate sourceDelegate)
    {
        this.m_isDelegate = true;
        this.m_fromIoc = false;
        this.m_invokeFunc = invokeFunc;
        this.m_sourceDelegate = sourceDelegate;
    }

    public PluginEntity(Method method, IPlugin plugin)
    {
        this.m_isDelegate = false;
        this.m_fromIoc = false;
        this.m_method = method;
        this.m_plugin = plugin;
    }

    public PluginEntity(Method method, Type pluginType, PluginManager pluginManager)
    {
        this.m_isDelegate = false;
        this.m_fromIoc = true;
        this.m_method = method;
        this.m_pluginType = pluginType;
        this.m_pluginManager = pluginManager;
    }

    public bool IsDelegate => this.m_isDelegate;
    public Method Method => this.m_method;
    public IPlugin Plugin => this.m_plugin;
    public bool FromIoc => this.m_fromIoc;
    public Delegate SourceDelegate => this.m_sourceDelegate;

    public async Task Run(IResolver resolver, object sender, PluginEventArgs e)
    {
        if (this.m_isDelegate)
        {
            await this.m_invokeFunc.Invoke(sender, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else if (this.m_fromIoc)
        {
            var plugin = (IPlugin)resolver.Resolve(this.m_pluginType) ?? throw new Exception(Resources.TouchSocketCoreResource.PluginIsNull.Format(this.m_pluginType));

            plugin.Loaded(this.m_pluginManager);
            await this.m_method.InvokeAsync(plugin, sender, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            plugin.Unloaded(this.m_pluginManager);
        }
        else
        {
            await this.m_method.InvokeAsync(this.m_plugin, sender, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }
}