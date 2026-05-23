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

using System.Diagnostics;
using TouchSocket.Core;

namespace TouchSocket.Mcp;

/// <summary>
/// 基于标准输入/输出（stdio）的 MCP 客户端，通过启动子进程或连接已有进程进行通信。
/// </summary>
public sealed class McpStdioClient : SetupConfigObject, IMcpClient, IDisposable
{
    private readonly McpClientBaseImpl m_clientBase = new McpClientBaseImpl();
    private Process m_process;
    private readonly SemaphoreSlim m_writeLock = new SemaphoreSlim(1, 1);
    private CancellationTokenSource m_readCts;
    private bool m_disposed;
    private bool m_connected;
    private bool m_killOnDispose;
    private McpStdioClientOptions m_options;

    /// <summary>
    /// 使用指定选项完成配置。
    /// </summary>
    /// <param name="options">stdio 客户端选项。</param>
    public Task SetupAsync(McpStdioClientOptions options)
    {
        var config = new TouchSocketConfig();
        config.SetValue(McpConfigExtension.McpStdioClientOptionsProperty, options);
        return this.SetupAsync(config);
    }

    /// <summary>
    /// 建立 stdio 连接。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (this.m_options == null)
        {
            throw new InvalidOperationException("McpStdioClient has not been setup.");
        }

        if (this.m_connected)
        {
            return Task.CompletedTask;
        }

        if (this.m_options.StartInfo != null)
        {
            var startInfo = this.m_options.StartInfo;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = false;

            this.m_process = Process.Start(startInfo);
            if (this.m_process == null)
            {
                throw new InvalidOperationException("Failed to start MCP server process.");
            }
        }
        else
        {
            this.m_process = this.m_options.Process ?? throw new InvalidOperationException("Either StartInfo or Process must be configured.");
        }

        this.StartReading(this.m_process.StandardOutput, cancellationToken);
        this.m_connected = true;
        this.m_clientBase.SetConnected(true);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<McpInitializeResult> InitializeAsync(CancellationToken cancellationToken = default)
    {
        return this.m_clientBase.InitializeAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListToolsResult> ListToolsAsync(CancellationToken cancellationToken = default)
    {
        return this.m_clientBase.ListToolsAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpCallToolResult> CallToolAsync(string name, Dictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
    {
        return this.m_clientBase.CallToolAsync(name, arguments, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListResourcesResult> ListResourcesAsync(CancellationToken cancellationToken = default)
    {
        return this.m_clientBase.ListResourcesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListResourceTemplatesResult> ListResourceTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return this.m_clientBase.ListResourceTemplatesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpReadResourceResult> ReadResourceAsync(string uri, CancellationToken cancellationToken = default)
    {
        return this.m_clientBase.ReadResourceAsync(uri, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListPromptsResult> ListPromptsAsync(CancellationToken cancellationToken = default)
    {
        return this.m_clientBase.ListPromptsAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpGetPromptResult> GetPromptAsync(string name, Dictionary<string, string> arguments = null, CancellationToken cancellationToken = default)
    {
        return this.m_clientBase.GetPromptAsync(name, arguments, cancellationToken);
    }

    /// <inheritdoc/>
    protected override void LoadConfig(TouchSocketConfig config)
    {
        this.m_options = config.GetValue(McpConfigExtension.McpStdioClientOptionsProperty) ?? new McpStdioClientOptions();
        this.m_killOnDispose = this.m_options.KillOnDispose;
        this.m_connected = false;
        this.m_clientBase.Bind(this.SendDataAsync, this.m_options.ClientOptions);
        base.LoadConfig(config);
    }

    private sealed class McpClientBaseImpl : McpClientBase
    {
        private Func<ReadOnlyMemory<byte>, CancellationToken, Task> m_sendAction;
        private bool m_connected;
        private bool m_setup;

        public void Bind(Func<ReadOnlyMemory<byte>, CancellationToken, Task> sendAction, McpClientOptions options)
        {
            this.m_sendAction = sendAction;
            this.SetOptions(options);
            this.m_setup = true;
            this.m_connected = false;
        }

        public void SetConnected(bool connected)
        {
            this.m_connected = connected;
        }

        public void Receive(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            this.OnReceiveData(data, cancellationToken);
        }

        protected override Task SendDataAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            return this.m_sendAction(data, cancellationToken);
        }

        protected override void ThrowIfNotConnected()
        {
            this.ThrowIfNotSetup();
            if (!this.m_connected)
            {
                throw new InvalidOperationException("McpStdioClient has not connected.");
            }
        }

        protected override void ThrowIfNotSetup()
        {
            if (!this.m_setup)
            {
                throw new InvalidOperationException("McpStdioClient has not been setup.");
            }
        }
    }

    private async Task SendDataAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        var line = Encoding.UTF8.GetString(data.ToArray());

        await this.m_writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await this.m_process.StandardInput.WriteLineAsync(line).ConfigureAwait(false);
            await this.m_process.StandardInput.FlushAsync().ConfigureAwait(false);
        }
        finally
        {
            this.m_writeLock.Release();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (this.m_disposed)
        {
            return;
        }

        this.m_disposed = true;
        this.m_connected = false;
        this.m_clientBase.SetConnected(false);
        this.m_readCts?.Cancel();
        this.m_readCts?.Dispose();

        try
        {
            if (this.m_killOnDispose && this.m_process != null && !this.m_process.HasExited)
            {
                this.m_process.Kill();
            }
        }
        catch
        {
        }

        this.m_process?.Dispose();
    }

    private void StartReading(StreamReader reader, CancellationToken cancellationToken)
    {
        this.m_readCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var ct = this.m_readCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (line == null)
                    {
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    var bytes = Encoding.UTF8.GetBytes(line);
                    this.m_clientBase.Receive(bytes, ct);
                }
            }
            catch (OperationCanceledException)
            {
                // normal shutdown
            }
        }, ct);
    }
}
