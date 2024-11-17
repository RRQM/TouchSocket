using TouchSocket.Core;
using TouchSocket.NamedPipe;

namespace NamedPipeWebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            #region TouchSocket相关
            builder.Services.ConfigureContainer(a =>
            {
                //在Host通用主机模式下，所有的容器注入时都是共享的，所以可以统一注入

                //使用asp的日志记录
                a.AddAspNetCoreLogger();
            });

            builder.Services.AddServiceHostedService<INamedPipeService, NamedPipeService>(config =>
            {
                config.SetPipeName("TouchSocketPipe")//设置命名管道名称
                      .ConfigurePlugins(a =>
                      {
                          //a.Add();//此处可以添加插件
                      });
            });
            #endregion


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
