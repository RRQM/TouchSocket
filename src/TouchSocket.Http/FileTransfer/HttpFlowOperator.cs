using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    /// <summary>
    /// 表示一个管理数据流的HTTP流操作器。
    /// </summary>
    public class HttpFlowOperator : FlowOperator
    {
        internal Result SetResult(Result result)
        {
            this.Result = result;
            return result;
        }

        internal Task AddFlowAsync(int flow)
        {
            return this.ProtectedAddFlowAsync(flow);
        }

        internal void AddCompletedLength(long flow)
        {
            this.completedLength += flow;
        }

        internal void SetLength(long len)
        {
            this.Length = len;
        }
    }
}
