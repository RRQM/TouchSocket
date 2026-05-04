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
using System.Reflection;
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace TouchSocket.WebApi.Swagger;

/// <summary>
/// SwaggerPlugin，负责提供 Swagger UI 静态页面。
/// 需与 <see cref="OpenApiPlugin"/> 配合使用（<see cref="SwaggerPluginManagerExtension.UseSwagger(IPluginManager)"/> 会自动添加两者）。
/// </summary>
[PluginOption(Singleton = true)]
internal sealed class SwaggerPlugin : PluginBase, IServerStartedPlugin, IHttpPlugin
{
    private readonly ILog m_logger;
    private readonly SwaggerOption m_options;
    private readonly Dictionary<string, ReadOnlyMemory<byte>> m_resources = new();

    /// <summary>
    /// 初始化 <see cref="SwaggerPlugin"/> 的新实例。
    /// </summary>
    public SwaggerPlugin(ILog logger, SwaggerOption options)
    {
        this.m_logger = logger;
        this.m_options = options;
    }

    /// <inheritdoc/>
    public async Task OnServerStarted(IServiceBase sender, ServiceStateEventArgs e)
    {
        if (e.ServerState != ServerState.Running)
        {
            await e.InvokeNext().ConfigureDefaultAwait();
            return;
        }

        var openApiPlugin = sender.PluginManager.Plugins.OfType<OpenApiPlugin>().FirstOrDefault();
        if (openApiPlugin == null)
        {
            this.m_logger?.Warning($"该服务器中似乎没有添加{nameof(OpenApiPlugin)}，Swagger UI 的 JSON 端点可能无法访问。");
        }

        this.LoadUiResources(openApiPlugin);

        await e.InvokeNext().ConfigureDefaultAwait();

        if (this.m_options.LaunchBrowser)
        {
            this.OpenBrowserToSwagger(sender);
        }
    }

    /// <inheritdoc/>
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        var url = e.Context.Request.RelativeURL;
        if (this.m_resources.TryGetValue(url, out var bytes))
        {
            e.Handled = true;
            e.Context.Response
                .SetStatusWithSuccess()
                .SetContentTypeByExtension(Path.GetExtension(url))
                .SetContent(bytes);
            await e.Context.Response.AnswerAsync().ConfigureDefaultAwait();
            return;
        }

        await e.InvokeNext().ConfigureDefaultAwait();
    }

    private void LoadUiResources(OpenApiPlugin openApiPlugin)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var prefix = GetPrefixPath(this.m_options.Prefix);

        foreach (var name in assembly.GetManifestResourceNames())
        {
            var fileName = name.Replace("TouchSocket.WebApi.Swagger.api.", string.Empty);

            // openapi.json 由 OpenApiPlugin 负责，不在此处注册
            if (fileName == "openapi.json")
            {
                continue;
            }

            try
            {
                using var stream = assembly.GetManifestResourceStream(name);
                ReadOnlyMemory<byte> bytes = stream.ReadAllToByteArray();
                var key = prefix.Length == 0 ? $"/{fileName}" : $"{prefix}/{fileName}";
                this.m_resources[key] = bytes;
            }
            catch (Exception ex)
            {
                this.m_logger?.Exception(this, ex);
            }
        }
    }

    private void OpenBrowserToSwagger(IServiceBase sender)
    {
        try
        {
            var monitor = (sender as ITcpServiceBase)?.Monitors?.FirstOrDefault();
            if (monitor == null)
            {
                return;
            }

            var iphost = monitor.Option.IpHost;
            string host;
            if (iphost.IsLoopback || iphost.DnsSafeHost == "127.0.0.1" || iphost.DnsSafeHost == "0.0.0.0")
            {
                host = "127.0.0.1";
            }
            else
            {
                host = iphost.DnsSafeHost;
            }

            var scheme = monitor.Option.UseSsl ? "https" : "http";
            var prefix = GetPrefixPath(this.m_options.Prefix);
            var index = prefix.Length == 0 ? "/index.html" : $"{prefix}/index.html";

            var psi = new ProcessStartInfo
            {
                FileName = $"{scheme}://{host}:{iphost.Port}{index}",
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            this.m_logger?.Debug(this, ex);
        }
    }

    private static string GetPrefixPath(string prefix)
    {
        return prefix.IsNullOrEmpty() ? string.Empty
            : (prefix.StartsWith("/") ? prefix : $"/{prefix}");
    }
}
