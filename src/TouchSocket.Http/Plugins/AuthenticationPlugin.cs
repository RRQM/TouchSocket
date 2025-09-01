// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http;

/// <summary>
/// Basic auth 认证插件
/// </summary>
/// <remarks>
/// PR https://github.com/RRQM/TouchSocket/pull/79 by <see href="https://github.com/kimdiego2098">Diego2098</see>
/// </remarks>
public sealed class AuthenticationPlugin : PluginBase, IHttpPlugin
{
    public string Password { get; set; } = "111111";
    public string Realm { get; set; } = "Server";
    public string UserName { get; set; } = "admin";

    public Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        var authorizationHeader = e.Context.Request.Headers["Authorization"];

        if (string.IsNullOrEmpty(authorizationHeader))
        {
            return this.Challenge(e, "Empty Authorization Header");
        }

        if (!authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return this.Challenge(e, "Invalid Authorization Header");
        }

        var authBase64 = authorizationHeader.Substring("Basic ".Length).Trim();

        string authString;
        try
        {
            authString = Encoding.UTF8.GetString(Convert.FromBase64String(authBase64));
        }
        catch
        {
            return this.Challenge(e, "Invalid Base64 Authorization Header");
        }

        var credentials = authString.Split(':');
        if (credentials.Length != 2)
        {
            return this.Challenge(e, "Invalid Authorization Header");
        }

        var username = credentials[0];
        var password = credentials[1];

        if (username != this.UserName || password != this.Password)
        {
            return this.Challenge(e, "Invalid Username or Password");
        }

        // 验证通过，继续下一个中间件或处理器
        return e.InvokeNext();
    }

    public AuthenticationPlugin SetCredentials(string userName, string password)
    {
        this.UserName = userName;
        this.Password = password;
        return this;
    }

    public AuthenticationPlugin SetRealm(string realm = "Server")
    {
        this.Realm = realm;
        return this;
    }

    private Task Challenge(HttpContextEventArgs e, string message)
    {
        e.Context.Response.Headers.Add("WWW-Authenticate", $"Basic realm=\"{this.Realm}\"");
        return e.Context.Response
            .SetStatus(401, message)
            .AnswerAsync();
    }
}