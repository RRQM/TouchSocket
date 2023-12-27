namespace WorkerService_NET8
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();
            #region Ìí¼ÓTcp·þÎñÆ÷
            builder.Services.AddTcpService(config =>
            {
                config.SetListenIPHosts(7789);
            });
            #endregion
            var host = builder.Build();
            host.Run();
        }
    }
}