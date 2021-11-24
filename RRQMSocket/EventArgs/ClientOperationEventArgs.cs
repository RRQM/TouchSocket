using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// Client消息操作事件
    /// </summary>
    public class ClientOperationEventArgs : OperationEventArgs
    {
        /// <summary>
        /// 客户端ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        public DataHandlingAdapter DataHandlingAdapter { get; set; }
    }
}
