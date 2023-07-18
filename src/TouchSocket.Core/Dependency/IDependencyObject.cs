using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 依赖对象接口
    /// </summary>
    public interface IDependencyObject : IDisposable
    {
        /// <summary>
        /// 获取依赖注入的值，当没有注入时，会返回默认值。
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dp"></param>
        /// <returns></returns>
        public TValue GetValue<TValue>(IDependencyProperty<TValue> dp);

        /// <summary>
        /// 是否有值。
        /// </summary>
        /// <param name="dp"></param>
        /// <returns></returns>
        public bool HasValue<TValue>(IDependencyProperty<TValue> dp);

        /// <summary>
        /// 重置属性值。
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dp"></param>
        /// <returns></returns>
        public DependencyObject RemoveValue<TValue>(IDependencyProperty<TValue> dp);

        /// <summary>
        /// 设置依赖注入的值
        /// </summary>
        /// <param name="dp"></param>
        /// <param name="value"></param>
        public DependencyObject SetValue<TValue>(IDependencyProperty<TValue> dp, TValue value);

        /// <summary>
        /// 尝试获取依赖注入的值，当没有注入时，会返回<see langword="false"/>。
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dp"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue<TValue>(IDependencyProperty<TValue> dp, out TValue value);

        /// <summary>
        /// 重置属性值。
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dp"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryRemoveValue<TValue>(IDependencyProperty<TValue> dp, out TValue value);
    }

}
