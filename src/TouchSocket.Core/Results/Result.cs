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

using System.Diagnostics;
using TouchSocket.Resources;

namespace TouchSocket.Core;

/// <summary>
/// 结果返回
/// </summary>
[DebuggerDisplay("ResultCode = {ResultCode}, Message = {Message}")]
public record struct Result
{
    /// <summary>
    /// 成功
    /// </summary>
    public static readonly Result Success = new Result(ResultCode.Success, TouchSocketCoreResource.OperationSuccessful);

    /// <summary>
    /// 初始状态
    /// </summary>
    public static readonly Result Default = new Result(ResultCode.Default, TouchSocketCoreResource.Default);

    /// <summary>
    /// 操作对象已被释放
    /// </summary>
    public static readonly Result Disposed = new Result(ResultCode.Disposed, TouchSocketCoreResource.ObjectDisposed);

    /// <summary>
    /// 未知失败
    /// </summary>
    public static readonly Result UnknownFail = new Result(ResultCode.Failure, TouchSocketCoreResource.UnknownError);

    /// <summary>
    /// 超时
    /// </summary>
    public static readonly Result Overtime = new Result(ResultCode.Overtime, TouchSocketCoreResource.OperationOvertime);

    /// <summary>
    /// 取消
    /// </summary>
    public static readonly Result Canceled = new Result(ResultCode.Canceled, TouchSocketCoreResource.OperationCanceled);

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="resultCode">结果代码，表示操作的结果</param>
    /// <param name="message">消息，提供操作结果的详细描述</param>
    public Result(ResultCode resultCode, string message)
    {
        this.ResultCode = resultCode;
        this.Message = message;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="exception">异常对象，用于提取错误信息</param>
    public Result(Exception exception)
    {
        if (typeof(OperationCanceledException) == exception.GetType())
        {
            this.ResultCode = ResultCode.Canceled;
            this.Message = exception.Message;
        }
        else
        {
            this.ResultCode = ResultCode.Exception; // 设置结果代码为异常
            this.Message = exception.Message; // 设置结果消息为异常的详细信息
        }
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="resultCode">结果代码，用于指定结果的状态</param>
    public Result(ResultCode resultCode)
    {
        this.ResultCode = resultCode; // 设置结果代码
        this.Message = resultCode.GetDescription(); // 根据结果代码设置相应的描述信息
    }

    /// <inheritdoc/>
    public ResultCode ResultCode { get; private set; }

    /// <inheritdoc/>
    public string Message { get; private set; }

    /// <inheritdoc/>
    public readonly bool IsSuccess => this.ResultCode == ResultCode.Success;

    /// <summary>
    /// 创建来自<see cref="ResultCode.Canceled"/>的<see cref="Result"/>
    /// </summary>
    /// <param name="msg">关联的消息</param>
    /// <returns>创建的Result对象</returns>
    public static Result FromCanceled(string msg)
    {
        return new Result(ResultCode.Canceled, msg);
    }

    /// <summary>
    /// 创建来自<see cref="ResultCode.Error"/>的<see cref="Result"/>
    /// </summary>
    /// <param name="msg">关联的消息</param>
    /// <returns>创建的Result对象</returns>
    public static Result FromError(string msg)
    {
        return new Result(ResultCode.Error, msg);
    }

    /// <summary>
    /// 创建来自<see cref="ResultCode.Exception"/>的<see cref="Result"/>
    /// </summary>
    /// <param name="msg">关联的消息</param>
    /// <returns>创建的Result对象</returns>
    public static Result FromException(string msg)
    {
        return new Result(ResultCode.Exception, msg);
    }

    /// <summary>
    /// 根据异常生成结果对象
    /// </summary>
    /// <param name="ex">异常对象</param>
    /// <returns>包含异常信息的结果对象</returns>
    public static Result FromException(Exception ex)
    {
        // 返回一个新的Result实例，包含异常错误代码和异常消息
        return new Result(ex);
    }

    /// <summary>
    /// 创建来自<see cref="ResultCode.Failure"/>的<see cref="Result"/>
    /// </summary>
    /// <param name="msg">关联的消息</param>
    /// <returns>创建的Result对象</returns>
    public static Result FromFail(string msg)
    {
        return new Result(ResultCode.Failure, msg);
    }

    /// <summary>
    /// 创建来自<see cref="ResultCode.Overtime"/>的<see cref="Result"/>
    /// </summary>
    /// <param name="msg">关联的消息</param>
    /// <returns>创建的Result对象</returns>
    public static Result FromOvertime(string msg)
    {
        return new Result(ResultCode.Overtime, msg);
    }

    /// <summary>
    /// 创建来自<see cref="ResultCode.Success"/>的<see cref="Result"/>
    /// </summary>
    /// <param name="msg">关联的消息</param>
    /// <returns>创建的Result对象</returns>
    public static Result FromSuccess(string msg)
    {
        return new Result(ResultCode.Success, msg);
    }

    /// <inheritdoc/>
    public override readonly string ToString()
    {
        return TouchSocketCoreResource.ResultToString.Format(this.ResultCode, this.Message);
    }
}