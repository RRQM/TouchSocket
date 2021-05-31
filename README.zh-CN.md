<p></p>
<p></p>
<p align="center">
<img src="https://i.loli.net/2021/05/07/vJy17TEMkZgrsxo.png" width = "100" height = "100" alt="图片名称" align=center />
</p>

 <div align="center"> 
  
[![NuGet version (RRQMSocket)](https://img.shields.io/nuget/v/RRQMSocket.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSocket/)
[![License](https://img.shields.io/badge/license-Apache%202-4EB1BA.svg)](https://www.apache.org/licenses/LICENSE-2.0.html)
[![Download](https://img.shields.io/nuget/dt/RRQMSocket)](https://www.nuget.org/packages/RRQMSocket/)
[![star](https://gitee.com/dotnetchina/RRQMSocket/badge/star.svg?theme=gvp)](https://gitee.com/dotnetchina/RRQMSocket/stargazers) 
[![fork](https://gitee.com/dotnetchina/RRQMSocket/badge/fork.svg?theme=gvp)](https://gitee.com/dotnetchina/RRQMSocket/members)

</div> 

<div align="center">

合抱之木，生于毫末；九层之台，起于垒土。

</div>
<div align="center">

**简体中文 | [English](./README.md)**

</div>

## 💿描述
|名称|版本（Nuget Version）|下载（Nuget Download）|描述|
|:---:|---|---|---|
|RRQMSocket| [![NuGet version (RRQMSocket)](https://img.shields.io/nuget/v/RRQMSocket.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSocket/)                                        | [![Download](https://img.shields.io/nuget/dt/RRQMSocket)](https://www.nuget.org/packages/RRQMSocket/) | **RRQMSocket**是一个整合性的、超轻量级的网络通信服务框架。它具有 **高并发连接** 、 **高并发处理** 、 **事件订阅** 、 **插件式扩展** 、 **多线程处理** 、 **内存池** 、 **对象池** 等特点，让使用者能够更加简单的、快速的搭建网络框架。在发送效率上，同步发送可达**20w/s**，异步发送可达**60w/s**。服务器在接收、处理效率上因**线程数量**而定。|
| RRQMSocket.FileTransfer | [![NuGet version (RRQMSocket.FileTransfer)](https://img.shields.io/nuget/v/RRQMSocket.FileTransfer.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSocket.FileTransfer/) | [![Download](https://img.shields.io/nuget/dt/RRQMSocket.FileTransfer)](https://www.nuget.org/packages/RRQMSocket.FileTransfer/) | RRQMSocket.FileTransfer是一个高性能的文件传输框架，您可以用它传输**任意大小**的文件，它可以完美支持**上传下载混合式队列传输**、**断点续传**、 **快速上传** 、**传输限速**、**获取文件信息**、**删除文件**等。在实时测试中，它的传输速率可达500Mb/s。 |
|RRQMSocket.RPC | [![NuGet version (RRQMSocket.RPC)](https://img.shields.io/nuget/v/RRQMSocket.RPC.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSocket.RPC/)                            | [![Download](https://img.shields.io/nuget/dt/RRQMSocket.RPC)](https://www.nuget.org/packages/RRQMSocket.RPC/) | RRQMSocket.RPC是一个超轻量、高性能、可扩展的微服务管理平台框架，目前已完成开发**RRQMRPC**、**XmlRpc**、**JsonRpc**、**WebApi**部分。**RRQMRPC**部分使用RRQM专属协议，支持客户端**异步调用**，服务端**异步触发**、以及**out**和**ref**关键字，**函数回调**等。在调用效率上也是非常强悍，在调用空载函数，且返回状态时，**10w**次调用仅用时**3.8**秒，不返回状态用时**0.9**秒。其他协议调用性能详看性能评测。|
|RRQMSocket.RPC.WebApi | [![NuGet version (RRQMSocket.RPC.WebApi)](https://img.shields.io/nuget/v/RRQMSocket.RPC.WebApi.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSocket.RPC.WebApi/)| [![Download](https://img.shields.io/nuget/dt/RRQMSocket.RPC.WebApi)](https://www.nuget.org/packages/RRQMSocket.RPC.WebApi/) | RRQMSocket.RPC.WebApi是一个扩展于RRQMSocket.RPC的WebApi组件，可以通过该组件创建WebApi服务解析器，让桌面端、Web端、移动端可以跨语言调用RPC函数。功能支持路由、Get传参、Post传参等。|
|RRQMSocket.RPC.XmlRpc | [![NuGet version (RRQMSocket.RPC.XmlRpc)](https://img.shields.io/nuget/v/RRQMSocket.RPC.XmlRpc.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSocket.RPC.XmlRpc/)| [![Download](https://img.shields.io/nuget/dt/RRQMSocket.RPC.XmlRpc)](https://www.nuget.org/packages/RRQMSocket.RPC.XmlRpc/) | RRQMSocket.RPC.XmlRpc是一个扩展于RRQMSocket.RPC的XmlRpc组件，可以通过该组件创建XmlRpc服务解析器，完美支持XmlRpc数据类型，类型嵌套，Array等，也能与CookComputing.XmlRpcV2完美对接。不限Web，Android等平台。|
|RRQMSocket.RPC.JsonRpc | [![NuGet version (RRQMSocket.RPC.JsonRpc)](https://img.shields.io/nuget/v/RRQMSocket.RPC.JsonRpc.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSocket.RPC.JsonRpc/)| [![Download](https://img.shields.io/nuget/dt/RRQMSocket.RPC.JsonRpc)](https://www.nuget.org/packages/RRQMSocket.RPC.JsonRpc/) | RRQMSocket.RPC.JsonRpc是一个扩展于RRQMSocket.RPC的JsonRpc组件，可以通过该组件创建JsonRpc服务解析器，支持JsonRpc全部功能，可与Web，Android等平台无缝对接。|
| RRQMSocket.Http | [![NuGet version (RRQMSocket.Http)](https://img.shields.io/nuget/v/RRQMSocket.Http.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSocket.Http/)                         | [![Download](https://img.shields.io/nuget/dt/RRQMSocket.Http)](https://www.nuget.org/packages/RRQMSocket.Http/) | RRQMSocket.Http是一个能够简单解析Http的服务组件，能够快速响应Http服务请求。|

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
- Unity
- 其他（即所有C#系）

## 🌴RRQMSocket特点速览

#### 对象池

对象池在RRQMSocket有很多应用，最主要的两个就是**连接对象池**和**处理对象池**。连接对象池就是当客户端成功连接时，首先会去连接对象池中找TcpSocketClient，然后没有的话，才会创建。如果哪个客户端掉线了，它的TcpSocketClient就会被回收。这也就是**ID重用**的原因。

然后就是处理对象池，在RRQMSocket中，接收数据的线程和IOCP内核线程是分开的，也就是比如说客户端给服务器发送了1w条数据，但是服务器收到后处理起来很慢，那传统的iocp肯定会放慢接收速率，然后通知客户端的tcp窗口，发生拥塞，然后让客户端暂缓发送。但是在RRQMSocket中会把收到的数据通过队列全都存起来，首先不影响iocp的接收，同时再分配线程去处理收到的报文信息，这样就相当于一个“泄洪湖泊”，能很大程度的提高处理数据的能力。

#### 多线程

由于有**处理对象池**的存在，使多线程处理变得简单。在客户端连接完成时，会自动分配该客户端辅助类（TcpSocketClient）的消息处理逻辑线程，假如服务器线程数量为10，则第一个连接的客户端会被分配到0号线程中，第二个连接将被分配到1号线程中，以此类推，循环分配。当某个客户端收到数据时，会将数据排入当前线程所独自拥有的队列当中，并唤醒线程执行。

#### 传统IOCP和RRQMSocket

RRQMSocket的IOCP和传统也不一样的，以微软官方为例，使用MemoryBuffer开辟一块内存，然后均分，然后给每个会话分配一个区接收，等收到数据以后，再复制一份，然后把复制的数据抛出处理。而RRQMSocket是每次接收之前，从内存池拿一个可用内存块，然后直接用于接收，等收到数据以后，直接就把这个内存块抛出去了，这样就避免了**复制操作**，虽然只是细小的设计，但是在传输**1000w**次**64kb**的数据时，性能相差了**10倍**。所以也是基于此，文件传输时效率才会高。

#### 数据处理适配器

相信大家都使用过其他的Socket产品，例如HPSocket，SuperSocket等，那么RRQMSocket在设计时也是借鉴了其他产品的优秀设计理念，数据处理适配器就是其中之一，但和其他产品的设计不同的是，RRQMSocket的适配器功能更加强大，它可以无视真实的数据，而模拟出想要的数据，例如：可以对数据进行预处理，从而解决数据分包。粘包的问题，也可以直接解析HTTP协议，经过适配器处理后传回一个HttpRequest对象等。

#### 粘包、分包解决

在RRQMSocket中处理TCP粘包、分包问题是非常简单的。只需要更改不同的**数据处理适配器**即可。例如：使用**固定包头**，只需要给TcpSocketClient和TcpClient赋值**FixedHeaderDataHandlingAdapter**的实例即可。同样对应的处理器也有**固定长度** 、 **终止字符分割** 等。

## 🔗联系作者

 - [CSDN博客主页](https://blog.csdn.net/qq_40374647)
 - [哔哩哔哩视频](https://space.bilibili.com/94253567)
 - [源代码仓库主页](https://gitee.com/RRQM_Home) 
 - 交流QQ群：234762506

## ✨API文档

[RRQMSocket API文档](https://gitee.com/RRQM_OS/RRQM/wikis/pages)

 
## 📦 安装

- [Nuget RRQMSocket](https://www.nuget.org/packages/RRQMSocket/)
- [微软Nuget安装教程](https://docs.microsoft.com/zh-cn/nuget/quickstart/install-and-use-a-package-in-visual-studio)

## 🍻RRQM系产品
| 名称| 版本（Nuget Version）|下载（Nuget Download）| 描述 |
|------|----------|-------------|-------|
| [RRQMCore](https://gitee.com/RRQM_OS/RRQMCore) | [![NuGet version (RRQMCore)](https://img.shields.io/nuget/v/RRQMCore.svg?style=flat-square)](https://www.nuget.org/packages/RRQMCore/) | [![Download](https://img.shields.io/nuget/dt/RRQMCore)](https://www.nuget.org/packages/RRQMCore/) | RRQMCore是为RRQM系提供基础服务功能的库，其中包含：**内存池**、**对象池**、**等待逻辑池**、**AppMessenger**、**3DES加密**、**Xml快速存储**、**运行时间测量器**、**文件快捷操作**、**高性能序列化器**、**规范日志接口**等。 |
| [RRQMMVVM](https://gitee.com/RRQM_OS/RRQMMVVM) | [![NuGet version (RRQMMVVM)](https://img.shields.io/nuget/v/RRQMMVVM.svg?style=flat-square)](https://www.nuget.org/packages/RRQMMVVM/) | [![Download](https://img.shields.io/nuget/dt/RRQMMVVM)](https://www.nuget.org/packages/RRQMMVVM/) | RRQMMVVM是超轻简的MVVM框架，但是麻雀虽小，五脏俱全。|
| [RRQMSkin](https://gitee.com/RRQM_OS/RRQMSkin) | [![NuGet version (RRQMSkin)](https://img.shields.io/nuget/v/RRQMSkin.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSkin/) | [![Download](https://img.shields.io/nuget/dt/RRQMSkin)](https://www.nuget.org/packages/RRQMSkin/) | RRQMSkin是WPF的控件样式库，其中包含： **无边框窗体** 、 **圆角窗体** 、 **水波纹按钮** 、 **输入提示筛选框** 、 **控件拖动效果** 、**圆角图片框**、 **弧形文字** 、 **扇形元素** 、 **指针元素** 、 **饼图** 、 **时钟** 、 **速度表盘** 等。|  

## 💐快速入门

## 一、TCP框架
### 1.1 创建服务器
详细过程请阅读[API文档](https://gitee.com/RRQM_OS/RRQM/wikis/pages)，以下仅简单示例。

**创建RRQMTcpService**

RRQMTcpService是对TcpService的简单封装，指定辅助类为RRQMSocketClient，在辅助类中不做任何数据处理，仅将数据在RRQMTcpService中抛出。

```CSharp
RRQMTcpService service = new RRQMTcpService();

//订阅事件
//service.ClientConnected += Service_ClientConnected;//订阅连接事件
//service.ClientDisconnected += Service_ClientDisconnected;//订阅断开连接事件
//service.CreatSocketCliect += Service_CreatSocketCliect;//订阅创建辅助类事件，可直接设置其他属性。
//service.OnReceived += Service_OnReceived;//可直接订阅收到数据事件。

//属性设置
service.IsCheckClientAlive = true;//使用空包检验活性，不会对数据有任何影响。
service.BufferLength = 1024;//设置缓存池大小，该数值在框架中经常用于申请ByteBlock，所以该值会影响内存池效率。
service.IDFormat = "TcpSocketClient_{0}";//设置分配ID的格式， 格式必须符合字符串格式，至少包含一个补位， 初始值为“{0}-TCP”
service.Logger = new Log();//设置内部日志记录器，默认日志是控制台输出。
service.MaxCount = 1000;//设置最大连接数，可动态设置，当已连接数超过设置数值时，将主动断开客户端。

//方法
service.Bind(7789, 2);//绑定监听，可绑定Ipv6，可监听所有地址。

Console.WriteLine("RRQMTcpService绑定成功");

```


**创建TokenTcpService**

TokenTcpService是继承于TcpService的功能扩展服务器，该服务器的主要功能是通过验证“口令”对连接的客户端进行筛选，及时的将**不允许连接**、**恶意连接**、**不安全连接**的客户端拒之门外，也可以实现**租户模式**。

```CSharp
TokenTcpService<MyTcpSocketClient> service = new TokenTcpService<MyTcpSocketClient>();
service.VerifyToken = "ABC";

//方法
service.Bind(7789, 2);//绑定监听，可绑定Ipv6，可监听所有地址。

Console.WriteLine("TokenTcpService绑定成功");

```

**注意：** 使用该服务器必须遵循连接协议，或使用专属客户端（TokenTcpClient）连接。

### 1.2 创建客户端

**创建TcpClient**

TcpClient可与任意服务器进行连接、收发，处理数据，也可以更加方便的处理粘包和分包，亦或者解析数据结构。

```CSharp
TcpClient client = new TcpClient();

//属性
client.BufferLength = 1024;//设置缓存池大小，该数值在框架中经常用于申请ByteBlock，所以该值会影响内存池效率。
client.Logger = new Log();//设置内部日志记录器，默认日志是控制台输出。
client.DataHandlingAdapter = new NormalDataHandlingAdapter();//数据处理适配器，可用于处理粘包、解析对象。

//事件
//client.ConnectedService += Client_ConnectedService;
//client.DisconnectedService += Client_DisconnectedService;
//client.OnReceived += Client_OnReceived;

//方法
client.Connect(new IPHost("127.0.0.1:7789"));//连接
Console.WriteLine("连接成功");
client.Send(Encoding.UTF8.GetBytes("若汝棋茗"));//发送数据
Console.WriteLine("发送成功");

```

**创建TokenTcpClient**

```CSharp
TokenTcpClient client = new TokenTcpClient();

//属性
client.VerifyToken="ABC";//设置链接口令。
client.BufferLength = 1024;//设置缓存池大小，该数值在框架中经常用于申请ByteBlock，所以该值会影响内存池效率。
client.Logger = new Log();//设置内部日志记录器，默认日志是控制台输出。
client.DataHandlingAdapter = new NormalDataHandlingAdapter();//数据处理适配器，可用于处理粘包、解析对象。

//事件
//client.ConnectedService += Client_ConnectedService;
//client.DisconnectedService += Client_DisconnectedService;
//client.OnReceived += Client_OnReceived;

//方法
client.Connect(new IPHost("127.0.0.1:7789"));//连接
Console.WriteLine("连接成功");
client.Send(Encoding.UTF8.GetBytes("若汝棋茗"));//发送数据
Console.WriteLine("发送成功");

```
### 1.3 数据处理适配器

数据处理适配器的主要作用就是对发送、接收的数据进行封装和解析。在RRQMSocket中，可以利用数据处理适配器解决**粘包**、**分包**问题，也能**解析Http**数据报文。

**类型**

- **NormalDataHandlingAdapter**普通TCP报文处理器
- **FixedSizeDataHandlingAdapter**固定长度TCP报文处理器
- **TerminatorDataHandlingAdapter**终止字符TCP报文处理器
- **FixedHeaderDataHandlingAdapter**固定包头TCP报文处理器
- **HttpDataHandlingAdapter**解析Http处理器（需安装RRQMSocket.Http）

**客户端使用**

客户端比较简单，直接对其赋值即可。

```CSharp
client.DataHandlingAdapter = new NormalDataHandlingAdapter();//数据处理适配器，可用于处理粘包、解析对象。
```

**服务器使用**

在服务器使用适配器时，必须保证每个**TcpSocketClient**都拥有一个**单独实例**的适配器，所以可以订阅**CreatSocketCliect**事件，然后又因为RRQMSocket中有连接对象池，所以最好进行**新创建判断**，然后再创建实例化，避免多次实例化赋值带来的性能问题。


```CSharp
 private static void Service_CreatSocketCliect(RRQMSocketClient arg1, CreatOption arg2)
 {
     if (arg2.NewCreate)
     {
         arg1.DataHandlingAdapter = new NormalDataHandlingAdapter();
     }
 }

```
如果是**自定义继承**的TcpSocketClient，可以重写**Create**方法。

**注意：在构造函数内赋值适配器无效，会被Create方法覆盖**

```CSharp
public class MyTcpSocketClient : TcpSocketClient
{
    /// <summary>
    /// 初次创建对象，效应相当于构造函数，但是调用时机在构造函数之后，可覆盖父类方法
    /// </summary>
    public override void Create()
    {
        this.DataHandlingAdapter = new NormalDataHandlingAdapter();//普通TCP报文处理器
        //this.DataHandlingAdapter = new FixedHeaderDataHandlingAdapter();//固定包头TCP报文处理器
        //this.DataHandlingAdapter = new FixedSizeDataHandlingAdapter(1024);//固定长度TCP报文处理器
        //this.DataHandlingAdapter = new TerminatorDataHandlingAdapter(1024, "\r\n");//终止字符TCP报文处理器
    }
}

```


#### 1.4 Demo
[RRQMBox](https://gitee.com/RRQM_OS/RRQMBox)


## 二、文件传输框架

### 2.1 特点

- 简单易用。
- 多线程处理。
- 高性能，传输速度可达500Mb/s。
- 超简单的传输限速设置，1k-10Gb 无级调节。
- 超简单的传输速度、传输进度获取。
- 随心所欲的暂停、继续、停止传输。
- 系统化的权限管理，让敏感文件只允许私有化下载。
- 随时发送消息，让客户端和服务器交流不延迟。
- 基于事件驱动，让每一步操作尽在掌握。
- 可视化的文件块流，可以实现像迅雷一样的填充式进度条。
- 超简单的断点续传设置，为大文件传输保驾护航。
- 无状态上传断点续传设置，让同一个文件，在不同客户端之间接力上传。
- 已经上传的文件，再次上传时，可实现快速上传。
- 极少的GC释放。

### 2.2 创建文件服务器

以下进行简单示例，详细使用见[文件传输入门](https://gitee.com/RRQM_OS/RRQM/wikis/pages)

```Csharp
 FileService fileService = new FileService();
 fileService.VerifyToken ="123ABC";
 
 fileService.BreakpointResume = true;//支持断点续传
 try
 {
     fileService.Bind(7789,2);//直接监听7789端口号。多线程，默认为1，此处设置线程数量为2
/* 订阅相关事件
 fileService.ClientConnected += FileService_ClientConnected;
 fileService.ClientDisconnected += FileService_ClientDisconnected;

 fileService.BeforeTransfer += FileService_BeforeTransfer ;
 fileService.FinishedTransfer += FileService_FinishedTransfer ;
 fileService.ReceiveSystemMes += FileService_ReceiveSystemMes;
*/
 }
 catch (Exception ex)
 {
     MessageBox.Show(ex.Message);
 }
```

### 2.3 传输文件

先初始化客户端。

```Csharp

FileClient fileClient = new FileClient();
//订阅事件
//fileClient.TransferFileError += FileClient_TransferFileError;
//fileClient.BeforeFileTransfer += this.FileClient_BeforeFileTransfer; ;
//fileClient.FinishedFileTransfer += this.FileClient_FinishedFileTransfer; ;
//fileClient.DisconnectedService += FileClient_DisConnectedService;
//fileClient.ReceiveSystemMes += this.FileClient_ReceiveSystemMes;
//fileClient.ConnectedService += this.FileClient_ConnectedService;
//fileClient.FileTransferCollectionChanged += this.FileClient_FileTransferCollectionChanged;
fileClient.Connect(new IPHost("127.0.0.1:7789"));//连接服务器。

```

调用**RequestTransfer**进行传输文件。此方法可**上传**，也可**下载**。请求成功后会将传输排入队列，然后依次传输。

**上传**
```Csharp

//restart属性可以自由设定。
//breakpointResume也可自由指定，但最好是从fileClient获取属性。
UrlFileInfo urlFileInfo = UrlFileInfo.CreatUpload("C:/1.txt", restart: true, breakpointResume: this.fileClient.BreakpointResume);
fileClient.RequestTransfer(urlFileInfo);
```

**下载**
```Csharp

//restart属性可以自由设定。
UrlFileInfo urlFileInfo = UrlFileInfo.CreatDownload("C:/1.txt", restart: true);
fileClient.RequestTransfer(urlFileInfo);
```

### 2.4 属性获取

- **TransferSpeed**：获取传输速度
- **TransferProgress**：获取传输进度
- **TransferFileInfo**：获取正在传输的文件信息
- **TransferStatus**：获取传输状态
- **BreakpointResume**：获取是否支持断点续传（该属性与服务器同步）
- **ReceiveDirectory**：获取或设置接收文件夹
- **FileTransferCollection**：获取传输文件集合

### 2.5 功能方法

```Csharp
fileClient.PauseTransfer();//暂停传输
fileClient.ResumeTransfer();//恢复传输

foreach (var item in fileClient.FileTransferCollection)
{
    fileClient.CancelTransfer(item);//从传输列表中获得传输信息，然后取消该传输任务
    break;
}

fileClient.StopThisTransfer();//停止当前下载
fileClient.StopAllTransfer();//停止所有下载
fileClient.SendSystemMessage("RRQM");//发送系统消息
fileClient.SendBytesWaitReturn(new byte[10],0,10);//发生字节数组并等待返回

```

#### 2.6 Demo示例

 **Demo位置：** [RRQMBox](https://gitee.com/RRQM_OS/RRQMBox)

 **说明：** 可以看到，图一正在上传一个Window的系统镜像文件，大约4.2Gb，传输速度已达到346Mb/s，这是因为服务器和客户端在同一电脑上，磁盘性能限制导致的。其次，GC基本上没有释放，性能非常强悍，图二是下载文件，性能依旧非常强悍。

![上传文件](https://images.gitee.com/uploads/images/2021/0409/190350_92a2ad36_8553710.png "上传文件")
![下载文件](https://images.gitee.com/uploads/images/2021/0409/190954_a212982d_8553710.png "下载文件")


## 三、RPC框架

RPC框架是所有远程过程调用的微服务管理平台，在该平台的托管下，使多种协议、多种序列化方式调用成为可能。目前可使用RRQMRPC、WebApi、XmlRpc、JsonRpc共同调用。

### 3.1 RRQMRPC

**特点**
- 简单易用。
- 多线程处理。
- 高性能，在保证送达但不返回的情况下，10w次调用用时0.8s，在返回的情况下，用时3.9s。
- 支持TCP、UDP等不同的协议调用相同服务。
- 支持指定服务异步执行。
- 支持权限管理，让非法调用死在萌芽时期。
- 全自动 **代码生成** ，可使用系统编译成dll调用，也可以使用插件生成代理调用。
- 代理方法会生成异步方法，支持客户端异步调用。
- **支持out、ref** ，参数设定默认值等。
- 随心所欲的序列化方式，除了自带的[超轻量级二进制序列化](https://blog.csdn.net/qq_40374647/article/details/114178244?spm=1001.2014.3001.5501)、xml序列化外，用户可以自己随意使用其他序列化。
- 支持编译式调用，也支持方法名+参数式调用。
- **全异常反馈** ，服务里发生的异常，会一字不差的反馈到客户端。
- 超简单、自由的**回调方式** 。

#### 创建RRQMRPC服务器

新建类文件，继承于ServerProvider，并将其中**公共方法**标识为**RRQMRPCMethod**即可。
```Csharp
public class Server: ServerProvider
{
    [RRQMRPCMethod]
    public string TestOne(string str)
    {
        return "若汝棋茗";
    }
 }
```
#### 启动RRQMRPC服务器

```Csharp
RPCService rpcService = new RPCService();
rpcService.RegistAllService();//注册所有服务

TcpRPCParser tcpRPCParser = new TcpRPCParser();
tcpRPCParser.SerializeConverter = new BinarySerializeConverter();
tcpRPCParser.Bind(7789, 5);
tcpRPCParser.NameSpace = "RRQMTest";
Console.WriteLine("TCP解析器添加完成");

rpcService.AddRPCParser("TcpParser", tcpRPCParser);

rpcService.OpenRPCServer();
Console.WriteLine("RPC启动完成");

Console.ReadKey();

```

#### 客户端引用

首先得下载[RRQMRPCVSIX插件](https://gitee.com/RRQM_OS/RRQMRPCVSIX/releases)，然后安装插件，成功后右击**任意项目**即可看见“**重新引用RRQMRPC**”条目。

<img src="https://i.loli.net/2021/05/18/xkr8caGp6eUql7d.jpg" width = "350" height = "200" alt="图片名称" align=center />

然后点击，**弹出窗口**，输入**IP及端口**，点击确认，即可**下载完成引用**，此时会在项目下生成RRQMRPC文件夹，里面含有代理文件。

<img src="https://i.loli.net/2021/05/18/V5gMDnv9k3etG6U.jpg" width = "300" height = "200" alt="图片名称" align=center />

#### 创建客户端

```Csharp
RPCClient client = new RPCClient();

client.InitializedRPC(new IPHost("127.0.0.1:7789"));

Server server = new Server(client);

string mes=server.TestOne("RRQM");//调用
```

#### RRQMRPC性能测试

 **说明：** 
图一、图二、图三分别为`UDP无反馈调用`、`TCP有反馈调用`、`TCP连接池有反馈调用`。调用次数均为10w次，调用性能非常nice。在无反馈中，吞吐量达14.28w，在有反馈中达2.72w，简直秒杀WCF（WCF使用http协议，在本机测试吞吐量为310）

![输入图片说明](https://images.gitee.com/uploads/images/2021/0409/191343_e5827d04_8553710.png "屏幕截图.png")

![输入图片说明](https://images.gitee.com/uploads/images/2021/0409/191501_abec9e45_8553710.png "屏幕截图.png")

![输入图片说明](https://images.gitee.com/uploads/images/2021/0409/191531_d7f0a8d4_8553710.png "屏幕截图.png")

#### 示例Demo

[RRQMBox](https://gitee.com/RRQM_OS/RRQMBox)


### 3.2 WebApi

WebApi功能，目前仅仅适用于Api调用，不具备MVC全部功能。

#### 特点
- 多线程处理。
- 高性能，100个客户端，10w次调用，仅用时17s。
- **全异常反馈** ，服务里发生的异常，会一字不差的反馈到客户端。
- 支持大部分路由规则。

#### 创建WebApi服务器

新建类文件，继承于**ServerProvider**，使用**Rount**属性指定路由规则，同时将其中**公共方法**标识为**Route**即可。同时也可制定路由规则。

```Csharp
[Route("/[controller]/[action]")]
public class Server: ServerProvider
{
    [Route]
    public string TestOne(string str)
    {
        return "若汝棋茗";
    }
 }
```
#### 启动WebApi服务器

```Csharp
RPCService rpcService = new RPCService();
rpcService.RegistAllService();//注册所有服务

WebApiParser webApiParser = new WebApiParser();
webApiParser.Bind(7792, 5);
Console.WriteLine("webApiParser解析器添加完成");

rpcService.AddRPCParser("webApiParser", webApiParser);

rpcService.OpenRPCServer();
Console.WriteLine("RPC启动完成");

Console.ReadKey();

```

此时即可使用PostManagent测试，也可以直接使用浏览器访问。

**注意：默认数据格式是xml，如需Json，请详看说明文档。**

#### 性能测试

![](https://i.loli.net/2021/05/23/vZj8EnSYA1aehH7.jpg)

### 3.3 XmlRpc

完美支持XmlRpc数据类型，类型嵌套，Array等。

#### 创建XmlRpc服务器

新建类文件，继承于**ServerProvider**，同时将其中**公共方法**标识为**XmlRpc**即可。

```Csharp
public class Server: ServerProvider
{
    [XmlRpc]
    public string TestOne(string str)
    {
        return "若汝棋茗";
    }
 }
```
#### 启动XmlRpc服务器

```Csharp
RPCService rpcService = new RPCService();
rpcService.RegistAllService();//注册所有服务

XmlRpcParser xmlRpcParser = new XmlRpcParser();
xmlRpcParser.Bind(7793, 5);
Console.WriteLine("xmlRpcParser解析器添加完成");

rpcService.AddRPCParser("xmlRpcParser", xmlRpcParser);

rpcService.OpenRPCServer();
Console.WriteLine("RPC启动完成");

Console.ReadKey();

```

此时即可使用CookComputing.XmlRpcV2测试。

### 3.4 JsonRpc

#### 创建JsonRpc服务器

新建类文件，继承于**ServerProvider**，同时将其中**公共方法**标识为**JsonRpc**即可。

```Csharp
public class Server: ServerProvider
{
    [JsonRpc]
    public string TestOne(string str)
    {
        return "若汝棋茗";
    }
 }
```
#### 启动XmlRpc服务器

```Csharp
RPCService rpcService = new RPCService();
rpcService.RegistAllService();//注册所有服务

JsonRpcParser jsonRpcParser = new JsonRpcParser();
jsonRpcParser.Bind(7793, 5);
Console.WriteLine("jsonRpcParser解析器添加完成");

rpcService.AddRPCParser("jsonRpcParser", jsonRpcParser);

rpcService.OpenRPCServer();
Console.WriteLine("RPC启动完成");

Console.ReadKey();

```

此时即可使用Json格式数据调用RPC。

```Csharp
TcpClient tcpClient = new TcpClient();
//tcpClient.OnReceived += TcpClient_OnReceived;//接收返回数据。
tcpClient.Connect(new IPHost("127.0.0.1:7793"));

tcpClient.Send(Encoding.UTF8.GetBytes("{\"jsonrpc\":\"2.0\",\"method\":\"TestOne\",\"params\":[5],\"id\":1}\r\n"));//此处必须包含“\r\n”。


```


## 致谢

谢谢大家对我的支持，如果还有其他问题，请加群QQ：234762506讨论。


## 💕 支持本项目
您的支持就是我不懈努力的动力。打赏时请一定留下您的称呼。

 **赞助总金额:516.6￥** 

**赞助名单：** 

（以下排名只按照打赏时间顺序）

> 1.Bobo Joker

> 2.UnitySir

> 3.Coffee

> 4.Ninety

> 5.*琼

<img src="https://images.gitee.com/uploads/images/2021/0330/234046_7662fb8c_8553710.png" width = "600" height = "400" alt="图片名称" align=center />

