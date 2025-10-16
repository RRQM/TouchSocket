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

using System.Net;

namespace TouchSocket.Sockets;

/// <summary>
/// <see cref="TouchSocketConfig"/>的Http扩展配置。
/// </summary>
public static class HttpConfigExtensions
{
    /// <summary>
    /// 代理属性。
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<IWebProxy> ProxyProperty = new DependencyProperty<IWebProxy>("Proxy", default);

    /// <summary>
    /// 设置代理。
    /// </summary>
    /// <param name="config">配置对象</param>
    /// <param name="proxyUri">代理Uri</param>
    /// <returns>配置对象</returns>
    public static TouchSocketConfig SetProxy(this TouchSocketConfig config, Uri proxyUri)
    {
        config.SetProxy(new WebProxy(proxyUri));
        return config;
    }

    /// <summary>
    /// 设置代理。
    /// </summary>
    /// <param name="config">配置对象</param>
    /// <param name="host">主机</param>
    /// <param name="port">端口</param>
    /// <returns>配置对象</returns>
    public static TouchSocketConfig SetProxy(this TouchSocketConfig config, string host, int port)
    {
        config.SetProxy(new WebProxy(host, port));
        return config;
    }

    /// <summary>
    /// 设置带凭证的代理。
    /// </summary>
    /// <param name="config">配置对象</param>
    /// <param name="proxyUri">代理Uri</param>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <returns>配置对象</returns>
    public static TouchSocketConfig SetProxy(this TouchSocketConfig config, Uri proxyUri, string username, string password)
    {
        config.SetProxy(new WebProxy(proxyUri)
        {
            Credentials = new NetworkCredential(username, password)
        });
        return config;
    }

    /// <summary>
    /// 设置带凭证的代理。
    /// </summary>
    /// <param name="config">配置对象</param>
    /// <param name="host">主机</param>
    /// <param name="port">端口</param>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <returns>配置对象</returns>
    public static TouchSocketConfig SetProxy(this TouchSocketConfig config, string host, int port, string username, string password)
    {
        config.SetProxy(new WebProxy(host, port)
        {
            Credentials = new NetworkCredential(username, password)
        });
        return config;
    }

    /// <summary>
    /// 设置系统代理。
    /// </summary>
    /// <param name="config">配置对象</param>
    /// <returns>配置对象</returns>
    public static TouchSocketConfig SetSystemProxy(this TouchSocketConfig config)
    {
        config.SetProxy(WebRequest.GetSystemWebProxy());
        return config;
    }
}