//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using Newtonsoft.Json.Linq;
using RpcArgsClassLib;
using System.ComponentModel;
using System.Timers;
using TouchSocket.Core;
using TouchSocket.Http.WebSockets;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.WebApi;
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace XUnitTestConsoleApp.Server
{

    [GeneratorRpcServer]
    public partial class XUnitTestController : TransientRpcServer
    {
        public static bool isStart;

        public static Action<string> ShowMsgMethod;

        private int a;

        private int invokeCount;

        public XUnitTestController()
        {
            var timer = new System.Timers.Timer(1000);
            timer.Elapsed += this.Timer_Elapsed;
            timer.Start();
            Console.WriteLine($"{this.GetType().Name}构造函数");
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitWebApi(HttpMethodType.GET)]
        [XunitDmtpRpc]
        public int Sum(int a, int b)
        {
            return a + b;
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitWebApi(HttpMethodType.GET)]
        [XunitDmtpRpc]
        [Description("性能测试")]
        public void Test01_Performance()
        {
            this.a++;
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitDmtpRpc]
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
        [DmtpRpc]
        public ProxyClass1 Test03_GetProxyClass()
        {
            return new ProxyClass1() { P1 = 10, P2 = new ProxyClass2() { P1 = 100, P2 = new ProxyClass3() { P1 = 1000 } } };
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitDmtpRpc]
        public int Test04_In32DefaultValue(int a = 100)
        {
            return a;
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitDmtpRpc]
        public void Test05_NoneReturnNoneParameter()
        {
        }

        [XunitDmtpRpc]
        public void Test06_OutParameters(out string name, out int age, out string occupation)
        {
            name = "若汝棋茗";
            age = 18;
            occupation = "搬砖工程师";
        }

        [XunitDmtpRpc]
        public void Test07_OutStringParameter(out string name)
        {
            name = "若汝棋茗";
        }

        [XunitDmtpRpc]
        public void Test08_RefStringParameter(ref string name)
        {
            name = name + "ref";
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitDmtpRpc]
        public bool Test09_Boolean(bool b)
        {
            return b;
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitDmtpRpc]
        public string Test10_StringDefaultNullValue(string s = null)
        {
            return s;
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitDmtpRpc]
        public string Test11_StringDefaultValue(string s = "RRQM")
        {
            return s;
        }

        [XunitJsonRpc]
        [XunitDmtpRpc]
        public Dictionary<int, string> Test12_Dictionary(int length)
        {
            var valuePairs = new Dictionary<int, string>();
            for (var i = 0; i < length; i++)
            {
                valuePairs.Add(i, i.ToString());
            }

            return valuePairs;
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitDmtpRpc]
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
        [XunitDmtpRpc]
        public List<Class01> Test14_ListClass01(int length)
        {
            var list = new List<Class01>();
            for (var i = 0; i < length; i++)
            {
                list.Add(new Class01() { Age = i });
            }
            return list;
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitWebApi(HttpMethodType.GET)]
        [XunitDmtpRpc]
        public Args Test15_ReturnArgs()
        {
            return new Args() { P1 = 10, P2 = 10.0, P3 = "RRQM" };
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitDmtpRpc]
        public Class04 Test16_ReturnClass4(int a, string b, int c = 10)
        {
            return new Class04() { P1 = a, P2 = b, P3 = c };
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitDmtpRpc]
        public double Test17_DoubleDefaultValue(double a = 3.1415926)
        {
            return a;
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitDmtpRpc]
        public Class01 Test18_Class1(Class01 class01)
        {
            return class01;
        }

        [XunitDmtpRpc]
        [XunitJsonRpc]
        public string Test19_CallBacktcpRpcService(string id, int age)
        {
            try
            {
                if (this.CallContext.Caller is IDmtpActorObject dmtp)
                {
                    string msg = dmtp.GetDmtpRpcActor().InvokeT<string>(id, "SayHello", InvokeOption.WaitInvoke, age);
                    this.ShowMsg($"TestCallBack，mes={msg}");
                    return msg;
                }
                return default;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitDmtpRpc]
        public string Test20_XmlRpc(string param, int a, double b, Args[] args)
        {
            return "RRQM";
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitDmtpRpc]
        public JObject Test21_JsonRpcReturnJObject()
        {
            var jobj = new JObject();
            jobj.Add("P1", "P1");
            jobj.Add("P2", "P2");
            jobj.Add("P3", "P3");
            return jobj;
        }

        [XunitJsonRpc]
        [XunitDmtpRpc]
        public int Test22_IncludeCaller(int a)
        {
            if (this.CallContext is IJsonRpcCallContext jsonRpcServerCallContext)
            {
            }
            return a;
        }

        [XunitDmtpRpc]
        public int Test23_InvokeType()
        {
            if (this.CallContext is IDmtpRpcCallContext context)
            {
            }

            return this.invokeCount++;
        }

        [XunitDmtpRpc]
        public int Test25_TestStruct(StructArgs structArgs)
        {
            return structArgs.P1;
        }

        [XunitDmtpRpc]
        public int Test26_TestCancellationToken()
        {
            var i = 0;
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

        [XunitDmtpRpc]
        public void Test27_TestCallBackFromCallContext()
        {
            if (this.CallContext.Caller is TcpDmtpSocketClient socketClient)
            {
                Task.Run(() =>
                {
                    string msg = socketClient.GetDmtpRpcActor().InvokeT<string>("SayHello", InvokeOption.WaitInvoke, 10);
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
        [XunitDmtpRpc]
        public void Test28_TestChannel(int channelID)
        {
            if (this.CallContext.Caller is TcpDmtpSocketClient socketClient)
            {
                if (socketClient.TrySubscribeChannel(channelID, out var channel))
                {
                    for (var i = 0; i < 1024; i++)
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

        [XunitDmtpRpc]
        public string Test30_CallBackHttpRpcService(string id, int age)
        {
            string msg = ((IDmtpActorObject)this.CallContext.Caller).GetDmtpRpcActor().InvokeT<string>(id, "SayHello", InvokeOption.WaitInvoke, age);
            this.ShowMsg($"HttpRpcServiceTestCallBack，mes={msg}");
            return msg;
        }

        [XunitDmtpRpc]
        public string Test31_DefaultBool(bool a = true, bool b = false)
        {
            return "rrqm";
        }

        [WebApi(HttpMethodType.GET)]
        public void Test31_WebSocket()
        {
            if (this.CallContext.Caller is HttpDmtpSocketClient client)
            {
                var b = client.SwitchProtocolToWebSocket(((IWebApiCallContext)this.CallContext).HttpContext);
                client.Logger.Info($"手动升级WS，状态：{b}");
            }
        }

        [JsonRpc("WSJsonRpc")]
        public int Test32_WSJsonRpc(int a, int b)
        {
            var callContext = this.CallContext;
            return a + b;
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitWebApi(HttpMethodType.POST)]
        [XunitDmtpRpc]
        public (int a, string b) Test33_TupleElementNames((int a, string b) tuple)
        {
            return tuple;
        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitWebApi(HttpMethodType.POST)]
        [XunitDmtpRpc]
        public (int a, string b) Test34_RefTupleElementNames(ref (int a, string b) tuple)
        {
            return tuple;
        }

        [XunitDmtpRpc]
        public void Test35_ListTupleElementNames(List<(int a1, (int a2, string a3) a4)> tuple)
        {

        }

        [XunitDmtpRpc]
        public void Test36_ListRefTupleElementNames(ref List<(int a1, (int a2, string a3) a4)> tuple)
        {

        }

        [XunitDmtpRpc]
        public void Test37_ListTupleElementNames2(List<(int b1, (int b2, string b3) b4)> tuple)
        {

        }

        [XunitJsonRpc]
        public string Test38_CallBackJsonRpcService(string id, int age)
        {
            try
            {
                string msg = ((ISocketClient)this.CallContext.Caller).GetJsonRpcActionClient().InvokeT<string>("SayHello", InvokeOption.WaitInvoke, age);
                this.ShowMsg($"TestCallBack，mes={msg}");
                return msg;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        [XunitXmlRpc]
        [XunitJsonRpc]
        [XunitWebApi(HttpMethodType.POST)]
        [XunitDmtpRpc]
        public Task Test39_Task()
        {
            return Task.CompletedTask;
        }

        [XunitDmtpRpc]
        public Metadata Test40_CallContextMetadata(IDmtpRpcCallContext callContext)
        {
            return callContext.Metadata;
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