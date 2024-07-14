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

using RpcProxy;
using System;
using System.Threading.Tasks;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.XmlRpc;

namespace XmlRpcClientApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var client =await GetXmlRpcClientAsync();

            //直接调用
            var result1 = client.InvokeT<int>("Sum", InvokeOption.WaitInvoke, 10, 20);
            Console.WriteLine($"直接调用，返回结果:{result1}");

            var result2 = client.Sum(10, 20);//此Sum方法是服务端生成的代理。
            Console.WriteLine($"代理调用，返回结果:{result2}");

            Console.ReadKey();
        }

        private static async Task<XmlRpcClient> GetXmlRpcClientAsync()
        {
            var jsonRpcClient = new XmlRpcClient();
            await jsonRpcClient.ConnectAsync("http://127.0.0.1:7789/xmlRpc");
            Console.WriteLine("连接成功");
            return jsonRpcClient;
        }
    }
}