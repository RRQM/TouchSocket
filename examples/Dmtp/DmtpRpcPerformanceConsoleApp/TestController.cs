using System.Text;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;

namespace RpcPerformanceConsoleApp
{
    public partial class TestController : SingletonRpcServer
    {
      
        [DmtpRpc(MethodInvoke = true)]
        public int Sum(int a, int b) => a + b;

        [DmtpRpc(MethodInvoke = true)]
        public byte[] GetBytes(int length)
        {
            return new byte[length];
        }

        [DmtpRpc(MethodInvoke = true)]
        public string GetBigString()
        {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < 10; i++)
            {
                stringBuilder.Append("RRQM");
            }
            return stringBuilder.ToString();
        }
    }
}
