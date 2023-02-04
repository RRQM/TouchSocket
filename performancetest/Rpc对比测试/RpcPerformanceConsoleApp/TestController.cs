using EventNext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Rpc.TouchRpc;
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
        [TouchRpc(true)]
        public Task<int> Sum(int a, int b)
        {
            return Task.FromResult(a + b);
        }

        [TouchRpc(true)]
        public Task<byte[]> GetBytes(int length)
        {
            return Task.FromResult(new byte[length]);
        }

        [TouchRpc(true)]
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
        [TouchRpc(true)]
        public int Sum(int a, int b) => a + b;

        [TouchRpc(true)]
        public byte[] GetBytes(int length)
        {
            return new byte[length];
        }

        [TouchRpc(true)]
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
