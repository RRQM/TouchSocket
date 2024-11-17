using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 定义了一个支持NAT（网络地址转换）服务的接口，用于扩展ITcpServiceBase服务以支持NAT穿透功能。
    /// </summary>
    /// <typeparam name="TClient">客户端类型的参数，必须实现INatSessionClient接口。</typeparam>
    public interface INatService<TClient> : ITcpServiceBase<TClient> where TClient : INatSessionClient
    {

    }
}
