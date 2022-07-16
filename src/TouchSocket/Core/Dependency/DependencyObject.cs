//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在TouchSocket.Core.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Collections.Generic;

namespace TouchSocket.Core.Dependency
{
    /// <summary>
    /// 依赖对象接口
    /// </summary>
    public interface IRRQMDependencyObject : System.IDisposable
    {
        /// <summary>
        /// 获取依赖注入的值
        /// </summary>
        /// <param name="dp"></param>
        /// <returns></returns>
        object GetValue(DependencyProperty dp);

        /// <summary>
        /// 获取依赖注入的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dp"></param>
        /// <returns></returns>
        public T GetValue<T>(DependencyProperty dp);

        /// <summary>
        /// 设置依赖注入的值
        /// </summary>
        /// <param name="dp"></param>
        /// <param name="value"></param>
        public DependencyObject SetValue(DependencyProperty dp, object value);
    }

    /// <summary>
    /// 依赖项对象.
    /// 非线程安全。
    /// </summary>
    public class DependencyObject : DisposableObject, IRRQMDependencyObject, System.IDisposable
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public DependencyObject()
        {
            this.m_dp = new Dictionary<DependencyProperty, object>();
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly Dictionary<DependencyProperty, object> m_dp;

        /// <summary>
        /// 获取依赖注入的值
        /// </summary>
        /// <param name="dp"></param>
        /// <returns></returns>
        public object GetValue(DependencyProperty dp)
        {
            if (this.m_dp.TryGetValue(dp, out object value))
            {
                return value;
            }
            else
            {
                if (dp.DependencyAction == null)
                {
                    return dp.DefauleValue;
                }
                else
                {
                    value = dp.DependencyAction.Func();
                    if (dp.DependencyAction.AlwaysNew)
                    {
                        this.SetValue(dp, value);
                    }
                    return value;
                }
            }
        }

        /// <summary>
        /// 获取依赖注入的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dp"></param>
        /// <returns></returns>
        public T GetValue<T>(DependencyProperty dp)
        {
            try
            {
                return (T)this.GetValue(dp);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// 设置依赖注入的值
        /// </summary>
        /// <param name="dp"></param>
        /// <param name="value"></param>
        public DependencyObject SetValue(DependencyProperty dp, object value)
        {
            dp.DataValidation(value);
            if (this.m_dp.ContainsKey(dp))
            {
                this.m_dp[dp] = value;
            }
            else
            {
                this.m_dp.Add(dp, value);
            }
            return this;
        }

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!this.m_disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }
                this.m_dp.Clear();
                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                this.m_disposedValue = true;
            }
        }
    }
}