using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    public interface IWaitDataAsync<T> : IWaitDataBase<T>
    {
        Task<WaitDataStatus> WaitAsync(TimeSpan timeSpan);
        Task<WaitDataStatus> WaitAsync(int millisecond);
    }
}
