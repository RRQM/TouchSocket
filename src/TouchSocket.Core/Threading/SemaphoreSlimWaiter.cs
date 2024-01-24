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

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace TouchSocket.Core
//{
//    /// <summary>
//    /// SemaphoreSlimWaiter
//    /// </summary>
//    public struct SemaphoreSlimWaiter:IDisposable
//    {
//        private readonly SemaphoreSlim m_semaphoreSlim;

//        /// <summary>
//        /// SemaphoreSlimWaiter
//        /// </summary>
//        /// <param name="semaphoreSlim"></param>
//        public SemaphoreSlimWaiter(SemaphoreSlim semaphoreSlim)
//        {
//            this.m_semaphoreSlim = semaphoreSlim;
//        }

//        public bool Wait(TimeSpan timeout)
//        {
//        }
//        public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
//        {
//        }
//        public bool Wait(int millisecondsTimeout)
//        {
//        }
//        public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
//        {
//        }
//        public void Wait()
//        {
//        }
//       public void Wait(CancellationToken cancellationToken); public void Dispose()
//       public Task WaitAsync(); {
//       public Task<bool> WaitAsync(int millisecondsTimeout);
//       public Task<bool> WaitAsync(TimeSpan timeout); }
//    }  public Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken);
//}      public Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken);
//       public Task WaitAsync(CancellationToken cancellationToken);