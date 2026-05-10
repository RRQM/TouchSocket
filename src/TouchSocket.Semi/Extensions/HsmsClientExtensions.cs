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

namespace TouchSocket.Semi;

/// <summary>
/// 提供 <see cref="IHsmsClient"/> 相关扩展方法。
/// </summary>
public static class HsmsClientExtensions
{
    /// <summary>
    /// 发送一条 HSMS Linktest.req 并等待 Linktest.rsp 响应。
    /// </summary>
    /// <param name="client">HSMS 客户端。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    public static Task<HsmsMessage?> SendLinkTestAsync(this IHsmsClient client, CancellationToken cancellationToken = default)
    {
        return client.SendHsmsMessageAsync(HsmsMessage.CreateLinkTestRequest(), cancellationToken);
    }

    /// <summary>
    /// 发送一条 Separate.req 消息，通知对端断开连接。
    /// </summary>
    /// <param name="client">HSMS 客户端。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    public static Task SendSeparateAsync(this IHsmsClient client, CancellationToken cancellationToken = default)
    {
        return client.SendHsmsMessageAsync(HsmsMessage.CreateSeparateRequest(), cancellationToken);
    }
}
