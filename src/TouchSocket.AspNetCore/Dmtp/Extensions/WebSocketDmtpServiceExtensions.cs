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
using TouchSocket.Core;
using TouchSocket.Dmtp.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection;


/// <summary>
/// 定义一个静态扩展类，提供与WebSocket相关的Dmtp服务扩展方法。
/// 该类的目的是为了扩展WebSocket的功能，使其能够支持Dmtp协议相关的操作。
/// </summary>
public static class WebSocketDmtpServiceExtensions
{
    /// <summary>
    /// 添加<see cref="WebSocketDmtpService"/>服务。
    /// </summary>
    /// <param name="services">要添加服务的<see cref="IServiceCollection"/>集合。</param>
    /// <param name="configAction">用于配置<see cref="TouchSocketConfig"/>的配置操作。</param>
    /// <returns>返回添加了<see cref="WebSocketDmtpService"/>服务的<see cref="IServiceCollection"/>集合。</returns>
    public static IServiceCollection AddWebSocketDmtpService(this IServiceCollection services, Action<TouchSocketConfig> configAction)
    {
        // 使用Singleton模式添加WebSocketDmtpService服务，并应用配置操作。
        return services.AddSingletonSetupConfigObject<IWebSocketDmtpService, WebSocketDmtpService>(configAction);
    }
}