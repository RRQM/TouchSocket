using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 单线程状况的流式数据处理适配器的装饰器。
    /// </summary>
    public class SingleStreamDecorator
    {
        private readonly SingleStreamDataHandlingAdapter m_adapter;

        public SingleStreamDecorator(SingleStreamDataHandlingAdapter adapter)
        {
            adapter.ReceivedCallBack = this.ReceivedCallBack;
            this.m_adapter = adapter;
        }

        private void ReceivedCallBack(ByteBlock block, IRequestInfo info)
        {
         
        }
    }
}
