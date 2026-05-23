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

using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchSocket.Mcp;

/// <summary>
/// 基于标准输入/输出（stdio）的 MCP 服务端，读取输入流并写入输出流。
/// </summary>
public sealed class McpStdioServer : ServiceBase
{
    private McpActor m_actor;
    private TextReader m_reader;
    private TextWriter m_writer;
    private readonly SemaphoreSlim m_writeLock = new SemaphoreSlim(1, 1);
    private CancellationTokenSource m_cancellationTokenSource;
    private Task m_runningTask;
    private ServerState m_serverState;

    /// <summary>
    /// 初始化 <see cref="McpStdioServer"/> 的新实例。
    /// </summary>
    public McpStdioServer()
    {
        this.m_serverState = ServerState.None;
    }

    /// <inheritdoc/>
    public override ServerState ServerState => this.m_serverState;

    /// <summary>
    /// 使用 stdio 服务端选项完成配置。
    /// </summary>
    /// <param name="options">stdio 服务端选项。</param>
    public Task SetupAsync(McpStdioServerOptions options)
    {
        var config = new TouchSocketConfig();
        config.SetValue(McpConfigExtension.McpStdioServerOptionsProperty, options);
        return this.SetupAsync(config);
    }

    /// <inheritdoc/>
    public override Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (this.m_actor == null)
        {
            throw new InvalidOperationException("McpStdioServer has not been setup.");
        }

        if (this.m_serverState == ServerState.Running)
        {
            return Task.CompletedTask;
        }

        this.m_cancellationTokenSource?.Cancel();
        this.m_cancellationTokenSource?.Dispose();
        this.m_cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        this.m_serverState = ServerState.Running;
        this.m_runningTask = this.RunLoopAsync(this.m_cancellationTokenSource.Token);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override async Task<Result> StopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            this.m_serverState = ServerState.Stopped;
            this.m_cancellationTokenSource?.Cancel();

            if (this.m_runningTask != null)
            {
                await this.m_runningTask.ConfigureAwait(false);
            }

            this.m_cancellationTokenSource?.Dispose();
            this.m_cancellationTokenSource = null;
            this.m_runningTask = null;
            return Result.Success;
        }
        catch (Exception ex)
        {
            this.m_serverState = ServerState.Exception;
            return Result.FromException(ex);
        }
    }

    /// <inheritdoc/>
    protected override void LoadConfig(TouchSocketConfig config)
    {
        base.LoadConfig(config);

        var options = config.GetValue(McpConfigExtension.McpStdioServerOptionsProperty) ?? new McpStdioServerOptions();
        var rpcServerProvider = this.Resolver.Resolve<IRpcServerProvider>();
        if (rpcServerProvider == null)
        {
            throw new InvalidOperationException("IRpcServerProvider is required for McpStdioServer.");
        }

        this.m_reader = options.Reader ?? Console.In;
        this.m_writer = options.Writer ?? Console.Out;
        this.m_actor = new McpActor(options.ServerOptions ?? new McpServerOptions());
        this.m_actor.Resolver = this.Resolver;
        this.m_actor.Logger = this.Logger;
        this.m_actor.SendAction = this.SendAsync;
        this.m_actor.SetRpcServerProvider(rpcServerProvider);
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            this.StopAsync().GetFalseAwaitResult();
            this.m_cancellationTokenSource?.Dispose();
            this.m_actor?.SafeDispose();
        }
        base.SafetyDispose(disposing);
    }

    private async Task RunLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                string line;
                try
                {
                    line = await this.m_reader.ReadLineAsync().ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (line == null)
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var bytes = Encoding.UTF8.GetBytes(line);
                var callContext = new McpStdioCallContext(this, cancellationToken);
                await this.m_actor.InputReceiveAsync(bytes, callContext).ConfigureAwait(false);
            }
        }
        catch
        {
            this.m_serverState = ServerState.Exception;
            throw;
        }
        finally
        {
            if (this.m_serverState == ServerState.Running)
            {
                this.m_serverState = ServerState.Stopped;
            }
        }
    }

    private async Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        var line = Encoding.UTF8.GetString(data.ToArray());

        await this.m_writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await this.m_writer.WriteLineAsync(line).ConfigureAwait(false);
            await this.m_writer.FlushAsync().ConfigureAwait(false);
        }
        finally
        {
            this.m_writeLock.Release();
        }
    }
}
