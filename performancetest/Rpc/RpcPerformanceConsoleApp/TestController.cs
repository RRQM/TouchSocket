using System.Text;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;

namespace RpcPerformanceConsoleApp
{
    public interface ITestTaskController
    {
        Task<int> Sum(int a, int b);

        Task<byte[]> GetBytes(int length);

        Task<string> GetBigString();
    }

    public class TestTaskController : ITestTaskController
    {
        [DmtpRpc(true)]
        public Task<int> Sum(int a, int b)
        {
            return Task.FromResult(a + b);
        }

        [DmtpRpc(true)]
        public Task<byte[]> GetBytes(int length)
        {
            return Task.FromResult(new byte[length]);
        }

        [DmtpRpc(true)]
        public Task<string> GetBigString()
        {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < 10; i++)
            {
                stringBuilder.Append("RRQM");
            }
            return Task.FromResult(stringBuilder.ToString());
        }
    }


    public partial class TestController : RpcServer
    {
        [DmtpRpc(true)]
        public int Sum(int a, int b) => a + b;

        [DmtpRpc(true)]
        public byte[] GetBytes(int length)
        {
            return new byte[length];
        }

        [DmtpRpc(true)]
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
