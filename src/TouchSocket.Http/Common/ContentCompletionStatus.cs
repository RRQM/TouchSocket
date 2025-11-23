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

namespace TouchSocket.Http;

/// <summary>
/// 内容完成状态枚举
/// </summary>
public enum ContentCompletionStatus
{
    /// <summary>
    /// 未知状态，初始状态
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 内容已完成，可以多次获取
    /// </summary>
    ContentCompleted = 1,

    /// <summary>
    /// 持续读取已完成，只能获取一次
    /// </summary>
    ReadCompleted = 2,

    /// <summary>
    /// 内容未完成，仍在进行中
    /// </summary>
    Incomplete = 3
}