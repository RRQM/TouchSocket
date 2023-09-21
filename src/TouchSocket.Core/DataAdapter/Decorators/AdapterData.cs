using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 适配器数据
    /// </summary>
    public ref struct AdapterData
    {
        /// <summary>
        /// 适配器数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        public AdapterData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.ByteBlock = byteBlock;
            this.RequestInfo = requestInfo;
        }

        /// <summary>
        /// ByteBlock
        /// </summary>
        public ByteBlock ByteBlock { get;private set; }

        /// <summary>
        /// RequestInfo
        /// </summary>
        public IRequestInfo RequestInfo { get; private set; }
        public void Dispose()
        { 
        
        }
    }
}
