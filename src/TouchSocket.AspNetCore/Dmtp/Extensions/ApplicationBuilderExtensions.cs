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

using TouchSocket.Dmtp.AspNetCore;

namespace Microsoft.AspNetCore.Builder
{

    /// <summary>
    /// 应用程序构建器扩展方法的静态部分。
    /// 这些扩展方法旨在简化应用程序构建过程，通过提供一些快捷方式来增强应用构建器的功能。
    /// </summary>
    public static partial class ApplicationBuilderExtensions
    {
        /// <summary>
        /// 使用基于WebSocket的Dmtp服务器。
        /// <para>启用该功能时，请先启用WebSocket。</para>
        /// </summary>
        /// <param name="builder">应用程序构建器实例。</param>
        /// <param name="url">WebSocket的路径。</param>
        /// <returns>返回应用程序构建器实例。</returns>
        public static IApplicationBuilder UseWebSocketDmtp(this IApplicationBuilder builder, string url = "/websocketdmtp")
        {
            return builder.UseMiddleware<WebSocketDmtpMiddleware>(url);
        }
    }
}