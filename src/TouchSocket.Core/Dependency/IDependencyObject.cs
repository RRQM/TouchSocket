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

namespace TouchSocket.Core
{
    /// <summary>
    /// 依赖对象接口
    /// </summary>
    public interface IDependencyObject : IDisposableObject
    {
        /// <summary>
        /// 获取依赖注入的值，当没有注入时，会返回<see cref="DependencyProperty{TValue}.DefauleValue"/>值。
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dp"></param>
        /// <returns></returns>
        TValue GetValue<TValue>(DependencyProperty<TValue> dp);

        /// <summary>
        /// 判断在当前对象中是否有已设置的属性值。
        /// </summary>
        /// <param name="dp"></param>
        /// <returns></returns>
        bool HasValue<TValue>(DependencyProperty<TValue> dp);

        /// <summary>
        /// 移除属性值。
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dp"></param>
        /// <returns></returns>
        void RemoveValue<TValue>(DependencyProperty<TValue> dp);

        /// <summary>
        /// 设置依赖注入的值，如果值已经存在，将被覆盖。
        /// </summary>
        /// <param name="dp"></param>
        /// <param name="value"></param>
        void SetValue<TValue>(DependencyProperty<TValue> dp, TValue value);

        /// <summary>
        /// 尝试获取依赖注入的值，当没有注入时，会返回<see langword="false"/>。
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dp"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetValue<TValue>(DependencyProperty<TValue> dp, out TValue value);

        /// <summary>
        /// 尝试重置属性值，如果没有这个值，则返回<see langword="false"/>。
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dp"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryRemoveValue<TValue>(DependencyProperty<TValue> dp, out TValue value);
    }
}