// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using TouchSocket.Core;
using TouchSocket.NamedPipe;

namespace NamedPipeWebApplication;

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
