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
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TouchSocket.Core
{
    /// <summary>
    /// 依赖项对象. 线程安全。
    /// </summary>
    public class DependencyObject : DisposableObject, IDependencyObject
    {
        private Dictionary<int, object> m_dp;
        private SpinLock m_lock = new SpinLock(Debugger.IsAttached);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Dictionary<int, object> GetDp()
        {
            return this.m_dp ??= new Dictionary<int, object>() ;
        }

        /// <inheritdoc/>
        public TValue GetValue<TValue>(DependencyProperty<TValue> dp)
        {
            if (this.TryGetValue(dp, out var value))
            {
                return value;
            }
            return dp.OnFailedToGetTheValue.Invoke(this);
        }

        /// <inheritdoc/>
        public bool HasValue<TValue>(DependencyProperty<TValue> dp)
        {
            var lockTaken = false;
            try
            {
                this.m_lock.Enter(ref lockTaken);
                return this.GetDp().ContainsKey(dp.Id);
            }
            finally
            {
                if (lockTaken)
                {
                    this.m_lock.Exit(false);
                }
            }
        }

        /// <inheritdoc/>
        public void RemoveValue<TValue>(DependencyProperty<TValue> dp)
        {
            var lockTaken = false;
            try
            {
                this.m_lock.Enter(ref lockTaken);
                this.GetDp().Remove(dp.Id);
            }
            finally
            {
                if (lockTaken)
                {
                    this.m_lock.Exit(false);
                }
            }
        }

        /// <inheritdoc/>
        public void SetValue<TValue>(DependencyProperty<TValue> dp, TValue value)
        {
            var lockTaken = false;
            try
            {
                this.m_lock.Enter(ref lockTaken);
                this.GetDp().AddOrUpdate(dp.Id, value);
            }
            finally
            {
                if (lockTaken)
                {
                    this.m_lock.Exit(false);
                }
            }
        }

        /// <inheritdoc/>
        public bool TryGetValue<TValue>(DependencyProperty<TValue> dp, out TValue value)
        {
            var lockTaken = false;
            try
            {
                this.m_lock.Enter(ref lockTaken);
                if (this.GetDp().TryGetValue(dp.Id, out var value1))
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
            finally
            {
                if (lockTaken)
                {
                    this.m_lock.Exit(false);
                }
            }
        }

        /// <inheritdoc/>
        public bool TryRemoveValue<TValue>(DependencyProperty<TValue> dp, out TValue value)
        {
            var lockTaken = false;
            try
            {
                this.m_lock.Enter(ref lockTaken);
                if (this.GetDp().TryGetValue(dp.Id, out var obj))
                {
                    value = (TValue)obj;
                    return this.GetDp().Remove(dp.Id);
                }
                value = default;
                return false;
            }
            finally
            {
                if (lockTaken)
                {
                    this.m_lock.Exit(false);
                }
            }
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
            var lockTakenFotThis = false;
            var lockTakenFotOther = false;
            try
            {
                this.m_lock.Enter(ref lockTakenFotThis);
                dependencyObject.m_lock.Enter(ref lockTakenFotOther);

                ThrowHelper.ThrowArgumentNullExceptionIf(dependencyObject,nameof(dependencyObject));

                ThrowHelper.ThrowObjectDisposedExceptionIf(dependencyObject);

                this.ThrowIfDisposed();

                foreach (var item in this.  GetDp())
                {
                    if (dependencyObject.GetDp().ContainsKey(item.Key))
                    {
                        if (overwrite)
                        {
                            dependencyObject.GetDp().Remove(item.Key);
                            dependencyObject.GetDp().Add(item.Key, item.Value);
                        }
                    }
                    else
                    {
                        dependencyObject.GetDp().Add(item.Key, item.Value);
                    }
                }
            }
            finally
            {
                if (lockTakenFotThis)
                {
                    this.m_lock.Exit(false);
                }

                if (lockTakenFotOther)
                {
                    dependencyObject.m_lock.Exit(false);
                }
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var lockTaken = false;
                try
                {
                    this.m_lock.Enter(ref lockTaken);
                    this.m_dp?.Clear();
                }
                finally
                {
                    if (lockTaken)
                    {
                        this.m_lock.Exit(false);
                    }
                }
            }
            base.Dispose(disposing);
        }
    }
}