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
using System.Diagnostics;
using System.Threading;

namespace TouchSocket.Core
{
    /// <summary>
    /// 依赖属性
    /// </summary>
    [DebuggerDisplay("Name={Name},Id={Id}")]
    public sealed class DependencyProperty<TValue> : DependencyPropertyBase, IEquatable<DependencyProperty<TValue>>
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        private readonly string m_name;

        /// <summary>
        /// 初始化依赖属性。
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="onFailedToGetTheValue">当<see cref="IDependencyObject"/>获取属性失败时，回调该函数</param>
        /// <param name="placeholder">占位重载，无实际意义</param>
        public DependencyProperty(string propertyName, Func<IDependencyObject, TValue> onFailedToGetTheValue, bool placeholder)
        {
            this.m_name = propertyName;
            this.OnFailedToGetTheValue = ThrowHelper.ThrowArgumentNullExceptionIf(onFailedToGetTheValue, nameof(onFailedToGetTheValue));
        }

        /// <summary>
        /// 初始化依赖属性。
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="defaultValue">当<see cref="IDependencyObject"/>获取属性失败时，回调函数将直接返回默认值。</param>
        public DependencyProperty(string propertyName, TValue defaultValue) : this(propertyName, (o) => defaultValue, false)
        {
        }

        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name => this.m_name;

        /// <summary>
        /// 当<see cref="IDependencyObject"/>获取属性失败时，回调该函数
        /// </summary>
        public Func<IDependencyObject, TValue> OnFailedToGetTheValue { get; }

        /// <summary>
        /// 判断否
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(DependencyProperty<TValue> left, DependencyProperty<TValue> right)
        {
            return left.Id != right.Id;
        }

        /// <summary>
        /// 判断是
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(DependencyProperty<TValue> left, DependencyProperty<TValue> right)
        {
            return left.Id == right.Id;
        }

        /// <summary>
        /// 注册依赖项属性。
        /// <para>依赖属性的默认值，可能会应用于所有的<see cref="IDependencyObject"/></para>
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value">依赖项属性值，一般该值应该是值类型，因为它可能会被用于多个依赖对象。</param>
        /// <returns></returns>
        [Obsolete("此方法已被弃用，请使用new直接代替", true)]
        public static DependencyProperty<TValue> Register(string propertyName, TValue value)
        {
            var dp = new DependencyProperty<TValue>(propertyName, value);
            return dp;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(DependencyProperty<TValue> other)
        {
            return this.Id == other.Id;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is DependencyProperty<TValue> obj2)
            {
                if (this.Id == obj2.Id)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Id;
        }
    }
}