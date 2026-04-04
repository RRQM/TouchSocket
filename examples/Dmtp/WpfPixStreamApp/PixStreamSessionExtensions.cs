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

using TouchSocket.Dmtp.PixStream;
using WpfApp1.Services;

namespace WpfApp1;

/// <summary>
/// 为 <see cref="IPixStreamSession"/> 提供扩展方法。
/// </summary>
public static class PixStreamSessionExtensions
{
    /// <summary>
    /// 将 <see cref="IPixStreamSession"/> 的帧接收回调绑定到指定的 <see cref="PixStreamDisplay"/>。
    /// </summary>
    /// <param name="session">要绑定的 <see cref="IPixStreamSession"/>。</param>
    /// <param name="display">用于渲染帧的 <see cref="PixStreamDisplay"/>。</param>
    public static void Bind(this IPixStreamSession session, PixStreamDisplay display)
    {
        session.OnReceived = display.OnFrameReceivedAsync;
    }
}
