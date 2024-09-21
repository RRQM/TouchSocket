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

            #region TouchSocket���
            builder.Services.ConfigureContainer(a =>
            {
                //��Hostͨ������ģʽ�£����е�����ע��ʱ���ǹ���ģ����Կ���ͳһע��

                //ʹ��asp����־��¼
                a.AddAspNetCoreLogger();
            });

            builder.Services.AddServiceHostedService<INamedPipeService, NamedPipeService>(config =>
            {
                config.SetPipeName("TouchSocketPipe")//���������ܵ�����
                      .ConfigurePlugins(a =>
                      {
                          //a.Add();//�˴�������Ӳ��
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
