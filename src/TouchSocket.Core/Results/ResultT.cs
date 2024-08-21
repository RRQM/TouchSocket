using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// 表示一个结构化的操作结果，包含操作的返回值、结果代码和消息。
    /// </summary>
    /// <typeparam name="T">结果值的类型。</typeparam>
    public readonly struct Result<T> : IResult<T>
    {
        /// <summary>
        /// 结果值。
        /// </summary>
        private readonly T m_value;
        
        /// <summary>
        /// 结果代码，用于表示操作的状态。
        /// </summary>
        private readonly ResultCode m_resultCode;
        
        /// <summary>
        /// 结果消息，提供操作的描述信息。
        /// </summary>
        private readonly string m_message;

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

        /// <summary>
        /// 获取结果值。
        /// </summary>
        public T Value => this.m_value;

        /// <inheritdoc/>
        public ResultCode ResultCode => m_resultCode;

        /// <inheritdoc/>
        public string Message => m_message;

        /// <inheritdoc/>
        public bool IsSuccess => this.ResultCode== ResultCode.Success;

        /// <summary>
        /// 隐式类型转换操作符，将值类型<typeparamref name="T"/>转换为<see cref="Result{T}"/>类型。
        /// </summary>
        /// <param name="value">要转换的值。</param>
        /// <returns>一个新的<see cref="Result{T}"/>实例，值为提供的值，结果代码和消息为成功。</returns>
        public static implicit operator Result<T>(T value)
        {
            return new Result<T>(value);
        }

    }
}
