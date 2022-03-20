//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
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
using System.Collections.Concurrent;

namespace RRQMCore.Dependency
{
    /// <summary>
    /// 依赖对象接口
    /// </summary>
    public interface IRRQMDependencyObject : System.IDisposable
    {
        /// <summary>
        /// 获取依赖注入的值
        /// </summary>
        /// <param name="dependencyProperty"></param>
        /// <returns></returns>
        object GetValue(DependencyProperty dependencyProperty);

        /// <summary>
        /// 获取依赖注入的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependencyProperty"></param>
        /// <returns></returns>
        public T GetValue<T>(DependencyProperty dependencyProperty);

        /// <summary>
        /// 设置依赖注入的值
        /// </summary>
        /// <param name="dependencyProperty"></param>
        /// <param name="value"></param>
        public RRQMDependencyObject SetValue(DependencyProperty dependencyProperty, object value);
    }

    /// <summary>
    /// 依赖项对象
    /// </summary>
    public class RRQMDependencyObject : IRRQMDependencyObject, System.IDisposable
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RRQMDependencyObject()
        {
            this.dp = new ConcurrentDictionary<DependencyProperty, object>();
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private ConcurrentDictionary<DependencyProperty, object> dp;

        /// <summary>
        /// 判断是否已释放。
        /// </summary>
        protected volatile bool disposedValue;

        /// <summary>
        /// 获取依赖注入的值
        /// </summary>
        /// <param name="dependencyProperty"></param>
        /// <returns></returns>
        public object GetValue(DependencyProperty dependencyProperty)
        {
            if (this.dp.TryGetValue(dependencyProperty, out object value))
            {
                return value;
            }
            else
            {
                return dependencyProperty.DefauleValue;
            }
        }

        /// <summary>
        /// 获取依赖注入的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependencyProperty"></param>
        /// <returns></returns>
        public T GetValue<T>(DependencyProperty dependencyProperty)
        {
            try
            {
                return (T)this.GetValue(dependencyProperty);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// 设置依赖注入的值
        /// </summary>
        /// <param name="dependencyProperty"></param>
        /// <param name="value"></param>
        public RRQMDependencyObject SetValue(DependencyProperty dependencyProperty, object value)
        {
            dependencyProperty.DataValidation(value);
            this.dp.AddOrUpdate(dependencyProperty, value, (DependencyProperty dp, object v) =>
            {
                return value;
            });
            return this;
        }

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }
                this.dp.Clear();
                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                this.disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~RRQMDependencyObject()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            this.Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }
}