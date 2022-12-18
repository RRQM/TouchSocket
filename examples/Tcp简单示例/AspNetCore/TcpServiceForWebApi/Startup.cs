using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using TcpServiceForWebApi.Plugins;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TcpServiceForWebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                //此处是企业版测试授权，在本示例中，除了GetWaitingClient功能，其余的开源包都完全支持。
                Enterprise.ForTest();
            }
            catch
            {
            }
            var tcpService = services.AddTcpService(config =>
              {
                  config.SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                  .UsePlugin()
                  .UseAspNetCoreContainer(services)
                  .ConfigurePlugins(a =>
                  {
                      a.Add<MyTcpPlugin>();//此插件就可以处理接收数据
                  });
              });
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TcpServiceForWebApi", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TcpServiceForWebApi v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}