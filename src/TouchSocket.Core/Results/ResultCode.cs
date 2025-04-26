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

namespace TouchSocket.Core;

/// <summary>
/// 结果类型
/// </summary>
public enum ResultCode : byte
{
    /// <summary>
    /// 默认，表示没有特定的结果状态
    /// </summary>
    Default = 0,

    /// <summary>
    /// 成功
    /// </summary>
    Success,

    /// <summary>
    /// 错误，程度较重的错误，但不影响系统的运行
    /// </summary>
    Error,

    /// <summary>
    /// 异常，程度较重的错误，可能是由于系统异常或其他不可恢复的原因导致的
    /// </summary>
    Exception,

    /// <summary>
    /// 失败，程度较轻的错误，可能是由于参数错误或其他可恢复的原因导致的
    /// </summary>
    Failure,

    /// <summary>
    /// 操作超时
    /// </summary>
    Overtime,

    /// <summary>
    /// 操作取消
    /// </summary>
    Canceled,

    /// <summary>
    /// 操作对象已被释放
    /// </summary>
    Disposed,

}