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

namespace TouchSocket.NamedPipe;

/// <summary>
/// 提供扩展方法以创建等待客户端实例。
/// </summary>
public static class WaitingClientExtension
{
    /// <summary>
    /// 为 INamedPipeClient 类型的客户端创建一个等待客户端实例。
    /// </summary>
    /// <param name="client">要为其创建等待客户端的 INamedPipeClient 实例。</param>
    /// <param name="waitingOptions">等待选项，用于配置等待行为。</param>
    /// <returns>返回一个配置好的等待客户端实例。</returns>
    public static IWaitingClient<INamedPipeClient, IReceiverResult> CreateWaitingClient(this INamedPipeClient client, WaitingOptions waitingOptions)
    {
        return client.CreateWaitingClient<INamedPipeClient, IReceiverResult>(waitingOptions);
    }

    /// <summary>
    /// 为 INamedPipeSessionClient 类型的客户端创建一个等待客户端实例。
    /// </summary>
    /// <param name="client">要为其创建等待客户端的 INamedPipeSessionClient 实例。</param>
    /// <param name="waitingOptions">等待选项，用于配置等待行为。</param>
    /// <returns>返回一个配置好的等待客户端实例。</returns>
    public static IWaitingClient<INamedPipeSessionClient, IReceiverResult> CreateWaitingClient(this INamedPipeSessionClient client, WaitingOptions waitingOptions)
    {
        return client.CreateWaitingClient<INamedPipeSessionClient, IReceiverResult>(waitingOptions);
    }
}