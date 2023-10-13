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