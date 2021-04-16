using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.Concurrent;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 传输集合
    /// </summary>
    public class TransferCollection<T> :ConcurrentList<T>
    {

    }
}
