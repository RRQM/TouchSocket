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

namespace TouchSocket.Sockets;

/// <summary>
/// Tcp命令行插件。
/// </summary>
[DynamicMethod]
public abstract class TcpCommandLinePlugin : PluginBase, ITcpReceivedPlugin
{
    private readonly ILog m_logger;
    private readonly Dictionary<string, Method> m_pairs = new Dictionary<string, TouchSocket.Core.Method>();

    /// <summary>
    /// Tcp命令行插件构造函数。
    /// 该插件初始化时，会扫描自身类定义的所有命令方法，并将它们注册到插件内部的映射中。
    /// 这允许插件在接收到命令时，能够根据命令名称找到并执行相应的处理方法。
    /// </summary>
    /// <param name="logger">用于日志记录的接口。确保外部提供的logger不为<see langword="null"/>，否则将抛出ArgumentNullException异常。</param>
    /// <exception cref="ArgumentNullException">如果logger参数为<see langword="null"/>，则抛出此异常。</exception>
    protected TcpCommandLinePlugin(ILog logger)
    {
        // 初始化成员变量m_logger，用于后续的日志记录。
        this.m_logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // 初始化数据转换器，用于处理命令的序列化和反序列化。
        this.Converter = new StringSerializerConverter(new StringToPrimitiveSerializerFormatter<object>(), new JsonStringToClassSerializerFormatter<object>());

        // 扫描当前类中所有公开实例方法，筛选出名称以"Command"结尾的方法。
        var ms = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(a => a.Name.EndsWith("Command"));

        // 将筛选出的命令方法注册到m_pairs字典中，以便后续通过命令名称调用相应的方法。
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
    /// 设置异常返回策略，当有执行异常时不返回异常。
    /// </summary>
    /// <returns>返回当前的TcpCommandLinePlugin实例，以支持链式调用。</returns>
    public TcpCommandLinePlugin NoReturnException()
    {
        // 设置是否在执行异常时返回异常的标志为<see langword="false"/>
        this.ReturnException = false;
        // 返回当前实例，以支持链式调用
        return this;
    }

    /// <inheritdoc/>
    public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
    {
        if (client is not IClientSender clientSender)
        {
            return;
        }
        try
        {
            var strs = e.Memory.Span.ToUtf8String().Split(' ');
            if (strs.Length > 0 && this.m_pairs.TryGetValue(strs[0], out var method))
            {
                var ps = method.Info.GetParameters();
                var os = new object[ps.Length];
                var index = 0;
                for (var i = 0; i < ps.Length; i++)
                {
                    if (ps[i].ParameterType.IsInterface && typeof(ITcpSession).IsAssignableFrom(ps[i].ParameterType))
                    {
                        os[i] = client;
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
                        await clientSender.SendAsync(this.Converter.Serialize(null, result)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }
                }
                catch (Exception ex)
                {
                    if (this.ReturnException)
                    {
                        await clientSender.SendAsync(ex.Message).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            this.m_logger.Log(LogLevel.Error, this, ex.Message, ex);
        }

        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }
}