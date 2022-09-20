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
using RpcArgsClassLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TouchSocket.Core.Log;
using TouchSocket.Core.XREF.Newtonsoft.Json.Linq;
using TouchSocket.Http.WebSockets;
using TouchSocket.Rpc;
using TouchSocket.Rpc.JsonRpc;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Rpc.WebApi;

namespace RRQMService.XUnitTest.Server
{
    public enum MyEnum
    {
        T1 = 0,
        T2 = 100,
        T3 = 200
    }

    public struct StructArgs
    {
        public int P1 { get; set; }
    }

    public class Args
    {
        public int P1 { get; set; }
        public double P2 { get; set; }
        public string P3 { get; set; }
    }

    public class Class01
    {
        public int Age { get; set; } = 1;
        public string Name { get; set; }
    }

    public class Class02
    {
        public int Age { get; set; }
        public List<int> list { get; set; }
        public string Name { get; set; }
        public int[] nums { get; set; }
    }

    public class Class03 : Class02
    {
        public int Length { get; set; }
    }

    public class Class04
    {
        public int P1 { get; set; }
        public string P2 { get; set; }
        public int P3 { get; set; }
    }

    public class MyClass
    {
        public int P1 { get; set; }
    }

    public class XUnitTestController : TransientRpcServer
    {
        public static bool isStart;

        public static Action<string> ShowMsgMethod;

        private int a;

        private int invokeCount;

        public XUnitTestController()
        {
            System.Timers.Timer timer = new System.Timers.Timer(1000);
            timer.Elapsed += this.Timer_Elapsed;
            timer.Start();

            Console.WriteLine($"{this.GetType().Name}构造函数");
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitWebApi(HttpMethodType.GET)]
        [XunitTouchRpc]
        public int Sum(int a, int b)
        {
            return a + b;
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitWebApi(HttpMethodType.GET)]
        [XunitTouchRpc]
        [Description("性能测试")]
        public void Test01_Performance()
        {
            this.a++;
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitTouchRpc]
        [Description("测试异步字符串")]
        public async Task<string> Test02_TaskString(string msg)
        {
            return await Task.Run(() =>
            {
                return msg;
            });
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitWebApi(HttpMethodType.GET)]
        [TouchRpc]
        public ProxyClass1 Test03_GetProxyClass()
        {
            return new ProxyClass1() { P1 = 10, P2 = new ProxyClass2() { P1 = 100, P2 = new ProxyClass3() { P1 = 1000 } } };
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitTouchRpc]
        public int Test04_In32DefaultValue(int a = 100)
        {
            return a;
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitTouchRpc]
        public void Test05_NoneReturnNoneParameter()
        {
        }

        [XunitTouchRpc]
        public void Test06_OutParameters(out string name, out int age, out string occupation)
        {
            name = "若汝棋茗";
            age = 18;
            occupation = "搬砖工程师";
        }

        [XunitTouchRpc]
        public void Test07_OutStringParameter(out string name)
        {
            name = "若汝棋茗";
        }

        [XunitTouchRpc]
        public void Test08_RefStringParameter(ref string name)
        {
            name = name + "ref";
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitTouchRpc]
        public bool Test09_Boolean(bool b)
        {
            return b;
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitTouchRpc]
        public string Test10_StringDefaultNullValue(string s = null)
        {
            return s;
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitTouchRpc]
        public string Test11_StringDefaultValue(string s = "RRQM")
        {
            return s;
        }

        [XunitJsonRpc]
        [XunitTouchRpc]
        public Dictionary<int, string> Test12_Dictionary(int length)
        {
            Dictionary<int, string> valuePairs = new Dictionary<int, string>();
            for (int i = 0; i < length; i++)
            {
                valuePairs.Add(i, i.ToString());
            }

            return valuePairs;
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitTouchRpc]
        public async Task Test13_Task()
        {
            await Task.Run(() =>
            {
                this.ShowMsg("TestTaskAsync");
            });
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitWebApi(HttpMethodType.GET)]
        [XunitTouchRpc]
        public List<Class01> Test14_ListClass01(int length)
        {
            List<Class01> list = new List<Class01>();
            for (int i = 0; i < length; i++)
            {
                list.Add(new Class01() { Age = i });
            }
            return list;
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitWebApi(HttpMethodType.GET)]
        [XunitTouchRpc]
        public Args Test15_ReturnArgs()
        {
            return new Args() { P1 = 10, P2 = 10.0, P3 = "RRQM" };
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitTouchRpc]
        public Class04 Test16_ReturnClass4(int a, string b, int c = 10)
        {
            return new Class04() { P1 = a, P2 = b, P3 = c };
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitTouchRpc]
        public double Test17_DoubleDefaultValue(double a = 3.1415926)
        {
            return a;
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitTouchRpc]
        public Class01 Test18_Class1(Class01 class01)
        {
            return class01;
        }

