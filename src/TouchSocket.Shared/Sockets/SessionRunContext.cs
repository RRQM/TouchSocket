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

namespace TouchSocket.Sockets;

/// <summary>
/// 标记当前异步流程是否运行在某个客户端的会话任务（接收循环及 Connected、Received、Closed 等回调）中。
/// <para>
/// 关闭与重连需要等待会话任务结束；若在会话任务自身的回调中调用并等待，会等待自己而死锁。
/// 通过该标记识别这种重入，此时不再等待会话任务。
/// </para>
/// </summary>
internal static class SessionRunContext
{
    private static readonly AsyncLocal<object> s_owner = new AsyncLocal<object>();

    /// <summary>
    /// 标记当前异步流程属于 <paramref name="owner"/> 的会话任务。
    /// 必须在会话任务对应的 async 方法内部调用，标记只在该方法及其派生的异步流程内有效。
    /// </summary>
    public static void Enter(object owner)
    {
        s_owner.Value = owner;
    }

    /// <summary>
    /// 判断当前异步流程是否运行在 <paramref name="owner"/> 的会话任务中。
    /// </summary>
    public static bool IsInSession(object owner)
    {
        return ReferenceEquals(s_owner.Value, owner);
    }
}
