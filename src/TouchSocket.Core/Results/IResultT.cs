namespace TouchSocket.Core
{
    /// <summary>
    /// 泛型接口<see cref="IResult{T}"/>，继承自<see cref="IResult"/>。
    /// 用于定义具有特定类型结果值的对象所需的行为。
    /// </summary>
    /// <typeparam name="T">结果值的类型。</typeparam>
    public interface IResult<T> : IResult
    {
        /// <summary>
        /// 获取结果值。
        /// </summary>
        /// <value>结果值，类型为 T。</value>
        T Value { get; }
    }
}
