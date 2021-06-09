//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMSocket;
using System;

namespace Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.ReadKey();
            TimeSpan timeSpan = RRQMCore.Diagnostics.TimeMeasurer.Run(() =>
              {
                  for (int i = 0; i < 100000000; i++)
                  {
                      Test test = new Test();
                  }
              });
            Console.WriteLine(timeSpan);
            Console.ReadKey();
            //TokenService<MySocketClient> tokenService = new TokenService<MySocketClient>();

            //var config = new TcpServerConfig();
            //config.SetValue(ServerConfig.BindIPHostProperty, new IPHost(7789))
            //    .SetValue(TcpServerConfig.MaxCountProperty, 10000)
            //    .SetValue(TokenServerConfig.VerifyTokenProperty, "123TT");

            //var config1 = new TcpServerConfig();
            //config1.BindIPHost = new IPHost(7789);
            //config1.MaxCount = 10000;
            //config1.ClearInterval = 60;

            //tokenService.Setup(config);
            //tokenService.Start();
        }
    }

    internal class MySocketClient : SocketClient
    {
        protected override void HandleReceivedData(ByteBlock byteBlock, object obj)
        {
        }
    }

    internal struct Test
    {
        public int offset;
        public int length;
        public ByteBlock byteBlock;
    }
}