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

namespace TouchSocket.Rpc;

/// <summary>
/// 调用结果
/// </summary>
public sealed class InvokeResult
{
    /// <summary>
    /// 初始化 <see cref="InvokeResult"/> 结构的新实例。
    /// </summary>
    /// <param name="status">调用状态。</param>
    public InvokeResult(InvokeStatus status)
    {
        this.Status = status;
        this.Message = status.ToString();
    }

    /// <summary>
    /// 初始化 <see cref="InvokeResult"/> 结构的新实例。
    /// </summary>
    /// <param name="ex">异常实例。</param>
    public InvokeResult(Exception ex)
    {
        this.Exception = ex;
        this.Status = InvokeStatus.Exception;
        this.Message = ex.Message;
    }

    /// <summary>
    /// 初始化 <see cref="InvokeResult"/> 结构的新实例。
    /// </summary>
    public InvokeResult()
    {
    }

    /// <summary>
    /// 异常
    /// </summary>
    public Exception Exception { get; set; }

    /// <summary>
    /// 信息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 执行返回值结果
    /// </summary>
    public object Result { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public InvokeStatus Status { get; set; }
}