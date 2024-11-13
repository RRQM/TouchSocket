using TouchSocket.Rpc;
using TouchSocket.WebApi;

namespace HttpPerformanceConsoleApp.Controllers
{
    public partial class ApiServer : RpcServer
    {
        [WebApi(Method = HttpMethodType.Get)]
        public int Add(int a, int b)
        {
            return a + b;
        }

    }
}
