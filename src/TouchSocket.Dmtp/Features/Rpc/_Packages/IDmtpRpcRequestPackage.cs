using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc
{
    public interface IDmtpRpcRequestPackage : IReadonlyRouterPackage
    {
        SerializationType SerializationType { get; }
        Metadata Metadata { get; }
        FeedbackType Feedback { get; }
        string InvokeKey { get; }
    }
}
