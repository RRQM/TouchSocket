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

namespace TouchSocket.Mqtt;

/// <summary>
/// 表示 Mqtt 会话 Actor 的操作结果。
/// </summary>
public readonly struct MqttSessionActorResult
{
    /// <summary>
    /// 初始化 <see cref="MqttSessionActorResult"/> 的新实例。
    /// </summary>
    /// <param name="sessionActor">会话 Actor 实例。</param>
    /// <param name="isNew">是否是新建的会话。</param>
    public MqttSessionActorResult(MqttSessionActor sessionActor, bool isNew)
    {
        this.SessionActor = sessionActor;
        this.IsNew = isNew;
    }
    /// <summary>
    /// 获取会话 Actor 实例。
    /// </summary>
    public MqttSessionActor SessionActor { get; }
    /// <summary>
    /// 获取是否是新建的会话。
    /// </summary>
    public bool IsNew { get; }
}