        [XunitTouchRpc]
        public string Test19_CallBacktcpRpcService(string id, int age)
        {
            //先在RPC服务器中找到TcpRpc解析器。
            TcpTouchRpcService tcpRpcService = (TcpTouchRpcService)this.RpcStore["tcpRPCParser"];

            string msg = tcpRpcService.Invoke<string>(id, "xunittest.rpc.tcp.callbackserver.sayhello", InvokeOption.WaitInvoke, age);
            this.ShowMsg($"TestCallBack，mes={msg}");
            return msg;
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitTouchRpc]
        public string Test20_XmlRpc(string param, int a, double b, Args[] args)
        {
            return "RRQM";
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitTouchRpc]
        public JObject Test21_JsonRpcReturnJObject()
        {
            JObject jobj = new JObject();
            jobj.Add("P1", "P1");
            jobj.Add("P2", "P2");
            jobj.Add("P3", "P3");
            return jobj;
        }

        [XunitJsonRpc]
        [XunitTouchRpc]
        public int Test22_IncludeCaller(int a)
        {
            if (this.CallContext is IJsonRpcCallContext jsonRpcServerCallContext)
            {
            }
            return a;
        }

        [XunitTouchRpc]
        public int Test23_InvokeType()
        {
            if (this.CallContext is ITouchRpcCallContext context)
            {
            }

            return this.invokeCount++;
        }

        [XunitTouchRpc]
        public int Test25_TestStruct(StructArgs structArgs)
        {
            return structArgs.P1;
        }

        [XunitTouchRpc]
        public int Test26_TestCancellationToken()
        {
            int i = 0;
            this.CallContext.TokenSource.Token.Register(() =>
            {
                this.ShowMsg($"任务已取消，i={i}");
            });

            for (; i < 500; i++)
            {
                if (this.CallContext.TokenSource.Token.IsCancellationRequested)
                {
                    return 10;
                }
                Thread.Sleep(20);
            }

            return 1;
        }

        [XunitTouchRpc]
        public void Test27_TestCallBackFromCallContext()
        {
            if (this.CallContext.Caller is TcpTouchRpcSocketClient socketClient)
            {
                Task.Run(() =>
                {
                    string msg = socketClient.Invoke<string>("xunittest.rpc.http.callbackserver.sayhello", InvokeOption.WaitInvoke, 10);
                    this.ShowMsg($"TestCallBack，mes={msg}");
                });
            }
        }

        /// <summary>
        /// "测试从RPC创建通道，从而实现流数据的传输"
        /// </summary>
        /// <param name="serverCallContext"></param>
        /// <param name="channelID"></param>
        [Description("测试从RPC创建通道，从而实现流数据的传输")]
        [XunitTouchRpc]
        public void Test28_TestChannel(int channelID)
        {
            if (this.CallContext.Caller is TcpTouchRpcSocketClient socketClient)
            {
                if (socketClient.TrySubscribeChannel(channelID, out Channel channel))
                {
                    for (int i = 0; i < 1024; i++)
                    {
                        channel.Write(new byte[1024]);
                    }
                }
            }
        }

        [XunitWebApi(HttpMethodType.POST)]
        public MyClass Test29_TestPost(int a, MyClass myClass)
        {
            return new MyClass() { P1 = a + myClass.P1 };
        }

        [XunitTouchRpc]
        public string Test30_CallBackHttpRpcService(string id, int age)
        {
            //先在RPC服务器中找到TcpRpc解析器。
            HttpTouchRpcService httpRpcService = (HttpTouchRpcService)this.RpcStore["m_httpService"];

            string msg = httpRpcService.Invoke<string>(id, "xunittest.rpc.http.callbackserver.sayhello", InvokeOption.WaitInvoke, age);
            this.ShowMsg($"HttpRpcServiceTestCallBack，mes={msg}");
            return msg;
        }

        [XunitTouchRpc]
        public string Test31_DefaultBool(bool a = true, bool b = false)
        {
            return "rrqm";
        }

        [WebApi(HttpMethodType.GET)]
        public void Test31_WebSocket()
        {
            if (this.CallContext.Caller is HttpTouchRpcSocketClient client)
            {
                bool b = client.SwitchProtocolToWebSocket(((IWebApiCallContext)this.CallContext).HttpContext);
                client.Logger.Info($"手动升级WS，状态：{b}");
            }
        }

        private void ShowMsg(string msg)
        {
            ShowMsgMethod?.Invoke(msg);
            Console.WriteLine(msg);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (isStart)
            {
                this.ShowMsg($"PerformanceTest,处理{this.a}次");
                this.a = 0;
            }
        }
    }
}