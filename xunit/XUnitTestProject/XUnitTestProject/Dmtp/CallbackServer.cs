using TouchSocket.Dmtp.Rpc;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;

namespace XUnitTestProject.Dmtp
{
    public class CallbackServer : RpcServer
    {
        public int count;

        [DmtpRpc(true)]
        [JsonRpc(true)]
        public int Add(int a, int b)
        {
            Interlocked.Increment(ref this.count);
            return a + b;
        }

        [DmtpRpc(true)]
        public int Ref(ref int a, out int b)
        {
            b = a;
            a++;
            return a + b;
        }

        [DmtpRpc(true)]
        [JsonRpc(true)]
        public string SayHello(int age)
        {
            return $"我今年{age}岁了";
        }
    }
}