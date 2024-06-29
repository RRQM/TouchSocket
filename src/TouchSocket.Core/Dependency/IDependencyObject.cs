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

namespace TouchSocket.Core
{
    /// <summary>
    /// 依赖对象接口
    /// </summary>
    public interface IDependencyObject : IDisposableObject
    {
        /// <summary>
        /// 获取依赖注入的值，当没有注入时，会返回默认值。
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dp"></param>
        /// <returns></returns>
        TValue GetValue<TValue>(IDependencyProperty<TValue> dp);

        /// <summary>
        /// 是否有值。
        /// </summary>
        /// <param name="dp"></param>
        /// <returns></returns>
        bool HasValue<TValue>(IDependencyProperty<TValue> dp);

        /// <summary>
        /// 重置属性值。
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dp"></param>
        /// <returns></returns>
        DependencyObject RemoveValue<TValue>(IDependencyProperty<TValue> dp);

        /// <summary>
        /// 设置依赖注入的值
        /// </summary>
        /// <param name="dp"></param>
        /// <param name="value"></param>
        DependencyObject SetValue<TValue>(IDependencyProperty<TValue> dp, TValue value);

        /// <summary>
        /// 尝试获取依赖注入的值，当没有注入时，会返回<see langword="false"/>。
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dp"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetValue<TValue>(IDependencyProperty<TValue> dp, out TValue value);

        /// <summary>
        /// 重置属性值。
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dp"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryRemoveValue<TValue>(IDependencyProperty<TValue> dp, out TValue value);
    }
}