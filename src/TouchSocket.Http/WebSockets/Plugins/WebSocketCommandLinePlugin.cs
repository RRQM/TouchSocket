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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http.WebSockets;

/// <summary>
/// WS命令行插件。
/// </summary>
[DynamicMethod]
public abstract class WebSocketCommandLinePlugin : PluginBase, IWebSocketReceivedPlugin
{
    private readonly ILog m_logger;
    private readonly Dictionary<string, Method> m_pairs = new Dictionary<string, Method>();

    /// <summary>
    /// WSCommandLinePlugin
    /// </summary>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected WebSocketCommandLinePlugin(ILog logger)
    {
        this.m_logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.Converter = new StringSerializerConverter(new StringToPrimitiveSerializerFormatter<object>(), new JsonStringToClassSerializerFormatter<object>());
        var ms = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(a => a.Name.EndsWith("Command"));
        foreach (var item in ms)
        {
            this.m_pairs.Add(item.Name.Replace("Command", string.Empty), new Method(item));
        }
    }

    /// <summary>
    /// 字符串转换器，默认支持基础类型和Json。可以自定义。
    /// </summary>
    public StringSerializerConverter Converter { get; }

    /// <summary>
    /// 是否返回执行异常。
    /// </summary>
    public bool ReturnException { get; set; } = true;

    /// <summary>
    /// 当有执行异常时，不返回异常。
    /// </summary>
    /// <returns></returns>
    public WebSocketCommandLinePlugin NoReturnException()
    {
        this.ReturnException = false;
        return this;
    }

    /// <inheritdoc/>
    public async Task OnWebSocketReceived(IWebSocket webSocket, WSDataFrameEventArgs e)
    {
        if (e.DataFrame.Opcode != WSDataType.Text)
        {
            await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }
        var strs = e.DataFrame.ToText().Split(' ');
        if (!this.m_pairs.TryGetValue(strs[0], out var method))
        {
            await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }
        var ps = method.Info.GetParameters();
        var os = new object[ps.Length];
        var index = 0;
        for (var i = 0; i < ps.Length; i++)
        {
            if (ps[i].ParameterType.IsInterface)
            {
                if (typeof(IHttpSession).IsAssignableFrom(ps[i].ParameterType))
                {
                    os[i] = webSocket.Client;
                }
                else if (typeof(IWebSocket).IsAssignableFrom(ps[i].ParameterType))
                {
                    os[i] = webSocket;
                }
            }
            else
            {
                os[i] = this.Converter.Deserialize(null, strs[index + 1], ps[i].ParameterType);
                index++;
            }
        }

        e.Handled = true;

        try
        {
            var result = await method.InvokeAsync(this, os).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            if (method.HasReturn)
            {
                await webSocket.SendAsync(this.Converter.Serialize(null, result)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
        catch (Exception ex)
        {
            if (this.ReturnException)
            {
                await webSocket.SendAsync(this.Converter.Serialize(null, ex.Message)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
    }
}