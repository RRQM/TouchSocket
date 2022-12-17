using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 响应数据。
    /// </summary>
    public struct ResponsedData
    {
        private readonly byte[] m_data;
        private readonly IRequestInfo m_requestInfo;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="data"></param>
        /// <param name="requestInfo"></param>
        public ResponsedData(byte[] data, IRequestInfo requestInfo)
        {
            m_data = data;
            m_requestInfo = requestInfo;
        }

        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data => m_data;

        /// <summary>
        /// RequestInfo
        /// </summary>
        public IRequestInfo RequestInfo => m_requestInfo;
    }
}
