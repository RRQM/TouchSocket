using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// SystemThreadingExtension
    /// </summary>
    public static class SystemThreadingExtension
    {
        /// <summary>
        /// 创建一个可释放的读取锁
        /// </summary>
        /// <param name="lockSlim"></param>
        /// <returns></returns>
        public static ReadLock CreateReadLock(this ReaderWriterLockSlim lockSlim)
        {
            return new ReadLock(lockSlim);
        }

        /// <summary>
        /// 创建一个可释放的写入锁
        /// </summary>
        /// <param name="lockSlim"></param>
        /// <returns></returns>
        public static WriteLock CreateWriteLock(this ReaderWriterLockSlim lockSlim)
        {
            return new WriteLock(lockSlim);
        }
    }
}
