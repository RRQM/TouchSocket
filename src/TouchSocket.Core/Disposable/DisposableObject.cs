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
using System.Runtime.CompilerServices;

namespace TouchSocket.Core
{
    /// <summary>
    /// 具有释放的对象。内部实现了<see cref="GC.SuppressFinalize(object)"/>，但不包括析构函数相关。
    /// </summary>
    public abstract partial class DisposableObject : IDisposableObject
    {
        /// <summary>
        /// 判断是否已释放。
        /// </summary>
        private volatile bool m_disposedValue;

        /// <inheritdoc/>
        public bool DisposedValue => this.m_disposedValue;

        /// <summary>
        /// 判断当前对象是否已经被释放。
        /// 如果已经被释放，则抛出ObjectDisposedException异常。
        /// </summary>
        /// <exception cref="ObjectDisposedException">当对象已经被释放时抛出此异常</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowIfDisposed()
        {
            // 检查对象是否已经被释放
            if (this.m_disposedValue)
            {
                // 如果对象已被释放，抛出ObjectDisposedException异常
                ThrowHelper.ThrowObjectDisposedException(this);
            }
        }


        /// <summary>
        /// 处置资源
        /// </summary>
        /// <param name="disposing">一个值，表示是否释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            // 标记当前对象为已处置状态
            this.m_disposedValue = true;
        }

        /// <summary>
        /// 释放资源。内部已经处理了<see cref="GC.SuppressFinalize(object)"/>
        /// </summary>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}