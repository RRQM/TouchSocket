//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace TouchSocket.Core
{
    /// <summary>
    /// 依赖项对象.
    /// 非线程安全。
    /// </summary>
    public class DependencyObject : DisposableObject, IDependencyObject
    {
        private readonly Dictionary<int, object> m_dp = new Dictionary<int, object>();

        /// <inheritdoc/>
        public TValue GetValue<TValue>(IDependencyProperty<TValue> dp)
        {
            return this.m_dp.TryGetValue(dp.Id, out var value) ? (TValue)value : dp.DefauleValue;
        }

        /// <inheritdoc/>
        public bool HasValue<TValue>(IDependencyProperty<TValue> dp)
        {
            return this.m_dp.ContainsKey(dp.Id);
        }

        /// <inheritdoc/>
        public DependencyObject RemoveValue<TValue>(IDependencyProperty<TValue> dp)
        {
            this.m_dp.Remove(dp.Id);
            return this;
        }

        /// <inheritdoc/>
        public DependencyObject SetValue<TValue>(IDependencyProperty<TValue> dp, TValue value)
        {
            this.m_dp.AddOrUpdate(dp.Id, value);
            return this;
        }

        /// <inheritdoc/>
        public bool TryGetValue<TValue>(IDependencyProperty<TValue> dp, out TValue value)
        {
            if (this.m_dp.TryGetValue(dp.Id, out var value1))
            {
                value = (TValue)value1;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        /// <inheritdoc/>
        public bool TryRemoveValue<TValue>(IDependencyProperty<TValue> dp, out TValue value)
        {
            if (this.m_dp.TryGetValue(dp.Id, out var obj))
            {
                value = (TValue)obj;
                this.m_dp.Remove(dp.Id);
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.m_dp.Clear();
            base.Dispose(disposing);
        }

        /// <summary>
        /// 将当前对象的依赖项克隆到目标对象中
        /// </summary>
        /// <param name="dependencyObject">目标对象</param>
        /// <param name="overwrite">当目标对象中存在相同依赖项时，是或否覆盖</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        protected void CloneTo(DependencyObject dependencyObject, bool overwrite)
        {
            if (dependencyObject is null)
            {
                throw new ArgumentNullException(nameof(dependencyObject));
            }

            if (dependencyObject.DisposedValue)
            {
                throw new ObjectDisposedException(nameof(dependencyObject));
            }

            this.ThrowIfDisposed();

            foreach (var item in this.m_dp)
            {
                if (dependencyObject.m_dp.ContainsKey(item.Key))
                {
                    if (overwrite)
                    {
                        dependencyObject.m_dp.Remove(item.Key);
                        dependencyObject.m_dp.Add(item.Key, item.Value);
                    }
                }
                else
                {
                    dependencyObject.m_dp.Add(item.Key, item.Value);
                }
            }
        }
    }
}