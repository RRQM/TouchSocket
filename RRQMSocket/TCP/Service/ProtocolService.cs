using RRQMCore.Exceptions;
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
        /// <summary>
        /// 重置ID
        /// </summary>
        /// <param name="oldID"></param>
        /// <param name="newID"></param>
        public override void ResetID(string oldID, string newID)
        {
            base.ResetID(oldID, newID);
            if (this.TryGetSocketClient(newID, out TClient client))
            {
                client.agreementHelper.SocketSend(0, Encoding.UTF8.GetBytes(newID));
            }
            else
            {
                throw new RRQMException("新ID不可用，请清理客户端重新修改ID");
            }
        }
    }
}
