using RRQMCore.Collections.Concurrent;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 协议订阅集合
    /// </summary>
    public class ProtocolSubscriberCollection : ConcurrentList<ProtocolSubscriber>
    {
    }
}
