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

using System;
using System.Diagnostics.CodeAnalysis;

namespace TouchSocket.Core;

/// <summary>
/// 表示一个结构化的操作结果，包含操作的返回值、结果代码和消息。
/// </summary>
/// <typeparam name="T">结果值的类型。</typeparam>
public readonly struct Result<T> : IResult<T>
{
    /// <summary>
    /// 结果消息，提供操作的描述信息。
    /// </summary>
    private readonly string m_message;

    /// <summary>
    /// 结果代码，用于表示操作的状态。
    /// </summary>
    private readonly ResultCode m_resultCode;

    /// <summary>
    /// 结果值。
    /// </summary>
    private readonly T m_value;

    /// <summary>
    /// 初始化<see cref="Result{T}"/>结构。
    /// </summary>
    /// <param name="value">操作的返回值。</param>
    /// <param name="resultCode">结果代码，表示操作的状态。</param>
    /// <param name="message">结果消息，提供操作的描述信息。</param>
    public Result(T value, ResultCode resultCode, string message)
    {
        this.m_value = value;
        this.m_resultCode = resultCode;
        this.m_message = message;
    }

    /// <summary>
    /// 初始化<see cref="Result{T}"/>结构，使用默认值作为结果值。
    /// </summary>
    /// <param name="code">结果代码，表示操作的状态。</param>
    /// <param name="message">结果消息，提供操作的描述信息。</param>
    public Result(ResultCode code, string message) : this(default, code, message) { }

    /// <summary>
    /// 初始化<see cref="Result{T}"/>结构，使用结果代码的描述作为消息。
    /// </summary>
    /// <param name="code">结果代码，表示操作的状态。</param>
    public Result(ResultCode code) : this(default, code, code.GetDescription()) { }

    /// <summary>
    /// 初始化<see cref="Result{T}"/>结构，将异常信息作为结果消息。
    /// </summary>
    /// <param name="exception">发生的异常。</param>
    public Result(Exception exception) : this(default, ResultCode.Exception, exception.Message) { }

    /// <summary>
    /// 初始化<see cref="Result{T}"/>结构，使用成功作为结果代码和消息。
    /// </summary>
    /// <param name="value">操作的返回值。</param>
    public Result(T value) : this(value, ResultCode.Success, ResultCode.Success.GetDescription()) { }

    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccess => this.ResultCode == ResultCode.Success;

    /// <inheritdoc/>
    public string Message => this.m_message;

    /// <inheritdoc/>
    public ResultCode ResultCode => this.m_resultCode;

    /// <summary>
    /// 获取结果值。
    /// </summary>
    public T Value => this.m_value;

    /// <summary>
    /// 隐式类型转换操作符，将值类型<typeparamref name="T"/>转换为<see cref="Result{T}"/>类型。
    /// </summary>
    /// <param name="value">要转换的值。</param>
    /// <returns>一个新的<see cref="Result{T}"/>实例，值为提供的值，结果代码和消息为成功。</returns>
    public static implicit operator Result<T>(T value)
    {
        if (value is null)
        {
            return new Result<T>(ResultCode.Fail, "value is null.");
        }
        return new Result<T>(value);
    }

    /// <summary>
    /// 隐式转换运算符，将<see cref="Result"/>类型转换为<see cref="Result{T}"/>类型。
    /// </summary>
    /// <param name="result">原始的<see cref="Result"/>对象。</param>
    /// <returns>转换后的<see cref="Result{T}"/>对象，其中T为泛型参数。</returns>
    public static implicit operator Result<T>(Result result)
    {
        // 使用传入的Result对象的ResultCode和Message属性值创建一个新的Result<T>对象
        return new Result<T>(result.ResultCode, result.Message);
    }

    /// <summary>
    /// 隐式转换运算符，将ResultT类型的对象转换为Result类型的对象。
    /// </summary>
    /// <param name="result">要转换的ResultT对象。</param>
    /// <returns>转换后的Result对象，包含了原ResultT对象的结果码和消息。</returns>
    public static implicit operator Result(Result<T> result)
    {
        // 使用Result<T>对象的结果码和消息创建一个新的Result对象并返回
        return new Result(result.ResultCode, result.Message);
    }
}