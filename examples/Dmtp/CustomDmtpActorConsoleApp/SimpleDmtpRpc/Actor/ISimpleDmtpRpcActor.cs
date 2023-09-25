using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Dmtp;

namespace CustomDmtpActorConsoleApp.SimpleDmtpRpc
{
    interface ISimpleDmtpRpcActor : IActor
    {
        void Invoke(string methodName);
        void Invoke(string targetId, string methodName);
    }
}
