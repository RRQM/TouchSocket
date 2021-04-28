<p></p>
<p></p>
<p align="center">
<img src="https://img-blog.csdnimg.cn/20210406140816743.png" width = "100" height = "100" alt="图片名称" align=center />
</p>

 <div align="center"> 
  
[![NuGet version (RRQMSocket)](https://img.shields.io/nuget/v/RRQMSocket.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSocket/)
[![License](https://img.shields.io/badge/license-Apache%202-4EB1BA.svg)](https://www.apache.org/licenses/LICENSE-2.0.html)
[![Download](https://img.shields.io/nuget/dt/RRQMSocket)](https://www.nuget.org/packages/RRQMSocket/)
[![star](https://gitee.com/dotnetchina/RRQMSocket/badge/star.svg?theme=gvp)](https://gitee.com/dotnetchina/RRQMSocket/stargazers) 
[![fork](https://gitee.com/dotnetchina/RRQMSocket/badge/fork.svg?theme=gvp)](https://gitee.com/dotnetchina/RRQMSocket/members)

</div> 

<div align="center">

日月之行，若出其中；星汉灿烂，若出其里。

</div>

<div align="center">

**English | [简体中文](./README.zh-CN.md)**

</div>

## 💿描述
&emsp;&emsp;Rrqmsocket is an integrated, ultra-lightweight network communication service framework。it has **High merging** 、 **High concurrent processing** 、 **Event subscription** 、 **Plug-in extension** 、 **Multithreading** 、 **Memory pool** 、 **Object pool** Features，Let the user more simple, fast build network framework。

## 🖥Support environment
- .NET Framework4.5 and above.
- .NET Core3.1 and above.
- .NET Standard 2.0 and above.

## 🥪Support framework
- WPF
- Winform
- Blazor
- Xamarin
- Mono
- Unity
- Others (ie all C # pedics)

## 🌴RRQMSocket Features Quick

 **Object pool** 

The object pool has a lot of applications in rrqmsocket, the most important two is **Connect the target pool** and **processing object pool**. The connection target pool is when the client links, first goes to the TCPSocketClient in the Connection Object Pool, and then no words will be created. If Which client is off, its TCPSocketClient will be reclaimed. This is why **id reuses**。

Then it is to deal with the object pool. In Rrqmsocket, the thread and IOCP kernel thread to receive data are separated, that is, the client sends 1W data to the server, but the server is processed and slow, the traditional IOCP Will definitely slow down the rate, then inform the client's TCP window, congestion, then let the client suspended. However, the RRQM will save all the data, first does not affect the reception of IOCP, and then allocate multithreading to process the message information, which is equivalent to a "ventilation lake", but the proportion of memory Rise。

 **Traditional IOCP and RRQMSocket** 

Rrqmsocket's IOCP and tradition are different. Take Microsoft's official as an example. It has opened up a memory, then split, and then assigns a zone to receive, wait for the data, then copy one, Then throw the data and processes the outside world. Rrqmsocket is before receiving each time, with a memory pool, then directly for receiving, wait for the data, then throwing this memory block directly, so that the replication operation is avoided. Therefore, the efficiency of the file transmission will be high. Of course, this operation is not advantageous when small data.。

**Data processing adapter** 

I believe everyone has used other Socket products, such as HPSocket, Supersocket, etc., Rrqmsocket is also drawing on the excellent design concept of other products, but the data processing adapter is one of them, but the design of other products is, we If you have easy tolerance, you will be loaded, but it is still powerful.。

The first is naming, "Data Process Adapter" means **pre-processing**, which includes **send** and **reception** two parts, its function is strongly unpaired. For example, when we handle TCP adhesive, we have three kinds of regular solutions, which are **fixed baotou**, **fixed length**, **termination character segmentation**, then this time data processing The adapter can be bigger.。

## 🔗Contact the author

 - [CSDN blog homepage](https://blog.csdn.net/qq_40374647)
 - [哩 哔 video](https://space.bilibili.com/94253567)
 - [Source code warehouse homepage](https://gitee.com/RRQM_Home) 
 - Communication QQ group：234762506

## ✨API documentation

[RRQMSocket API documentation](https://gitee.com/dotnetchina/RRQMSocket/wikis/pages)

 
## 📦 installation

- [Nuget RRQMSocket](https://www.nuget.org/packages/RRQMSocket/)
- [Microsoft NuGet Installation Tutorial](https://docs.microsoft.com/zh-cn/nuget/quickstart/install-and-use-a-package-in-visual-studio)

## 🍻RRQM products
| Name                                           | Nuget version                                                                                                                              | download                                                                                              | description                                                                                                                                                                                          |
| ---------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [RRQMCore](https://gitee.com/RRQM_OS/RRQMCore) | [![NuGet version (RRQMCore)](https://img.shields.io/nuget/v/RRQMCore.svg?style=flat-square)](https://www.nuget.org/packages/RRQMCore/) | [![Download](https://img.shields.io/nuget/dt/RRQMCore)](https://www.nuget.org/packages/RRQMCore/) | Rrqmcore is a library that provides the basic service function for the RRQM system, which contains: <br> Memory pool, object pool, wait logic pool, AppMessenger, 3DES encryption, <br> XML fast storage, running time measuring machine, file shortcut, high Performance sequencer, <br> Specification log interface, etc. |
| [RRQMMVVM](https://gitee.com/RRQM_OS/RRQMMVVM) | [![NuGet version (RRQMMVVM)](https://img.shields.io/nuget/v/RRQMMVVM.svg?style=flat-square)](https://www.nuget.org/packages/RRQMMVVM/) | [![Download](https://img.shields.io/nuget/dt/RRQMMVVM)](https://www.nuget.org/packages/RRQMMVVM/) | RRQMMVVM is a super-lighting MVVM framework, but the sparrow is small, and the fifty is full.                                                                                                                                          |
| [RRQMSkin](https://gitee.com/RRQM_OS/RRQMSkin) | [![NuGet version (RRQMSkin)](https://img.shields.io/nuget/v/RRQMSkin.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSkin/) | [![Download](https://img.shields.io/nuget/dt/RRQMSkin)](https://www.nuget.org/packages/RRQMSkin/) | Rrqmskin is the WPF control style library, which contains: <br> Bless-Box Form, Rounded Form, Water Corrugated Button, Enter Prompt Filter, Control Drag Effect, <br> Rounded Picture Box, Curved Text, Fan elements, pointer elements, pie chart, clock. Speed ​​dial, etc.               |

## 💐Frame quick view

## First, TCP framework
#### 1.1 Create a TCP service framework
[RRQMSocket Getting Started](https://gitee.com/dotnetchina/RRQMSocket/wikis/2.3%20%E5%88%9B%E5%BB%BA%E3%80%81%E4%BD%BF%E7%94%A8TcpService?sort_id=3897349)
#### 1.2 feature
- It is easy to use.
- Multi-threaded processing.
- IOCP perfect design model to avoid receiving data replication.
- Simple, stable management client connection, automatically verify client activity.
- Ultra-simple solution, bracket issues, see [RRQMSocket to solve TCP adhesive bags, sub-package issues](https://blog.csdn.net/qq_40374647/article/details/110680179?spm=1001.2014.3001.5501)。
- Memory pool design, avoid memory repeated application, release.
- Object pool design, avoid application, release of data objects.
#### 1.3 Demo
[RRQMSocket.Demo](https://gitee.com/RRQM_Home/RRQMSocket.Demo)

## Second, TOKEN TCP Framework
#### 2.1 Overview
The Token system is based on a TCP server that defines a connected server. Its main function is to filter the client that is about to complete. The screen is to verify token. If the Token does not meet the regulations, disconnect, other features and TCP directly The server is consistent.
#### 2.2 Characteristics
- Filter unqualified TCP client.
- Multi-tenant use.
- The client server is unified ID, which is convenient to retrieve.
#### 2.3 Create and use the Token framework
[Create and use the Token framework](https://gitee.com/dotnetchina/RRQMSocket/wikis/3.2%20%E5%88%9B%E5%BB%BA%E3%80%81%E4%BD%BF%E7%94%A8Token%E6%9C%8D%E5%8A%A1%E5%99%A8?sort_id=3896799)
#### 2.4 Demo
[RRQMSocket.Demo](https://gitee.com/RRQM_Home/RRQMSocket.Demo)


## 三、File transfer framework
#### 3.1 Create a file server framework

The following simple example, detailed use [File Transport Getting Started](https://gitee.com/dotnetchina/RRQMSocket/wikis/5.1%20%E6%A6%82%E8%BF%B0?sort_id=3897485)
```
 FileService fileService = new FileService();
 fileService.VerifyToken ="123ABC";
 
 fileService.BreakpointResume = true;//Support breakpoint resume
 try
 {
     fileService.Bind(7789,2);//Listening directly to the 7789 port number. Multi-thread, default is 1, here settings the number of threads 2
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

#### 3.2 Characteristics
- It is easy to use.
- Multi-threaded processing.
- High performance, the transmission speed can reach 500MB / s.
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

#### 3.3 Demo example
 **Demo location:** [RRQMSocket.FileTransfer.Demo](https://gitee.com/RRQM_Home/RRQMSocket.FileTransfer.Demo)

 **Description:** It can be seen that Figure 1 is uploading a Window system image file, approximately 4.2GB, the transfer speed has reached 346MB / s because the server and client are on the same computer, which is caused by disk performance limits. Second, GC basically has no release, the performance is very powerful, Figure 2 is the download document, the performance is still very powerful.

![upload files](https://images.gitee.com/uploads/images/2021/0409/190350_92a2ad36_8553710.png "upload files")
![download file](https://images.gitee.com/uploads/images/2021/0409/190954_a212982d_8553710.png "download file")


## Fourth, RPC framework
#### 4.1 Creating an RPC Service
New class files, inherited to serverProvider, and identify the public method to RRQMRPCMETHOD.
```
public class Server: ServerProvider
{
    [RRQMRPCMethod]
    public string TestOne(string str)
    {
        return "RRQM";
    }
 }
```
#### 4.2 Start RPC service

[Start RPC Service Description](https://gitee.com/dotnetchina/RRQMSocket/wikis/6.3%20%E5%88%9B%E5%BB%BA%E3%80%81%E5%90%AF%E5%8A%A8RPC%E6%9C%8D%E5%8A%A1%E5%99%A8?sort_id=3904370)

#### 4.3 Characteristics
- Simple and easy.
- Multi-threaded processing.
- High performance, when the delivery but does not return, 10W times is 0.8s, and in the case of returning, it is used in 3.9 s.
- Support for different protocol calls such as TCP, UDP.
- Support the designated service asynchronous execution.
- Support permission management, let illegal calls die in the germination period.
- Fully automatic ** code generation **, you can use the system to compile into a DLL call, you can also use the plug-in to generate a proxy call.
- Agent method generates asynchronous methods to support client asynchronous calls.
- ** Supports OUT, REF **, parameter setting defaults, etc.
- Sequence in the heart, in addition to their own [ultra-lightweight binary serialization](https://blog.csdn.net/qq_40374647/article/details/114178244?spm=1001.2014.3001.5501)、Outside XML serialization, users can use other serialization themselves.
- Support compiling calls, also support method name + parameter call.
- **Full exception feedback** ，The exception that occurs in the service, will be referred to in the word to the client.
- Super simple, free ** callback mode **.

#### 4.3 Demo example
 **Demo location:** [RRQMSocket.RPC.Demo](https://gitee.com/RRQM_Home/RRQMSocket.RPC.Demo)

 **Description:** 
Figure 1, Figure 2, Figure 3 respectively `udp no feedback call`,` TCP has feedback calls`, `TCP connection pool has feedback calls. The number of calls is 10W, and the call performance is very Nice. In no feedback, throughput is 14.28W, in the feedback of 2.72W, simply spike WCF (WCF uses HTTP protocol, in this machine test throughput 310)

![输入图片说明](https://images.gitee.com/uploads/images/2021/0409/191343_e5827d04_8553710.png "屏幕截图.png")

![输入图片说明](https://images.gitee.com/uploads/images/2021/0409/191501_abec9e45_8553710.png "屏幕截图.png")

![输入图片说明](https://images.gitee.com/uploads/images/2021/0409/191531_d7f0a8d4_8553710.png "屏幕截图.png")


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

