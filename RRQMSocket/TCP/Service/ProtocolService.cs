using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 协议服务器
    /// </summary>
    public abstract class ProtocolService<TClient> : TokenService<TClient>where TClient:ProtocolSocketClient,new()
    {
    }
}
