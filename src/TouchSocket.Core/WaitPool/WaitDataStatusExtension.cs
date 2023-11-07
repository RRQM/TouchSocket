using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Resources;

namespace TouchSocket.Core
{
    /// <summary>
    /// WaitDataStatusExtension
    /// </summary>
    public static class WaitDataStatusExtension
    {
        /// <summary>
        /// 当状态不是<see cref="WaitDataStatus.SetRunning"/>时抛出异常。
        /// </summary>
        /// <param name="status"></param>
        public static void ThrowIfNotRunning(this WaitDataStatus status)
        {
            switch (status)
            {
                case WaitDataStatus.SetRunning:
                    return;
                case WaitDataStatus.Canceled: throw new OperationCanceledException();
                case WaitDataStatus.Overtime: throw new TimeoutException();
                case WaitDataStatus.Disposed:
                case WaitDataStatus.Default:
                default:
                    {
                        throw new Exception(TouchSocketCoreResource.UnknownError.GetDescription());
                    }
            }
        }
    }
}
