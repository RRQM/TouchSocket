using System.Text;
using TouchSocket.Core.Config;
using TouchSocket.Sockets;

namespace PipelineConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TcpService service = new TcpService();

            service.Received = (client, byteBlock, requestInfo) =>
            {
                if (requestInfo is Pipeline pipeline)//实际上Pipeline继承自Stream
                {
                    //pipeline.ReadTimeout = 1000 * 60;//设置读取超时时间为60秒。
                    //StreamReader streamReader = new StreamReader(pipeline);//所以可以直接用StreamReader构造
                    //string? ss = streamReader.ReadLine();//会一直等换行，直到等到换行，才继续向下执行
                    //Console.WriteLine(ss);

                    while (true)
                    {
                        byte[] buffer = new byte[1024];
                        int r = pipeline.Read(buffer);
                        var str = Encoding.UTF8.GetString(buffer, 0, r);
                        if (str.Contains("E"))
                        {
                            break;
                        }
                        pipeline.Write(Encoding.UTF8.GetBytes(str));
                        Console.WriteLine(str);
                    }
                }
                //当Pipeline退出该事件方法时，会被自动释放，下次会投递新的Pipeline实例。
                // 如果里面还有未Read完的数据，下次会继续投递,如果想直接丢弃，则在此处直接调用Disopose即可。

            };

            //声明配置
            var config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
                .SetDataHandlingAdapter(() => new PipelineDataHandlingAdapter());//配置适配器为Pipeline

            //载入配置
            service.Setup(config);

            //启动
            service.Start();

            Console.ReadKey();
        }
    }
}