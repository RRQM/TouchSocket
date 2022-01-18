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
<div align="center">
<img src="https://i.bmp.ovh/imgs/2021/06/0bb09b575906f48b.png"  alt="图片名称" align=center />
</div>

## 💿描述
| 名称|地址 |描述|
|---|---|---|
|[![NuGet version (RRQMSocket)](https://img.shields.io/nuget/v/RRQMSocket.svg?label=RRQMSocket)](https://www.nuget.org/packages/RRQMSocket/)|[Gitee](https://gitee.com/dotnetchina/RRQMSocket)<br>[Github](https://github.com/RRQM/RRQMSocket)| RRQMSocket是一个整合性的、超轻量级的网络通信框架。<br>包含了TCP、UDP、Ssl、Channel、Protocol、Token、<br>租户模式等一系列的通信模块。其扩展组件包含：WebSocket、<br>大文件传输、RPC、WebApi、XmlRpc、JsonRpc等内容|
|[![NuGet version](https://img.shields.io/nuget/v/RRQMSocketFramework.svg?label=RRQMSocketFramework)](https://www.nuget.org/packages/RRQMSocketFramework/)|[Gitee](https://gitee.com/dotnetchina/RRQMSocket)<br>[Github](https://github.com/RRQM/RRQMSocket) |**RRQMSocketFramework**是RRQMSocket系列的增强企业版，<br>两者在基础功能上没有区别，但是在扩展功能上有一定差异性，<br>例如RPC中的EventBus、文件传输中的限速功能等，<br>具体差异请看[RRQM商业运营](https://gitee.com/RRQM_Home/RRQMBox/wikis/%E5%95%86%E4%B8%9A%E8%BF%90%E8%90%A5)|

## 🎀依赖、扩展库
| 名称|地址 |描述|
|---|---|---|
| [![NuGet version (RRQMCore)](https://img.shields.io/nuget/v/RRQMCore.svg?label=RRQMCore)](https://www.nuget.org/packages/RRQMCore)|[Gitee](https://gitee.com/dotnetchina/RRQMSocket)<br>[Github](https://github.com/RRQM/RRQMSocket) | RRQMCore是为RRQM系提供基础服务功能的库，其中包含：<br>**内存池**、**对象池**、**等待逻辑池**、**AppMessenger**、**3DES加密**、<br>**Xml快速存储**、**运行时间测量器**、**文件快捷操作**、<br>**高性能序列化器**、**规范日志接口**等。 |
| [![NuGet version (RRQMSocket.WebSocket)](https://img.shields.io/nuget/v/RRQMSocket.WebSocket.svg?label=RRQMSocket.WebSocket)](https://www.nuget.org/packages/rrqmsocket.websocket)|[Gitee](https://gitee.com/dotnetchina/RRQMSocket)<br>[Github](https://github.com/RRQM/RRQMSocket) |  RRQMSocket.WebSocket是一个高效，超轻量级的WebSocket框架。<br>它包含了Service和Client两大组件，支持Ssl，同时定义了文本、二进制或<br>其他类型数据的快捷发送、分片发送接口，可与js等任意WebSocket组件交互|
| [![NuGet version (RRQMSocket.Http)](https://img.shields.io/nuget/v/RRQMSocket.Http.svg?label=RRQMSocket.Http)](https://www.nuget.org/packages/rrqmsocket.http)|[Gitee](https://gitee.com/dotnetchina/RRQMSocket)<br>[Github](https://github.com/RRQM/RRQMSocket)  |  RRQMSocket.Http是一个能够简单解析Http的服务组件，<br>能够快速响应Http服务请求。|
|[![NuGet version (RRQMSocket.RPC)](https://img.shields.io/nuget/v/RRQMSocket.RPC.svg?label=RRQMSocket.RPC)](https://www.nuget.org/packages/rrqmsocket.rpc)|[Gitee](https://gitee.com/RRQM_Home/rrqmsocket.rpc)<br>[Github](https://github.com/RRQM/RRQMSocket.RPC) |RPC是一个超轻量、高性能、可扩展的微服务管理平台框架，<br>目前已完成开发**RRQMRPC**、**XmlRpc**、**JsonRpc**、**WebApi**部分。<br> **RRQMRPC**部分使用RRQM专属协议，支持客户端**异步调用**，<br>服务端**异步触发**、以及**out**和**ref**关键字，**函数回调**等。<br>在调用效率上也是非常强悍，在调用空载函数，且返回状态时，<br>**10w**次调用仅用时**3.8**秒，不返回状态用时**0.9**秒。<br>其他协议调用性能详看性能评测。
|[![NuGet version (RRQMSocket.RPC.XmlRpc)](https://img.shields.io/nuget/v/RRQMSocket.RPC.XmlRpc.svg?label=RRQMSocket.RPC.XmlRpc)](https://www.nuget.org/packages/rrqmsocket.rpc.xmlrpc)|[Gitee](https://gitee.com/RRQM_Home/rrqmsocket.rpc)<br>[Github](https://github.com/RRQM/RRQMSocket.RPC) | XmlRpc是一个扩展于RRQMSocket.RPC的XmlRpc组件，可以通过<br>该组件创建XmlRpc服务解析器，完美支持XmlRpc数据类型，类型嵌套，<br>Array等，也能与CookComputing.XmlRpcV2完美对接。<br>不限Web，Android等平台。|
| [![NuGet version (RRQMSocket.RPC.JsonRpc)](https://img.shields.io/nuget/v/RRQMSocket.RPC.JsonRpc.svg?label=RRQMSocket.RPC.JsonRpc)](https://www.nuget.org/packages/rrqmsocket.rpc.jsonrpc)|[Gitee](https://gitee.com/RRQM_Home/rrqmsocket.rpc)<br>[Github](https://github.com/RRQM/RRQMSocket.RPC) | JsonRpc是一个扩展于RRQMSocket.RPC的JsonRpc组件，<br>可以通过该组件创建JsonRpc服务解析器，支持JsonRpc全部功能，可与Web，Android等平台无缝对接。|
|[![NuGet version (RRQMSocket.RPC.WebApi)](https://img.shields.io/nuget/v/RRQMSocket.RPC.WebApi.svg?label=RRQMSocket.RPC.WebApi)](https://www.nuget.org/packages/rrqmsocket.rpc.webapi)|[Gitee](https://gitee.com/RRQM_Home/rrqmsocket.rpc)<br>[Github](https://github.com/RRQM/RRQMSocket.RPC) | WebApi是一个扩展于RRQMSocket.RPC的WebApi组件，可以通过<br>该组件创建WebApi服务解析器，让桌面端、Web端、移动端可以<br>跨语言调用RPC函数。功能支持路由、Get传参、Post传参等。|
| [![NuGet version (RRQMSocket.FileTransfer)](https://img.shields.io/nuget/v/RRQMSocket.FileTransfer.svg?label=RRQMSocket.FileTransfer)](https://www.nuget.org/packages/rrqmsocket.filetransfer)|[Gitee](https://gitee.com/RRQM_Home/rrqmsocket.filetransfer)<br>[Github](https://github.com/RRQM/RRQMSocket.FileTransfer) |  这是一个高性能的C/S架构的文件传输框架，您可以用它传输<br>**任意大小**的文件，它可以完美支持**上传下载混合式队列传输**、<br>**断点续传**、 **快速上传** 、**传输限速**、**获取文件信息**、**删除文件**等。<br>在实际测试中，它的传输速率可达1000Mb/s。 |


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
- Unity（在IL2cpp编译时，需要导入源码）
- 其他（即所有C#系）

## 🌴RRQMSocket特点速览

#### 传统IOCP和RRQMSocket

RRQMSocket的IOCP和传统也不一样，就以微软官方示例为例，使用MemoryBuffer开辟一块内存，均分，然后给每个会话分配一个区接收，等收到数据后，再**复制**源数据，然后把复制的数据进行处理。而RRQMSocket是每次接收之前，从内存池拿一个可用内存块，然后**直接用于接收**，等收到数据以后，直接就把这个内存块抛出处理，这样就避免了**复制操作**，虽然只是细小的设计，但是在传输**1000w**次**64kb**的数据时，性能相差了**10倍**。

#### 数据处理适配器

相信大家都使用过其他的Socket产品，例如HPSocket，SuperSocket等，那么RRQMSocket在设计时也是借鉴了其他产品的优秀设计理念，数据处理适配器就是其中之一，但和其他产品的设计不同的是，RRQMSocket的适配器功能更加强大，它不仅可以提前解析数据包，还可以解析数据对象。例如：可以使用固定包头对数据进行预处理，从而解决**数据分包**、**粘包**的问题。也可以直接解析**HTTP**数据协议、WebSocket数据协议等。

#### 兼容性与适配

RRQMSocket提供多种框架模型，能够完全兼容基于TCP、UDP协议的所有协议。例如：TcpService与TcpClient，其基础功能和Socket一模一样，只是增强了框架的**坚固性**和**并发性**，将**连接**和**接收数据**通过事件的形式抛出，让使用者能够更加友好的使用。

## 🔗联系作者

- [CSDN博客主页](https://blog.csdn.net/qq_40374647)
- [哔哩哔哩视频](https://space.bilibili.com/94253567)
- [源代码仓库主页](https://gitee.com/RRQM_Home) 
- 交流QQ群：234762506

## 🌟API手册
- [ API首页 ](https://gitee.com/RRQM_Home/RRQMBox/wikis/API%E6%89%8B%E5%86%8C)
- [说明](https://gitee.com/RRQM_Home/RRQMBox/wikis/%E8%AF%B4%E6%98%8E(%E4%BD%BF%E7%94%A8%E5%89%8D%E5%BF%85%E8%A6%81%E9%98%85%E8%AF%BB))
- [ 历史更新 ](https://gitee.com/RRQM_Home/RRQMBox/wikis/%E5%8E%86%E5%8F%B2%E6%9B%B4%E6%96%B0)
- [ 商业运营 ](https://gitee.com/RRQM_Home/RRQMBox/wikis/%E5%95%86%E4%B8%9A%E8%BF%90%E8%90%A5)
- [疑难解答](https://gitee.com/RRQM_Home/RRQMBox/wikis/%E7%96%91%E9%9A%BE%E8%A7%A3%E7%AD%94)


## ✨简单示例

 **_更多配置请查看API文档的配置说明文档，一下仅以最简方式创建实例。_** 

 **【TcpService】** 

```
SimpleTcpService service = new SimpleTcpService();
service.Connecting += (client, e) =>{};//有客户端正在连接
service.Connected += (client, e) =>{};//有客户端连接
service.Disconnected += (client, e) =>{};//有客户端断开连接
service.Received += (client, byteBlock, obj) =>
{
    //从客户端收到信息
    string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
    Console.WriteLine($"已从{client.Name}接收到信息：{mes}");//Name即IP+Port
};
//声明配置
var config = new TcpServiceConfig();
config.ListenIPHosts = new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) };//同时监听两个地址
//载入配置
service.Setup(config);
//启动
 service.Start();
```

 **【TcpClient】** 
```
SimpleTcpClient tcpClient = new SimpleTcpClient();
tcpClient.Connected += (client, e) =>{};//成功连接到服务器
tcpClient.Disconnected += (client, e) =>{};//从服务器断开连接，当连接不成功时不会触发。
//载入配置
tcpClient.Setup("127.0.0.1:7789");
tcpClient.Connect();
tcpClient.Send(Encoding.UTF8.GetBytes("RRQM"));
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

在服务器中只需设置配置SslOption属性和接收模式（接收模式在Ssl模式下只支持BIO和Select）。

服务器配置
```
config.SslOption = new ServiceSslOption() { Certificate = new X509Certificate2("RRQMSocket.pfx", "RRQMSocket"), SslProtocols = SslProtocols.Tls12 };
config.ReceiveType = ReceiveType.Select;
```

客户端配置

```
config.ReceiveType = ReceiveType.BIO;
config.SslOption = new ClientSslOption()
{
    ClientCertificates = new X509CertificateCollection() { new X509Certificate2("RRQMSocket.pfx", "RRQMSocket") },
    SslProtocols = SslProtocols.Tls12,
    TargetHost = "127.0.0.1",
    CertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => { return true; }
};
```

【WS服务器】

```
WSService wSService = new WSService();
wSService.Received += WSService_Received;
wSService.Connected += WSService_Connected;

var config = new WSServiceConfig();
config.ListenIPHosts = new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) };//同时监听两个地址
config.ReceiveType = ReceiveType.IOCP;

wSService.Setup(config).Start();
Console.WriteLine("WS服务器已启动");
```

【WS客户端】

```
SimpleWSClient myWSClient = new SimpleWSClient();
myWSClient.Received += MyWSClient_Received;
WSClientConfig config = new WSClientConfig();
config.RemoteIPHost = new IPHost("127.0.0.1:7789");
myWSClient.Setup(config);
myWSClient.Connect();
Console.WriteLine("连接成功");
while (true)
{
    myWSClient.Send(Console.ReadLine());
}
```

【WS 接收数据（服务器、客户端均可用）】

```
private static void MyWSClient_Received(IWSClientBase client, WSDataFrame dataFrame)
{
    switch (dataFrame.Opcode)
    {
        case WSDataType.Cont:
            Console.WriteLine($"收到中间数据，长度为：{dataFrame.PayloadLength}");
            break;
        case WSDataType.Text:
            Console.WriteLine(dataFrame.GetMessage());
            break;
        case WSDataType.Binary:
            if (dataFrame.FIN)
            {
                Console.WriteLine($"收到二进制数据，长度为：{dataFrame.PayloadLength}");
            }
            else
            {
                Console.WriteLine($"收到未结束的二进制数据，长度为：{dataFrame.PayloadLength}");
            }
            break;
        case WSDataType.Close:
            break;
        case WSDataType.Ping:
            break;
        case WSDataType.Pong:
            break;
        default:
            break;
    }
}

```



## 🧲应用场景模拟
[场景入口](https://gitee.com/RRQM_Home/RRQMBox/wikis/%E5%BA%94%E7%94%A8%E5%9C%BA%E6%99%AF%E6%A8%A1%E6%8B%9F)

***

## 致谢

谢谢大家对我的支持，如果还有其他问题，请加群QQ：234762506讨论。

[![Giteye chart](https://chart.giteye.net/gitee/dotnetchina/RRQMSocket/RJ9NH249.png)](https://giteye.net/chart/RJ9NH249)

## 后续开发计划

1. FTP功能开发

## 支持作者

[支持入口](https://gitee.com/RRQM_Home/RRQMBox/wikis/%E6%94%AF%E6%8C%81%E4%BD%9C%E8%80%85)
