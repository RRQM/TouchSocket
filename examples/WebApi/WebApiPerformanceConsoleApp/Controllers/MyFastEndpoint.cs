using FastEndpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpPerformanceConsoleApp.Controllers
{
    public class MyFastEndpoint : Endpoint<MyRequest, MyResponse>
    {
        public override void Configure()
        {
            Get("/ApiServer/Add");
            AllowAnonymous();
        }

        public override async Task HandleAsync(MyRequest req, CancellationToken ct)
        {
            await this.Send.OkAsync(new MyResponse
            {
                Result = req.A + req.B
            }, ct);
        }
    }

    public class MyRequest
    {
        public int A { get; set; }
        public int B { get; set; }
    }

    public class MyResponse
    {
        public int Result { get; set; }
    }


}
