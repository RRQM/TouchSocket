using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TcpServiceForWebApi
{
    public class TcpResult : ResultBase
    {
        public TcpResult(ResultCode resultCode, string message) : base(resultCode, message)
        {
        }
    }
}
