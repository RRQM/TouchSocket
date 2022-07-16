//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using TouchSocket.Core.AspNetCore;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Rpc;

namespace TouchRpcWebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public static RpcStore RpcStore { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //声明一个适应于Asp，兼容IServiceCollection服务的容器
            IContainer container = new AspNetCoreContainer(services);

            //向Asp服务中添加IWSTouchRpcService
            var touchRpcService = services.AddWSTouchRpc(new TouchSocketConfig()
                .SetContainer(container));//设置IOC容器

            RpcStore = new RpcStore(container);//和RpcStore共享容器
            RpcStore.AddRpcParser(nameof(touchRpcService), touchRpcService);//添加解析器
            RpcStore.RegisterAllServer();//注册所有服务。

            services.AddControllers();

            // ���Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Demo", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Demo v1");
            });

            app.UseWebSockets();//必须先使用WebSocket
            app.UseWSTouchRpc("/wstouchrpc");//该操作不会影响原有的WebSocket，只要url不同即可。

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}