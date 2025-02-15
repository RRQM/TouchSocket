//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Sockets;

namespace RpcFactoryConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var clientFactory = CreateTcpClientFactory();

        using (var clientFactoryResult = await clientFactory.GetClient())
        {
            //这里可以让得到的通讯单体进行业务交流
            var client = clientFactoryResult.Client;
            // client.GetDmtpRpcActor().Invoke();
        }
        ;
    }

    private static TcpDmtpClientFactory CreateTcpClientFactory()
    {
        var clientFactory = new TcpDmtpClientFactory()
        {
            MinCount = 5,//最小数量，在主连接器成功建立以后，会检测可用连接是否大于该值，否的话会自动建立。
            MaxCount = 10,//最大数量，当超过该数量的连接后，会等待指定时间，或者永久等待。
            GetConfig = () => //配置辅助通信
            {
                return new TouchSocketConfig()
                   .SetRemoteIPHost("tcp://127.0.0.1:7789")
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRpc();
                   });
            }
        };
        return clientFactory;
    }

    private static HttpDmtpClientFactory CreateHttpClientFactory()
    {
        var clientFactory = new HttpDmtpClientFactory()
        {
            MinCount = 5,//最小数量，在主连接器成功建立以后，会检测可用连接是否大于该值，否的话会自动建立。
            MaxCount = 10,//最大数量，当超过该数量的连接后，会等待指定时间，或者永久等待。
            GetConfig = () => //配置辅助通信
            {
                return new TouchSocketConfig()
                   .SetRemoteIPHost("tcp://127.0.0.1:7789");
            }
        };
        return clientFactory;
    }
}