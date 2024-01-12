using BeetleX.FastHttpApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpPerformanceConsoleApp.Controllers
{
    [Controller]
    internal class FastApi
    {
        [Get(Route = "ApiServer/Add")]
        public int Add(int a, int b)
        {
            return a + b;
        }
    }
}
