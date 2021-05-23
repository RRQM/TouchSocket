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

**English | [简体中文](./README.zh-CN.md)**

</div>

## 💿Description
|Name|Version（Nuget Version）|Download（Nuget Download）|Description|
|:---:|---|---|---|
|RRQMSocket| [![NuGet version (RRQMSocket)](https://img.shields.io/nuget/v/RRQMSocket.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSocket/)                                        | [![Download](https://img.shields.io/nuget/dt/RRQMSocket)](https://www.nuget.org/packages/RRQMSocket/) | **RRQMSocket** is an integrated, ultra-lightweight network communication service framework. It has **high concurrency connection**, **high concurrency processing**, **event subscription**, **plug-in extension**, **multi-thread processing**, **memory pool**, **Target pool**, etc., allowing users to make more simple, fast build network frames. In the transmission efficiency, the synchronous transmission can reach **20W/s**, asynchronous sends up to **60w/s**. The server is receiving and processing efficiency is determined by the number of **threads**.|
| RRQMSocket.FileTransfer | [![NuGet version (RRQMSocket.FileTransfer)](https://img.shields.io/nuget/v/RRQMSocket.FileTransfer.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSocket.FileTransfer/) | [![Download](https://img.shields.io/nuget/dt/RRQMSocket.FileTransfer)](https://www.nuget.org/packages/RRQMSocket.FileTransfer/) | RRQMSocket.FileTransfer is a high-performance file transfer framework, you can use it to transfer **arbitrary size** file, it can be perfectly supported **upload download mix queue transmission**, **breakpoint resume**, **Quick upload**, **Transmission speed**, **get file information**, **delete file**, etc. In real-time testing, its transmission rate can reach 500MB / s. |
|RRQMSocket.RPC | [![NuGet version (RRQMSocket.RPC)](https://img.shields.io/nuget/v/RRQMSocket.RPC.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSocket.RPC/)                            | [![Download](https://img.shields.io/nuget/dt/RRQMSocket.RPC)](https://www.nuget.org/packages/RRQMSocket.RPC/) | RRQMSocket.rpc is a superblight, high performance, scalable micro service management platform framework, currently completed the development of **RRQMRPC**, **XmlRpc**, **JsonRpc**, **WebApi** section. **RRQMRPC** section uses the RRQM exclusive protocol, supports client **asynchronous call**, server **asynchronous trigger**, and **out** and **ref** keyword, **function callback**Wait. It is also very powerful in calling efficiency. When the no-load function is called, and when the state is returned, **10W** times is only used only when only **3.8** seconds, does not return the status **0.9** seconds. Other protocol call performance details performance evaluation.|
|RRQMSocket.RPC.WebApi | [![NuGet version (RRQMSocket.RPC.WebApi)](https://img.shields.io/nuget/v/RRQMSocket.RPC.WebApi.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSocket.RPC.WebApi/)| [![Download](https://img.shields.io/nuget/dt/RRQMSocket.RPC.WebApi)](https://www.nuget.org/packages/RRQMSocket.RPC.WebApi/) | RRQMSocket.rpc.Webapi is a WebAPI component extends on RRQMSocket.rpc. You can create a WebAPI service parser through this component, allowing the desktop, web end, and mobile ports to call RPC functions across language. Function support routing, GET MET, POST pass|
|RRQMSocket.RPC.XmlRpc | [![NuGet version (RRQMSocket.RPC.XmlRpc)](https://img.shields.io/nuget/v/RRQMSocket.RPC.XmlRpc.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSocket.RPC.XmlRpc/)| [![Download](https://img.shields.io/nuget/dt/RRQMSocket.RPC.XmlRpc)](https://www.nuget.org/packages/RRQMSocket.RPC.XmlRpc/) | RRQMSocket.RPC.XmlRpc is an XMLRPC component extends in RRQMSocket.RPC, which can create an XMLRPC service parser through this component, perfectly supporting XMLRPC data types, type nested, array, etc., can also be perfectly docked with CookComputing.xmlrpcv2. Not limited to Web, Android and other platforms.|
|RRQMSocket.RPC.JsonRpc | [![NuGet version (RRQMSocket.RPC.JsonRpc)](https://img.shields.io/nuget/v/RRQMSocket.RPC.JsonRpc.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSocket.RPC.JsonRpc/)| [![Download](https://img.shields.io/nuget/dt/RRQMSocket.RPC.JsonRpc)](https://www.nuget.org/packages/RRQMSocket.RPC.JsonRpc/) | RRQMSocket.RPC.jsonRPC is an JSONRPC component extends to rrqmsocket.rpc, which can create a JSONRPC service parser through this component, support all of the JSONRPC, which can be seamlessly docked with web, Android.|
| RRQMSocket.Http | [![NuGet version (RRQMSocket.Http)](https://img.shields.io/nuget/v/RRQMSocket.Http.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSocket.Http/)                         | [![Download](https://img.shields.io/nuget/dt/RRQMSocket.Http)](https://www.nuget.org/packages/RRQMSocket.Http/) | RRQMSocket.http is a service component that can simply parse HTTP and can quickly respond to HTTP service requests.|

## 🖥Support environment
- .NET Framework4.5 and above.
- .NET Core3.1 and above.
- .net Standard 2.0 and above.d above.

## 🥪Support framework
- WPF
- Winform
- Blazor
- Xamarin
- Mono
- Unity
- Other（All C# pedics）

## 🌴RRQMSocket Features Quick
#### Object pool

The object pool has a lot of applications in rrqmsocket, the most important two is **Connect the target pool** and **processing object pool**. The connection target pool is when the client is successfully connected, first go to the Connection Object Pool to find TCPSocketClient, and then no words will be created. If Which client is off, its TCPSocketClient will be reclaimed. This is why ID is reused.

Then it is to deal with the object pool. In Rrqmsocket, the thread and IOCP kernel thread to receive data are separated, that is, the client sends 1W data to the server, but the server is processed and slow, the traditional IOCP Will definitely slow down the rate, then inform the client's TCP window, congestion, then let the client suspended. However, in the rqmsocket, the received data is all saved through the queue. It does not affect the reception of the IOCP, and then the thread will process the received message information, which is equivalent to a "ventilation lake", which can be large Improve the ability to process data.

#### Multithreading

Due to the existence of **processing object pool**, make multi-threading process simple. When the client connection is complete, the message processing logic thread of the client auxocketClient is automatically assigned, if the number of server threads is 10, the first connection client is assigned to the line, the second A connection will be assigned to the 1st, push, loop allocation. When a client receives data, the data is discharged into the queue owned by the current thread and wakes up the thread execution.

#### Traditional IOCP and RRQMSocket

Rrqmsocket's IOCP and tradition are different. Take Microsoft's official as an example, use MemoryBuffer to open a memory, then split, then assign each session to receive a zone, wait after receiving data, copy one, then copy copy Data throwing process. Rrqmsocket is before receiving, with a memory pool, then directly for receiving, after receiving the data, then throwing this memory block directly, so avoid **copy operation** Although it is only a small design, the performance varies from 10 times when transmitting **1000w** **64kb**. So, based on this, the efficiency of the file transmission will be high.

#### Data processing adapter

I believe that everyone has used other Socket products, such as HPSocket, Supersocket, etc., Rrqmsocket is also drawing on the excellent design concept of other products, and the data processing adapter is one of them, but the design of other products is Rrqmsocket. The adapter function is more powerful, it can ignore the real data, and simulate the desired data, for example, you can preprocess data to resolve the data score. The problem of adhesive bags can also be parsed directly to the HTTP protocol, and pass back an HttpRequest object after the adapter process.

#### Bonded bag, subcontract solution

Handling TCP adhesive bags in Rrqmsocket, the subcontracting problem is very simple. Just change different **data processing adapters**. For example: Using **fixed cladding**, only need to assign the TCPSocketClient and TCPClient assignment **fixedHeaderDataHandLingAdapter**. The same corresponding processor also has **fixed length**, **termination character segmentation**, etc.

## 🔗Contact the author

 - [CSDN blog homepage](https://blog.csdn.net/qq_40374647)
 - [哩 哔 video](https://space.bilibili.com/94253567)
 - [Source code warehouse homepage](https://gitee.com/RRQM_Home) 
 - Communication QQ group：234762506

## ✨API Documentation

[RRQMSocket API Documentation](https://gitee.com/RRQM_OS/RRQM/wikis/pages)

 
## 📦 Installation

- [Nuget RRQMSocket](https://www.nuget.org/packages/RRQMSocket/)
- [Microsoft NuGet Installation Tutorial](https://docs.microsoft.com/zh-cn/nuget/quickstart/install-and-use-a-package-in-visual-studio)

## 🍻RRQM Products
| Name| Version（Nuget Version）|Download（Nuget Download）| Description |
|------|----------|-------------|-------|
| [RRQMCore](https://gitee.com/RRQM_OS/RRQMCore) | [![NuGet version (RRQMCore)](https://img.shields.io/nuget/v/RRQMCore.svg?style=flat-square)](https://www.nuget.org/packages/RRQMCore/) | [![Download](https://img.shields.io/nuget/dt/RRQMCore)](https://www.nuget.org/packages/RRQMCore/) | RRQMCore is a library that provides the basic service function for the RRQM, including: **memory pool**, **object pool**, **waiting logic pool**, **AppMessenger**, **3DES encryption**, **XML Quick Storage**, **Runtime Meter**, **File Shortcut Operation**, **High Performance Series**, **Specifies Log Interface**, etc. |
| [RRQMMVVM](https://gitee.com/RRQM_OS/RRQMMVVM) | [![NuGet version (RRQMMVVM)](https://img.shields.io/nuget/v/RRQMMVVM.svg?style=flat-square)](https://www.nuget.org/packages/RRQMMVVM/) | [![Download](https://img.shields.io/nuget/dt/RRQMMVVM)](https://www.nuget.org/packages/RRQMMVVM/) | RRQMMVVM is a super-lighting MVVM framework, but the sparrow is small, and the fifty is full.|
| [RRQMSkin](https://gitee.com/RRQM_OS/RRQMSkin) | [![NuGet version (RRQMSkin)](https://img.shields.io/nuget/v/RRQMSkin.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSkin/) | [![Download](https://img.shields.io/nuget/dt/RRQMSkin)](https://www.nuget.org/packages/RRQMSkin/) | RRQMSkin is the control style library of WPF, which contains: **Borderless form**, **rounded form**, **water ripple button**, **input prompt filter box**, **control drag Move effect**, **rounded picture box**, **arc text**, **fan element**, **pointer element**, **pie chart**, **clock**, **Speed ​​dial**, etc.|  

## 💐Get started

## First, TCP framework
### 1.1 Creating a server
For details, please read [API Document](https://gitee.com/rrqm_os/rrqm/wikis/pages), just simply examples.

**Create RRQMTCPService**

RRQMTCPService is a simple package for TCPService, specifying a secondary class RRQMSocketClient, do not do any data processing in the secondary class, only the data is thrown in the RRQMTCPService.

```CSharp
RRQMTcpService service = new RRQMTcpService();

//Subscribe event
//service.ClientConnected += Service_ClientConnected;//Subscribe to the connection event
//service.ClientDisconnected += Service_ClientDisconnected;//Subscribe to disconnection
//service.CreatSocketCliect += Service_CreatSocketCliect;//Subscribe to create a secondary event, you can set other properties directly.
//service.OnReceived += Service_OnReceived;//You can directly subscribe to the received data event.

//Property setting
service.IsCheckClientAlive = true;//Using empty package test activity, there is no impact on the data.
service.BufferLength = 1024;//Set the cache pool size, which is often used in the framework for application byteblock, so this value affects the efficiency of the memory pool.
service.IDFormat = "TcpSocketClient_{0}";//Set the format of the assigned ID, the format must match the string format, at least one reset, the initial value is "{0} -tcp"
service.Logger = new Log();//Set the internal log recorder, the default log is the console output.
service.MaxCount = 1000;//Set the maximum number of connections, which can be dynamically set, and the client will be disconnected when the received number exceeds the set value.

//method
service.Bind(7789, 2);//Binding listening, bind IPv6, can listen all the addresses.

Console.WriteLine("RRQMTcpService Binding success");

```


**Create tokencpservice**

Tokentcpservice is a function extension server inherited in TCPService，The main function of the server is to filter the connected client by verifying the "password"，Timely will not allow contact, **malicious connection**, **unsafe connection** client refused to do, **tenant mode**。

```CSharp
TokenTcpService<MyTcpSocketClient> service = new TokenTcpService<MyTcpSocketClient>();
service.VerifyToken = "ABC";

//method
service.Bind(7789, 2);//Binding listening, bind IPv6, can listen all the addresses.

Console.WriteLine("TokenTcpService Binding success");

```

**note：** Using this server must follow the connection protocol or use the exclusive client (TOKENTCPCLIENT).

### 1.2 Create a client

**Create TCPCLIENT**

TCPClient can connect, send and receive, process data with any server, or more convenient to process adhesive bags and subcontracts, or analyze the data structure.

```CSharp
TcpClient client = new TcpClient();

//Attributes
client.BufferLength = 1024;//Set the cache pool size, which is often used in the framework for application byteblock, so this value affects the efficiency of the memory pool.
client.Logger = new Log();//Set the internal log recorder, the default log is the console output.
client.DataHandlingAdapter = new NormalDataHandlingAdapter();//Data processing adapters can be used to process adhesive bags, parse objects.

//event
//client.ConnectedService += Client_ConnectedService;
//client.DisconnectedService += Client_DisconnectedService;
//client.OnReceived += Client_OnReceived;

//method
client.Connect(new IPHost("127.0.0.1:7789"));//connection
Console.WriteLine("connection succeeded");
client.Send(Encoding.UTF8.GetBytes("若汝棋茗"));//send data
Console.WriteLine("Sent successfully");

```

**Create tokencpclient**

```CSharp
TokenTcpClient client = new TokenTcpClient();

//Attributes
client.VerifyToken="ABC";//Set the link password.
client.BufferLength = 1024;//Set the cache pool size, which is often used in the framework for application byteblock, so this value affects the efficiency of the memory pool.
client.Logger = new Log();//Set the internal log recorder, the default log is the console output.
client.DataHandlingAdapter = new NormalDataHandlingAdapter();//Data processing adapters can be used to process adhesive bags, parse objects.

//event
//client.ConnectedService += Client_ConnectedService;
//client.DisconnectedService += Client_DisconnectedService;
//client.OnReceived += Client_OnReceived;

//method
client.Connect(new IPHost("127.0.0.1:7789"));//connection
Console.WriteLine("connection succeeded");
client.Send(Encoding.UTF8.GetBytes("若汝棋茗"));//send data
Console.WriteLine("Sent successfully");

```
### 1.3 Data processing adapter

The main role of the data processing adapter is to encapsulate and analyze the data transmitted, received. In Rrqmsocket, you can use the data processing adapter to solve the **adhesive**, **sub-package** problem, or **resolve http** data packets.

**Types of**

- **NormalDataHandlingAdapter**：Ordinary TCP packet processor
- **FixedSizeDataHandlingAdapter**：Fixed length TCP packet processor
- **TerminatorDataHandlingAdapter**：Termination Character TCP Packet Processor
- **FixedHeaderDataHandlingAdapter**：Fixed head TCP packet processor
- **HttpDataHandlingAdapter**：Analyze HTTP processor (need to install RRQMSocket.http)

**Client use**

The client is relatively simple and assigns it directly.

```CSharp
client.DataHandlingAdapter = new NormalDataHandlingAdapter();//Data processing adapters can be used to process adhesive bags, parse objects.
```

**Server use**

When using an adapter, you must guarantee that each **tcpsocketclient** has a **separate instance** adapter, so you can subscribe to the **creatsocketcliect** event, and then because there is a connection target pool in rrqmsocket, it is best Create a new creation of the new creation, then create instantiation, avoiding the performance issues caused by multiple instantiation assignments.


```CSharp
 private static void Service_CreatSocketCliect(RRQMSocketClient arg1, CreatOption arg2)
 {
     if (arg2.NewCreate)
     {
         arg1.DataHandlingAdapter = new NormalDataHandlingAdapter();
     }
 }

```
If it is a TCPSocketClient of **custom inheritance**, you can rewrite the **Create** method.

**Note: The assignment adapter in the constructor is invalid, it will be overwritten by the Create method.**

```CSharp
public class MyTcpSocketClient : TcpSocketClient
{
    /// <summary>
    /// The initial creation of objects is equivalent to constructing, but the time to call the timing after constructor, the parent class method can be overwritten.
    /// </summary>
    public override void Create()
    {
        this.DataHandlingAdapter = new NormalDataHandlingAdapter();//Ordinary TCP packet processor
    
    }
}

```


#### 1.4 Demo
[RRQMBox](https://gitee.com/RRQM_OS/RRQMBox)


## 二、File transfer framework

### 2.1 Characteristics

- Simple and easy.
- Multi-threaded processing.
- High performance, transmission speed up to 500MB / s.
- Ultra-simple transmission speed limit setting, 1K-10GB stepless adjustment.
- Ultra-simple transmission speed, transmission progress acquisition.
- Suspend, continue, continue to transfer.
- Systematic privilege management allows sensitive files to allow only privatization downloads.
- Send messages at any time to allow clients and server communication.
- Based on event driver, let each step are in progress.
- Visual file stream can realize a filling progress bar like Thunder.
- Super simple breakpoint renewal settings are escorted for large files.
- Unless the breakpoint renewal is set, let the same file, connect the power between the different clients.
- Uploaded files, upload it again, fast uploading.
- Extreme GC release.

### 2.2 Create a file server

The following simple example, detailed use [File Transfer Getting Started](https://gitee.com/rrqm_os/rrqm/wikis/pages)

```Csharp
 FileService fileService = new FileService();
 fileService.VerifyToken ="123ABC";
 
 fileService.BreakpointResume = true;//Support breakpoint resume
 try
 {
     fileService.Bind(7789,2);//Listening directly to the 7789 port number. Multi-thread, default is 1, here settings the number of threads 2
/* Subscribe to related events
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

### 2.3 Transfer file

First initialize the client.

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
fileClient.Connect(new IPHost("127.0.0.1:7789"));//connect to the server.

```

Call the **RequestTransfer** to transfer files. This method can be **uploaded** , or **download**. The transfer will be discharged into the queue after the request is successful.

**Upload**
```Csharp

//The Restart property can be freely set.
//BreakPointResume can also be freely specified, but it is best to get attributes from FileClient.
UrlFileInfo urlFileInfo = UrlFileInfo.CreatUpload("C:/1.txt", restart: true, breakpointResume: this.fileClient.BreakpointResume);
fileClient.RequestTransfer(urlFileInfo);
```

**download**
```Csharp

//The Restart property can be freely set.
UrlFileInfo urlFileInfo = UrlFileInfo.CreatDownload("C:/1.txt", restart: true);
fileClient.RequestTransfer(urlFileInfo);
```

### 2.4 Attribute acquisition

- **TRANSFERSPEED**: Get Transfer Speed
- **TransferProgress**: Get Transfer Progress
- **TransferfileInfo**: Get the file information being transferred
- **TransferStatus**: Get Transfer Status
- **BREAKPOINTRESUME**: Get not to support breakpoints (this property is synchronized with the server)
- **ReceiveDirectory**: Get or set the reception folder
- **FileTransfercollection**: Get Transfer File Collection

### 2.5 Function method

```Csharp
fileClient.PauseTransfer();//Suspend transmission
fileClient.ResumeTransfer();//Restore transmission

foreach (var item in fileClient.FileTransferCollection)
{
    fileClient.CancelTransfer(item);//Get transfer information from the transfer list, then cancel the transfer task
    break;
}

fileClient.StopThisTransfer();//Stop current download
fileClient.StopAllTransfer();//Stop all downloads
fileClient.SendSystemMessage("RRQM");//Send system message
fileClient.SendBytesWaitReturn(new byte[10],0,10);//An array of bytes and wait for returning

```

#### 2.6 Demo example

 **Demo location：** [RRQMBox](https://gitee.com/RRQM_OS/RRQMBox)

 **Description：** It can be seen that Figure 1 is uploading a Window system image file, approximately 4.2GB, the transfer speed has reached 346MB / s because the server and client are on the same computer, which is caused by disk performance limits. Second, GC basically has no release, the performance is very powerful, Figure 2 is the download document, the performance is still very powerful.

![上传文件](https://images.gitee.com/uploads/images/2021/0409/190350_92a2ad36_8553710.png "上传文件")
![下载文件](https://images.gitee.com/uploads/images/2021/0409/190954_a212982d_8553710.png "下载文件")


## 三、RPC framework

The RPC framework is a micro-service management platform called in all remote processes. Under the hoster of the platform, multiple protocols, multiple serialization modes are possible. Co-calls are currently available using RRQMRPC, WebAPI, XMLRPC, JSONRPC.

### 3.1 RRQMRPC

**Characteristics**
- Simple and easy.
- Multi-threaded processing.
- High performance, when the delivery but does not return, 10W times is 0.8s, and in the case of returning, it is used in 3.9 s.
- Support for different protocol calls such as TCP, UDP.
- Support the designated service asynchronous execution.
- Support permission management, let illegal calls die in the germination period.
- Fully automatic **code generation**, you can use the system to compile into a DLL call, you can also use the plug-in to generate a proxy call.
- Agent method generates asynchronous methods to support client asynchronous calls.
- **Supports OUT, REF**, parameter setting defaults, etc.
- Sequence in the heart, in addition to their own [ultra-lightweight binary serialization](https://blog.csdn.net/qqqqq_40374647/Article/details/114178244?spm=1001.2014.3001.5501), XML serialization Users can use other serializations themselves.
- Support compilation call, also support method name + parameter call.
- **All exception feedback**, the exception occurred in the service, the word is not bad to the client.
- Super simple, free **callback mode**.

#### Create an RRQMRPC server

New class files, inherit it in serverProvider, and will be identified as **rrqmrpcmethod** in **public method**.
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
#### Start the RRQMRPC server

```Csharp
RPCService rpcService = new RPCService();
rpcService.RegistAllService();//Register all services

TcpRPCParser tcpRPCParser = new TcpRPCParser();
tcpRPCParser.SerializeConverter = new BinarySerializeConverter();
tcpRPCParser.Bind(7789, 5);
tcpRPCParser.NameSpace = "RRQMTest";
Console.WriteLine("TCP parser added to complete");

rpcService.AddRPCParser("TcpParser", tcpRPCParser);

rpcService.OpenRPCServer();
Console.WriteLine("RPC starts completion");

Console.ReadKey();

```

#### Client reference

First, you have to download [RRQMRPCVSIX plug-in](https://gitee.com/rrqm_os/rrqmrpcvsix/releses), then install the plugin, right-click  **can see** Re-reference RRQMRPC entry .

<img src="https://i.loli.net/2021/05/18/xkr8caGp6eUql7d.jpg" width = "350" height = "200" alt="图片名称" align=center />

then click，**pop-up window**，Enter **IP and port**, click OK, you can download the completion reference, which will generate the RRQMRPC folder under the project, which contains the proxy file.

<img src="https://i.loli.net/2021/05/18/V5gMDnv9k3etG6U.jpg" width = "300" height = "200" alt="图片名称" align=center />

#### Create a client

```Csharp
RPCClient client = new RPCClient();

client.InitializedRPC(new IPHost("127.0.0.1:7789"));

Server server = new Server(client);

string mes=server.TestOne("RRQM");//Call
```

#### RRQMRPC Performance Test

 **Description：** 
Figure 1, Figure 2, Figure 3 respectively `udp no feedback call`,` TCP has feedback calls`, `TCP connection pool has feedback calls. The number of calls is 10W, and the call performance is very Nice. In no feedback, throughput is 14.28W, in the feedback of 2.72W, simply spike WCF (WCF uses HTTP protocol, in this machine test throughput 310)

![输入图片说明](https://images.gitee.com/uploads/images/2021/0409/191343_e5827d04_8553710.png "屏幕截图.png")

![输入图片说明](https://images.gitee.com/uploads/images/2021/0409/191501_abec9e45_8553710.png "屏幕截图.png")

![输入图片说明](https://images.gitee.com/uploads/images/2021/0409/191531_d7f0a8d4_8553710.png "屏幕截图.png")

#### Example Demo

[RRQMBox](https://gitee.com/RRQM_OS/RRQMBox)


### 3.2 WebApi

WEBAPI features, currently only for API calls, not full of MVC.

#### Characteristics
- Multi-threaded processing.
- High performance, 100 clients, 10W times call, only when only 17s.
- **All exception feedback**, the exception occurred in the service, the word is not bad to the client.
- Support most of the routing rules.

#### Create a WebAPI server

New class files, inherited to **serverProvider**, use the **Rount** property to specify the routing rule, and the **public method** is identified as **route**. It is also possible to make routing rules.

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
#### Start WebAPI Server

```Csharp
RPCService rpcService = new RPCService();
rpcService.RegistAllService();//Register all services

WebApiParser webApiParser = new WebApiParser();
webApiParser.Bind(7792, 5);
Console.WriteLine("WebApiParser parser added to complete");

rpcService.AddRPCParser("webApiParser", webApiParser);

rpcService.OpenRPCServer();
Console.WriteLine("RPC starts completion");

Console.ReadKey();

```

You can use the PostmanAgent test, you can also use your browser to access.

**Note: The default data format is XML, if you need JSON, please explain the documentation details.**

#### Performance Testing

![](https://i.loli.net/2021/05/23/vZj8EnSYA1aehH7.jpg)

### 3.3 XmlRpc

Perfect support for XMLRPC data type, type nested, array, etc.

#### Create an XMLRPC server

New class files, inherited in **serverprovider**, and the **public method** can be identified as **xmlrpc**.

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
#### Start XMLRPC Server

```Csharp
RPCService rpcService = new RPCService();
rpcService.RegistAllService();//Register all services

XmlRpcParser xmlRpcParser = new XmlRpcParser();
xmlRpcParser.Bind(7793, 5);
Console.WriteLine("XMLRPCPARSER parser added to complete");

rpcService.AddRPCParser("xmlRpcParser", xmlRpcParser);

rpcService.OpenRPCServer();
Console.WriteLine("RPC starts completion");

Console.ReadKey();

```

You can use the cookcomputing.xmlrpcv2 test.

### 3.4 JsonRpc

#### Create a JSONRPC server

New class files, inherited in **serverProvider**, and the **public method** is identified as **jsonrpc**.

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
#### Start XMLRPC Server

```Csharp
RPCService rpcService = new RPCService();
rpcService.RegistAllService();//Register all services

JsonRpcParser jsonRpcParser = new JsonRpcParser();
jsonRpcParser.Bind(7793, 5);
Console.WriteLine("JSONRPCPARSER parser added to complete");

rpcService.AddRPCParser("jsonRpcParser", jsonRpcParser);

rpcService.OpenRPCServer();
Console.WriteLine("RPC starts completion");

Console.ReadKey();

```

The RPC can be called using JSON format data.

```Csharp
TcpClient tcpClient = new TcpClient();
//tcpClient.OnReceived += TcpClient_OnReceived;//Receive returns data.
tcpClient.Connect(new IPHost("127.0.0.1:7793"));

tcpClient.Send(Encoding.UTF8.GetBytes("{\"jsonrpc\":\"2.0\",\"method\":\"TestOne\",\"params\":[5],\"id\":1}\r\n"));// "\r\n" must be included here.


```


## Thank you

Thank you for your support, if there are other problems, please add group QQ: 234762506.


## 💕 Support this project
Your support is the driving force for my unremitting efforts. Please leave your name when you reward.

 **Sponsorship total amount: 366.6 ¥** 

**Sponsored list:** 

(The following ranking is only in the order of rewards)

> 1.Bobo Joker

> 2.UnitySir

> 3.Coffee

<img src="https://images.gitee.com/uploads/images/2021/0330/234046_7662fb8c_8553710.png" width = "600" height = "400" alt="图片名称" align=center />

