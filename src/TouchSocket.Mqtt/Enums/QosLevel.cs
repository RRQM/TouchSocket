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
/// 表示Mqtt消息的服务质量（QoS）级别。
/// </summary>
public enum QosLevel : byte
{
    /// <summary>
    /// 最多一次。消息发布完全依赖底层网络的能力。消息可能到达一次也可能根本没到达。
    /// </summary>
    AtMostOnce = 0,

    /// <summary>
    /// 至少一次。确保消息至少到达一次，但消息可能会重复。
    /// </summary>
    AtLeastOnce = 1,

    /// <summary>
    /// 只有一次。确保消息到达一次且仅到达一次。
    /// </summary>
    ExactlyOnce = 2
}