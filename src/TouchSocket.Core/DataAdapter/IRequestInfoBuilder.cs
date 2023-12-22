using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 指示<see cref="IRequestInfo"/>应当如何构建
    /// </summary>
    public interface IRequestInfoBuilder:IRequestInfo
    {
        /// <summary>
        /// 构建数据时，指示内存池的申请长度。
        /// </summary>
        int MaxLength { get;}

        /// <summary>
        /// 构建对象到<see cref="ByteBlock"/>
        /// </summary>
        /// <param name="byteBlock"></param>
        void Build(ByteBlock byteBlock);
    }
}
