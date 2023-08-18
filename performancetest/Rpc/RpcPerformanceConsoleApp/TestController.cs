using EventNext;
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

    /// <summary>
    /// BeetleXRPC仅支持Task返回。
    /// NewLifeRPC仅支持常规参数返回。
    /// 只有RRQM全兼容。哎！！还得写两个服务类。
    /// </summary>
    [Service(typeof(ITestTaskController))]
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
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < 10; i++)
            {
                stringBuilder.Append("RRQM");
            }
            return Task.FromResult(stringBuilder.ToString());
        }
    }


    public class TestController : RpcServer
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
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < 10; i++)
            {
                stringBuilder.Append("RRQM");
            }
            return stringBuilder.ToString();
        }
    }
}
