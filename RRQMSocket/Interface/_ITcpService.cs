using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// TCP服务器辅助接口
    /// </summary>
    public interface _ITcpService : IService
    {
        /// <summary>
        /// 重新设置ID
        /// </summary>
        /// <param name="oldID"></param>
        /// <param name="newID"></param>
        void ResetID(string oldID, string newID);
    }
}
