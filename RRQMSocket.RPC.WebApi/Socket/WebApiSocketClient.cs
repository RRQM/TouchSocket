using RRQMCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.WebApi
{
    /// <summary>
    /// WebApiSocket辅助类
    /// </summary>
    public class WebApiSocketClient : SimpleSocketClient
    {
        /// <summary>
        /// 禁用适配器赋值
        /// </summary>
        /// <param name="adapter"></param>
        public sealed override void SetDataHandlingAdapter(DataHandlingAdapter adapter)
        {
            throw new RRQMException($"{nameof(WebApiSocketClient)}不允许设置适配器。");
        }

        internal void SetAdapter(DataHandlingAdapter adapter)
        {
            base.SetDataHandlingAdapter(adapter);
        }
    }
}
