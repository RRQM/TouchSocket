//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Rpc;
using TouchSocket.Rpc.TouchRpc;

namespace RRQMService.RPC.Server
{
    public class TestRpcServer : RpcServer
    {
        [Description("测试同步调用")]
        [TouchRpc]
        public string TestOne(int id)//同步服务
        {
            return $"若汝棋茗,id={id}";
        }

        [Description("测试TestTwo")]
        [TouchRpc]
        public string TestTwo(int id)//同步服务
        {
            return $"若汝棋茗,id={id}";
        }

        [Description("测试Out")]
        [TouchRpc]
        public void TestOut(out int id)
        {
            id = 10;
        }

        [Description("测试Ref")]
        [TouchRpc]
        public void TestRef(ref int id)
        {
            id += 1;
        }

        [Description("测试异步")]
        [TouchRpc]
        public Task<string> AsyncTestOne(int id)//异步服务,尽量不要用Async结尾，不然生成的异步代码方法将出现两个Async
        {
            return Task.Run(() =>
            {
                return $"若汝棋茗,id={id}";
            });
        }

        [TouchRpc]
        public void InvokeAll()
        {
            if (this.RpcStore.TryGetRpcParser("tcpRpcParser", out IRpcParser rpcParser))//通过添加解析器时的键，找到对应的解析器
            {
                if (rpcParser is TcpTouchRpcService tcpRpcParser)//转换为TcpRpcParser，或对应解析器类型
                {
                    TcpTouchRpcSocketClient[] clients = tcpRpcParser.GetClients();//获取到所以客户端。
                    foreach (TcpTouchRpcSocketClient client in clients)
                    {
                        client.Invoke("Callback", null);
                    }
                }
            }
        }
    }

    public class PerformanceRpcServer : RpcServer
    {
        [Description("测试性能")]
        [TouchRpc]
        public string Performance()//同步服务
        {
            return "若汝棋茗";
        }

        [Description("测试并发性能")]
        [TouchRpc(true)]
        public int ConPerformance(int num)
        {
            return ++num;
        }

        [Description("测试并发性能2")]
        [TouchRpc]
        public int ConPerformance2(int num)
        {
            return ++num;
        }
    }

    public class ElapsedTimeRpcServer : RpcServer
    {
        [Description("测试可取消的调用")]
        [TouchRpc]
        public bool DelayInvoke(ICallContext serverCallContext, int tick)//同步服务
        {
            for (int i = 0; i < tick; i++)
            {
                Thread.Sleep(100);
                if (serverCallContext.TokenSource.IsCancellationRequested)
                {
                    Console.WriteLine("客户端已经取消该任务！");
                    return false;//实际上在取消时，客户端得不到该值
                }
            }
            return true;
        }
    }

    public class InstanceRpcServer : RpcServer
    {
        public int Count { get; set; }

        [Description("测试调用实例")]
        [TouchRpc]
        public int Increment()//同步服务
        {
            return ++this.Count;
        }
    }

    public class GetCallerRpcServer : RpcServer
    {
        [Description("测试调用上下文")]
        [TouchRpc]
        public string GetCallerID(ICallContext callContext)
        {
            if (callContext.Caller is TcpTouchRpcSocketClient socketClient)
            {
                return socketClient.ID;
            }
            return null;
        }
    }

    [RpcProxy("MyArgs")]
    public class Args
    {
    }
}