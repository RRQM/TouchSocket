using TouchSocket.Dmtp;

namespace CustomDmtpActorConsoleApp.SimpleDmtpRpc
{
    internal interface ISimpleDmtpRpcActor : IActor
    {
        void Invoke(string methodName);

        void Invoke(string targetId, string methodName);
    }
}