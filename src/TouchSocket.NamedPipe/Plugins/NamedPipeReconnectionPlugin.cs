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
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe;

/// <summary>
/// 命名管道重连插件
/// </summary>
[PluginOption(Singleton = true)]
public sealed class NamedPipeReconnectionPlugin<TClient> : ReconnectionPlugin<TClient>, INamedPipeClosedPlugin where TClient : INamedPipeClient
{
    /// <inheritdoc/>
    public override Func<TClient, int, Task<bool?>> ActionForCheck { get; set; }

    /// <summary>
    /// 构造函数，用于初始化NamedPipeReconnectionPlugin实例。
    /// </summary>
    /// <remarks>
    /// 该构造函数通过设置ActionForCheck属性来定义检查管道连接状态的操作。
    /// </remarks>
    public NamedPipeReconnectionPlugin()
    {
        // 定义一个lambda表达式，用于检查连接状态。
        // 参数c表示当前连接对象，参数i表示重试次数，但在此场景中未使用。
        // 返回连接对象的Online属性值，表示连接状态。
        this.ActionForCheck = (c, i) =>
        {
            return Task.FromResult<bool?>(c.Online);
        };
    }

    /// <inheritdoc/>
    public async Task OnNamedPipeClosed(INamedPipeSession client, ClosedEventArgs e)
    {
        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        if (client is not TClient tClient)
        {
            return;
        }

        _ = EasyTask.SafeRun(async () =>
        {
            if (e.Manual)
            {
                return;
            }

            while (true)
            {
                if (this.DisposedValue)
                {
                    return;
                }
                if (await this.ActionForConnect.Invoke(tClient).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                {
                    return;
                }
            }
        });
    }
}