namespace TouchSocket.Core
{
    /// <summary>
    /// IDependencyProperty
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IDependencyProperty<out TValue>
    {
        /// <summary>
        /// 默认值
        /// </summary>
        TValue DefauleValue { get; }

        /// <summary>
        /// 标识属性的唯一
        /// </summary>
        int Id { get; }

        /// <summary>
        /// 属性名称
        /// </summary>
        string Name { get; }
    }
}
