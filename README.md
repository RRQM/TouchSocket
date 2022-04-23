<p></p>
<p></p>
<p align="center">
<img src="https://ftp.bmp.ovh/imgs/2021/06/351eeccfadc07014.png" width = "100" height = "100" alt="图片名称" align=center />
</p>

 <div align="center"> 
  
[![NuGet version (RRQMSocket)](https://img.shields.io/nuget/v/RRQMSocket.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSocket/)
[![License](https://img.shields.io/badge/license-Apache%202-4EB1BA.svg)](https://www.apache.org/licenses/LICENSE-2.0.html)
[![Download](https://img.shields.io/nuget/dt/RRQMSocket)](https://www.nuget.org/packages/RRQMSocket/)
[![star](https://gitee.com/dotnetchina/RRQMSocket/badge/star.svg?theme=gvp)](https://gitee.com/dotnetchina/RRQMSocket/stargazers) 
[![fork](https://gitee.com/dotnetchina/RRQMSocket/badge/fork.svg?theme=gvp)](https://gitee.com/dotnetchina/RRQMSocket/members)
<a href="https://jq.qq.com/?_wv=1027&k=gN7UL4fw">
<img src="https://img.shields.io/badge/QQ群-234762506-red" alt="QQ">
</a>
</div>  

<div align="center">

合抱之木，生于毫末；九层之台，起于垒土。

</div>

## 🎀描述

 **_以下Nuget包，均已在此仓开源。使用Apache License 2.0开源协议。_** 

| 名称|地址 |描述|
|---|---|---|
|[![NuGet version (RRQMSocket)](https://img.shields.io/nuget/v/RRQMSocket.svg?label=RRQMSocket)](https://www.nuget.org/packages/RRQMSocket/)|[Gitee](https://gitee.com/dotnetchina/RRQMSocket)<br>[Github](https://github.com/RRQM/RRQMSocket)| RRQMSocket是一个高性能的、超轻量级的网络通信框架。<br>包含了TCP、UDP、Ssl、Channel、Protocol、Token、<br>租户模式等一系列的通信模块。其扩展组件包含：WebSocket、<br>大文件传输、RPC、WebApi、XmlRpc、JsonRpc等内容|
| [![NuGet version (RRQMCore)](https://img.shields.io/nuget/v/RRQMCore.svg?label=RRQMCore)](https://www.nuget.org/packages/RRQMCore)|[Gitee](https://gitee.com/dotnetchina/RRQMSocket)<br>[Github](https://github.com/RRQM/RRQMSocket) | RRQMCore是为RRQM系提供基础服务功能的库，其中包含：<br>**内存池**、**对象池**、**等待逻辑池**、**AppMessenger**、**3DES加密**、<br>**Xml快速存储**、**运行时间测量器**、**文件快捷操作**、<br>**高性能序列化器**、**规范日志接口**等。 |
| [![NuGet version (RRQMSocket.WebSocket)](https://img.shields.io/nuget/v/RRQMSocket.WebSocket.svg?label=RRQMSocket.WebSocket)](https://www.nuget.org/packages/rrqmsocket.websocket)|[Gitee](https://gitee.com/dotnetchina/RRQMSocket)<br>[Github](https://github.com/RRQM/RRQMSocket) |  RRQMSocket.WebSocket是一个高效，超轻量级的WebSocket框架。<br>它包含了Service和Client两大组件，支持Ssl，同时定义了文本、二进制或<br>其他类型数据的快捷发送、分片发送接口，可与js等任意WebSocket组件交互|
| [![NuGet version (RRQMSocket.Http)](https://img.shields.io/nuget/v/RRQMSocket.Http.svg?label=RRQMSocket.Http)](https://www.nuget.org/packages/rrqmsocket.http)|[Gitee](https://gitee.com/dotnetchina/RRQMSocket)<br>[Github](https://github.com/RRQM/RRQMSocket)  |  RRQMSocket.Http是一个能够简单解析Http的服务组件，<br>能够快速响应Http服务请求。|
|[![NuGet version (RRQMSocket.RPC)](https://img.shields.io/nuget/v/RRQMSocket.RPC.svg?label=RRQMSocket.RPC)](https://www.nuget.org/packages/rrqmsocket.rpc)|[Gitee](https://gitee.com/dotnetchina/RRQMSocket)<br>[Github](https://github.com/RRQM/RRQMSocket) |RPC是一个超轻量、高性能、可扩展的微服务管理平台框架，<br>目前已完成开发**RRQMRPC**、**XmlRpc**、**JsonRpc**、**WebApi**部分。<br> **RRQMRPC**部分使用RRQM专属协议，支持客户端**异步调用**，<br>服务端**异步触发**、以及**out**和**ref**关键字，**函数回调**等。<br>在调用效率上也是非常强悍，在调用空载函数，且返回状态时，<br>**10w**次调用仅用时**3.8**秒，不返回状态用时**0.9**秒。<br>其他协议调用性能详看性能评测。
|[![NuGet version (RRQMSocket.RPC.XmlRpc)](https://img.shields.io/nuget/v/RRQMSocket.RPC.XmlRpc.svg?label=RRQMSocket.RPC.XmlRpc)](https://www.nuget.org/packages/rrqmsocket.rpc.xmlrpc)|[Gitee](https://gitee.com/dotnetchina/RRQMSocket)<br>[Github](https://github.com/RRQM/RRQMSocket)| XmlRpc是一个扩展于RRQMSocket.RPC的XmlRpc组件，可以通过<br>该组件创建XmlRpc服务解析器，完美支持XmlRpc数据类型，类型嵌套，<br>Array等，也能与CookComputing.XmlRpcV2完美对接。<br>不限Web，Android等平台。|
| [![NuGet version (RRQMSocket.RPC.JsonRpc)](https://img.shields.io/nuget/v/RRQMSocket.RPC.JsonRpc.svg?label=RRQMSocket.RPC.JsonRpc)](https://www.nuget.org/packages/rrqmsocket.rpc.jsonrpc)|[Gitee](https://gitee.com/dotnetchina/RRQMSocket)<br>[Github](https://github.com/RRQM/RRQMSocket)| JsonRpc是一个扩展于RRQMSocket.RPC的JsonRpc组件，<br>可以通过该组件创建JsonRpc服务解析器，支持JsonRpc全部功能，可与Web，Android等平台无缝对接。|
|[![NuGet version (RRQMSocket.RPC.WebApi)](https://img.shields.io/nuget/v/RRQMSocket.RPC.WebApi.svg?label=RRQMSocket.RPC.WebApi)](https://www.nuget.org/packages/rrqmsocket.rpc.webapi)|[Gitee](https://gitee.com/dotnetchina/RRQMSocket)<br>[Github](https://github.com/RRQM/RRQMSocket)| WebApi是一个扩展于RRQMSocket.RPC的WebApi组件，可以通过<br>该组件创建WebApi服务解析器，让桌面端、Web端、移动端可以<br>跨语言调用RPC函数。功能支持路由、Get传参、Post传参等。|
| [![NuGet version (RRQMSocket.FileTransfer)](https://img.shields.io/nuget/v/RRQMSocket.FileTransfer.svg?label=RRQMSocket.FileTransfer)](https://www.nuget.org/packages/rrqmsocket.filetransfer)|[Gitee](https://gitee.com/dotnetchina/RRQMSocket)<br>[Github](https://github.com/RRQM/RRQMSocket) |  这是一个高性能的C/S架构的文件传输框架，您可以用它传输<br>**任意大小**的文件，它可以完美支持**上传下载混合式队列传输**、<br>**断点续传**、 **快速上传** 、**传输限速**、**获取文件信息**、**删除文件**等。<br>在实际测试中，它的传输速率可达1000Mb/s。 |

## 🎫企业版描述
 **_以下Nuget包，并未开源，其源代码需要付费购买。但是其Nuget包可以免费商用。_** 
| 名称|地址 |描述|
|---|---|---|
|[![NuGet version](https://img.shields.io/nuget/v/RRQMSocketFramework.svg?label=RRQMSocketFramework)](https://www.nuget.org/packages/RRQMSocketFramework/)|[Gitee](https://gitee.com/dotnetchina/RRQMSocket)<br>[Github](https://github.com/RRQM/RRQMSocket) |RRQMSocketFramework是RRQMSocket系列的增强企业版。<br>也是合集版本。这意味着您只需要安装RRQMSocketFramework，<br>即可使用所有组件功能，但是RRQMSocketFramework中也包含了<br>未开源的企业版功能，这需要密钥才能使用，当然也可以试用。<br>所以两者在**基础功能**没有区别，只是在扩展功能上有一定差异性。<br>具体差异请看[企业版相关](https://www.yuque.com/eo2w71/rrqm/80696720a95e415d94c87fa03642513d)|

## 🖥支持环境
- .NET Framework4.5及以上。
- .NET Core3.1及以上。
- .NET Standard2.0及以上。

## 🥪支持框架
- WPF
- Winform
- Blazor
- Xamarin
- Mono
- Unity（在IL2cpp编译时，需要导入源码或添加link.xml，亦或者直接安装RRQMSocketFramework，Mono则直接加载dll即可）
- 其他（即所有C#系）

### unity内link.xml设置(放置在Assets文件夹内)
[unity官方文档 托管代码剥离](https://docs.unity3d.com/cn/current/Manual/ManagedCodeStripping.html#LinkXML)
```
<linker>
	<assembly fullname="RRQMCore" />
	<assembly fullname="RRQMSocket" />
</linker>
```


## 🌴RRQMSocket特点速览

#### 传统IOCP和RRQMSocket

RRQMSocket的IOCP和传统也不一样，就以微软官方示例为例，使用MemoryBuffer开辟一块内存，均分，然后给每个会话分配一个区接收，等收到数据后，再**复制**源数据，然后把复制的数据进行处理。而RRQMSocket是每次接收之前，从内存池拿一个可用内存块，然后**直接用于接收**，等收到数据以后，直接就把这个内存块抛出处理，这样就避免了**复制操作**，虽然只是细小的设计，但是在传输**1000w**次**64kb**的数据时，性能相差了**10倍**。

#### 数据处理适配器

相信大家都使用过其他的Socket产品，那么RRQMSocket在设计时也是借鉴了其他产品的优秀设计理念，数据处理适配器就是其中之一，但和其他产品的设计不同的是，RRQMSocket的适配器功能更加强大，它不仅可以提前解析数据包，还可以解析数据对象。例如：可以使用固定包头对数据进行预处理，从而解决**数据分包**、**粘包**的问题。也可以直接解析**HTTP**数据协议、WebSocket数据协议等。

#### 兼容性与适配

RRQMSocket提供多种框架模型，能够完全兼容基于TCP、UDP协议的所有协议。例如：TcpService与TcpClient，其基础功能和Socket一模一样，只是增强了框架的**坚固性**和**并发性**，将**连接**和**接收数据**通过事件的形式抛出，让使用者能够更加友好的使用。

## 🔗联系作者

- [CSDN博客主页](https://blog.csdn.net/qq_40374647)
- [哔哩哔哩视频](https://space.bilibili.com/94253567)
- [源代码仓库主页](https://gitee.com/RRQM_Home) 
- 交流QQ群：234762506

## 🌟说明文档
- [ 文档首页 ](https://www.yuque.com/eo2w71/rrqm/2c5dab34026d2b45ada6e51ae9e51a5a)

## ✨简单示例

 **_以下仅以最简方式创建示例，更多详情请查看[说明文档](https://www.yuque.com/eo2w71/rrqm/2c5dab34026d2b45ada6e51ae9e51a5a)。_** 

 **【TcpService】** 

```
TcpService service = new TcpService();
service.Connecting += (client, e) => { };//有客户端正在连接
service.Connected += (client, e) => { };//有客户端连接
service.Disconnected += (client, e) => { };//有客户端断开连接
service.Received += (client, byteBlock, requestInfo) =>
{
    //从客户端收到信息
    string mes = byteBlock.ToString();
    Console.WriteLine($"已从{client.ID}接收到信息：{mes}");

    client.Send(mes);//将收到的信息直接返回给发送方

    //client.Send("id",mes);//将收到的信息返回给特定ID的客户端

    var clients = service.GetClients();
    foreach (var targetClient in clients)//将收到的信息返回给在线的所有客户端。
    {
        if (targetClient.ID != client.ID)
        {
            targetClient.Send(mes);
        }
    }
};

service.Setup(new RRQMConfig()//载入配置     
    .SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
    .SetMaxCount(10000)
    .SetThreadCount(100))
    .Start();//启动
```

 **【TcpClient】** 
```
TcpClient tcpClient = new TcpClient();
tcpClient.Connected += (client, e) => { };//成功连接到服务器
tcpClient.Disconnected += (client, e) => { };//从服务器断开连接，当连接不成功时不会触发。
tcpClient.Received += (client, byteBlock, requestInfo) =>
{
    //从服务器收到信息
    string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
    Console.WriteLine($"接收到信息：{mes}");
};

//声明配置
RRQMConfig config = new RRQMConfig();
config.SetRemoteIPHost(new IPHost("127.0.0.1:7789"))
    .UsePlugin()
    .SetBufferLength(1024 * 10);

//载入配置
tcpClient.Setup(config);
tcpClient.Connect();
tcpClient.Send("RRQM");
```

 **【TcpClient 断线重连】** 

```
tcpClient.UseReconnection(tryCount:5,printLog:true);
```

 **【FixedHeaderPackageAdapter包模式】** 

该适配器主要解决TCP粘分包问题，数据格式采用简单而高效的“包头+数据体”的模式，其中包头支持：

- Byte模式（1+n），一次性最大接收255字节的数据。
- Ushort模式（2+n），一次最大接收65535字节。
- Int模式（4+n），一次最大接收2G数据。

以上数据头均采用RRQMBitConverter的默认端模式（小端模式），使用者可以根据需求切换默认端模式。

```
RRQMBitConverter.DefaultEndianType = EndianType.Little;
```

 **【CustomFixedHeaderDataHandlingAdapter】** 

用户自定义固定包头适配器，主要帮助用户解决具有固定包头的数据帧信息。例如：下列数据格式，仅需要实现几个接口，就能完成解析，详细操作请参照API。

|1|1|1|**********|

 **【CustomUnfixedHeaderDataHandlingAdapter】** 

用户自定义不固定包头适配器，主要帮助用户解决具有包头不固定的数据帧信息。例如：最典型的HTTP数据包，其数据头和数据体由“\r\n”隔开，而数据头又因为请求者的请求信息的不同，头部数据也不固定，而数据体的长度，也是由数据头的ContentLength的值显式指定的，所以可以考虑使用CustomUnfixedHeaderDataHandlingAdapter解析，也是仅通过简单的开发，就能实现。


 **【Ssl加密】** 

在[RRQMBox](https://gitee.com/RRQM_Home/RRQMBox/tree/master/Ssl%E8%AF%81%E4%B9%A6%E7%9B%B8%E5%85%B3)中，放置了一个自制Ssl证书，密码为“RRQMSocket”以供测试。使用配置非常方便。

在服务器中只需设置配置SslOption属性和接收模式。

 **服务器配置** 
```
config.SetServerSslOption(new ServiceSslOption() { Certificate = new X509Certificate2("RRQMSocket.pfx", "RRQMSocket"), SslProtocols = SslProtocols.Tls12 });
```

 **客户端配置** 

```
config.SetClientSslOption(new ClientSslOption()
{
    ClientCertificates = new X509CertificateCollection() { new X509Certificate2("RRQMSocket.pfx", "RRQMSocket") },
    SslProtocols = SslProtocols.Tls12,
    TargetHost = "127.0.0.1",
    CertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => { return true; }
});
```

 **【Http服务器】** 

HttpService支持**Https协议**、**静态页面**、**WebSocket**、**JsonRpc**、**XmlRpc**、**WebApi**插件的挂载。

```
var service = new HttpService();

service.AddPlugin<MyHttpPlug>();
service.AddPlugin<HttpStaticPagePlugin>().
   AddFolder("../../../../../api");//添加静态页面

service.AddPlugin<WebSocketServerPlugin>().//添加WebSocket功能
   SetTimeout(10 * 1000).
   SetWSUrl("/ws").
   SetCallback(WSCallback);

service.AddPlugin<MyWebSocketPlugin>();//添加WS事务触发。

service.AddPlugin<MyWSCommandLinePlugin>();//添加WS命令行事务。

var config = new RRQMConfig();
config.UsePlugin()
    .SetReceiveType(ReceiveType.Auto)
    .SetListenIPHosts(new IPHost[] { new IPHost(7789) });

service.Setup(config).Start();
Console.WriteLine("Http服务器已启动");
Console.WriteLine("浏览器访问：http://127.0.0.1:7789/index.html");
Console.WriteLine("WS访问：ws://127.0.0.1:7789/ws");
```

 **【WebSocket客户端】** 
```csharp
WSClient myWSClient = new WSClient();
myWSClient.Setup("ws://127.0.0.1:7789/ws");
myWSClient.Connect();
Console.WriteLine("连接成功");

Console.WriteLine("连接成功");
while (true)
{
    myWSClient.SendWithWS(Console.ReadLine());
}
```

 **【RPC调用】** 

- WebApi：下列服务，可让浏览器通过`url/XUnitTestServer/Sum?a=10&b=20`来调用，结果可选xml或json。
- JsonRpc：下列服务，可让普通TCP使用`{"jsonrpc":"2.0","method":"Sum","params":[10，20],"id":1}`来调用，也能让web通过http/https来调用。
- xmlRpc：下列服务，可通过http+xml的形式调用。
- RRQMRPC：使用专有协议调用。

```
[Route("/[controller]/[action]")]
public class XUnitTestServer : ControllerBase
{

    [XmlRpc]
    [JsonRpc]
    [Route]
    [RRQMRPC]
    public int Sum(int a, int b)
    {
        return a + b;
    }
}
```


## 🧲应用场景模拟
[场景入口](https://www.yuque.com/eo2w71/rrqm/b138b52168853afb65369ca8171f14b9)

***

## 致谢

谢谢大家对RRQM的支持，如果还有其他问题，请加群QQ：234762506讨论。

## 支持作者

[支持入口](https://www.yuque.com/eo2w71/rrqm/a5199820843b324f025633fdeee44394)
