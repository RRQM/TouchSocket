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

**简体中文 | [English](./README.md)**

</div>

## 💿描述
&emsp;&emsp;RRQMSocket是一个整合性的、超轻量级的网络通信服务框架。它具有 **高并发连接** 、 **高并发处理** 、 **事件订阅** 、 **插件式扩展** 、 **多线程处理** 、 **内存池** 、 **对象池** 等特点，让使用者能够更加简单的、快速的搭建网络框架。

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

 **对象池** 

对象池在RRQMSocket有很多应用，最主要的两个就是**连接对象池**和**处理对象池**。连接对象池就是当客户端链接时，首先会去连接对象池中找TcpSocketClient，然后没有的话，才会创建。如果哪个客户端掉线了，它的TcpSocketClient就会被回收。这也就是**ID重用**的原因。

然后就是处理对象池，在RRQMSocket中，接收数据的线程和IOCP内核线程是分开的，也就是比如说客户端给服务器发送了1w条数据，但是服务器收到后处理起来很慢，那传统的iocp肯定会放慢接收速率，然后通知客户端的tcp窗口，发生拥塞，然后让客户端暂缓发送。但是RRQM会把收到的数据全都存起来，首先不影响iocp的接收，同时再分配多线程去处理收到的报文信息，这样就相当于一个“泄洪湖泊”，但是对内存的占比就会上升。

 **传统IOCP和RRQMSocket** 

RRQMSocket的IOCP和传统也不一样的，就以微软官方为例，它是开辟了一块内存，然后均分，然后给每个会话分配一个区去接收，等收到数据以后，再复制一份，然后把数据抛出去，让外界处理。而RRQMSocket是每次接收之前，从内存池拿一个可用内存块，然后直接用于接收，等收到数据以后，直接就把这个内存块抛出去了，这样就避免了复制操作。所以，文件传输时效率才会高。当然这个操作在小数据时是没什么优势的。

**数据处理适配器** 

相信大家都使用过其他的Socket产品，例如HPSocket，SuperSocket等，那么RRQMSocket在设计时也是借鉴了其他产品的优秀设计理念，数据处理适配器就是其中之一，但和其他产品的设计不同的是，我们删繁就简，轻装上阵，但是依旧功能强大。

首先是命名，“数据处理适配器”的意思就是对数据进行**预处理**，这也就包括**发送**和**接收**两部分，其功能强大程度不言而喻。例如：我们在**处理TCP粘包**时，常规的解决思路有三种，分别为 **固定包头** 、 **固定长度** 、 **终止字符分割** ，那么这时候数据处理适配器就可以大显身手了。

## 🔗联系作者

 - [CSDN博客主页](https://blog.csdn.net/qq_40374647)
 - [哔哩哔哩视频](https://space.bilibili.com/94253567)
 - [源代码仓库主页](https://gitee.com/RRQM_Home) 
 - 交流QQ群：234762506

## ✨API文档

[RRQMSocket API文档](https://gitee.com/dotnetchina/RRQMSocket/wikis/pages)

 
## 📦 安装

- [Nuget RRQMSocket](https://www.nuget.org/packages/RRQMSocket/)
- [微软Nuget安装教程](https://docs.microsoft.com/zh-cn/nuget/quickstart/install-and-use-a-package-in-visual-studio)

## 🍻RRQM系产品
| 名称                                           | Nuget版本                                                                                                                              | 下载                                                                                              | 描述                                                                                                                                                                                          |
|------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [RRQMCore](https://gitee.com/RRQM_OS/RRQMCore) | [![NuGet version (RRQMCore)](https://img.shields.io/nuget/v/RRQMCore.svg?style=flat-square)](https://www.nuget.org/packages/RRQMCore/) | [![Download](https://img.shields.io/nuget/dt/RRQMCore)](https://www.nuget.org/packages/RRQMCore/) | RRQMCore是为RRQM系提供基础服务功能的库，其中包含：<br>内存池、对象池、等待逻辑池、AppMessenger、3DES加密、<br>Xml快速存储、运行时间测量器、文件快捷操作、高性能序列化器、<br>规范日志接口等。 |
| [RRQMMVVM](https://gitee.com/RRQM_OS/RRQMMVVM) | [![NuGet version (RRQMMVVM)](https://img.shields.io/nuget/v/RRQMMVVM.svg?style=flat-square)](https://www.nuget.org/packages/RRQMMVVM/) | [![Download](https://img.shields.io/nuget/dt/RRQMMVVM)](https://www.nuget.org/packages/RRQMMVVM/) | RRQMMVVM是超轻简的MVVM框架，但是麻雀虽小，五脏俱全。                                                                                                                                          |
| [RRQMSkin](https://gitee.com/RRQM_OS/RRQMSkin) | [![NuGet version (RRQMSkin)](https://img.shields.io/nuget/v/RRQMSkin.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSkin/) | [![Download](https://img.shields.io/nuget/dt/RRQMSkin)](https://www.nuget.org/packages/RRQMSkin/) | RRQMSkin是WPF的控件样式库，其中包含：<br>无边框窗体、圆角窗体、水波纹按钮、输入提示筛选框、控件拖动效果、<br>圆角图片框、弧形文字、扇形元素、指针元素、饼图、时钟。速度表盘等。|  

## 💐框架速览

## 一、TCP框架
#### 1.1 创建TCP服务框架
[RRQMSocket入门](https://gitee.com/dotnetchina/RRQMSocket/wikis/2.3%20%E5%88%9B%E5%BB%BA%E3%80%81%E4%BD%BF%E7%94%A8TcpService?sort_id=3897349)
#### 1.2 特点
- 简单易用。
- 多线程处理。
- IOCP完美设计模型，避免收到数据再复制。
- 简单、稳定管理客户端连接，自动检验客户端活性。
- 超简单的解决粘包、分包问题，详见[RRQMSocket解决TCP粘包、分包问题](https://blog.csdn.net/qq_40374647/article/details/110680179?spm=1001.2014.3001.5501)。
- 内存池设计，避免内存重复申请、释放。
- 对象池设计，避免数据对象的申请、释放。
#### 1.3 Demo
[RRQMSocket.Demo](https://gitee.com/RRQM_Home/RRQMSocket.Demo)

## 二、Token系TCP框架
#### 2.1 概述
Token系服务器是基于Tcp服务器一款限定连接的服务器，其主要功能就是对即将完成连接的客户端进行筛选，筛选手段就是验证Token，如果Token不符合规定，则直接断开连接，其他功能和Tcp服务器一致。
#### 2.2 特点
- 过滤不合格TCP客户端。
- 多租户使用。
- 客户端服务器统一ID，方便检索。
#### 2.3 创建及使用Token系框架
[创建及使用Token系框架](https://gitee.com/dotnetchina/RRQMSocket/wikis/3.2%20%E5%88%9B%E5%BB%BA%E3%80%81%E4%BD%BF%E7%94%A8Token%E6%9C%8D%E5%8A%A1%E5%99%A8?sort_id=3896799)
#### 2.4 Demo
[RRQMSocket.Demo](https://gitee.com/RRQM_Home/RRQMSocket.Demo)


## 三、文件传输框架
#### 3.1 创建文件服务器框架

以下进行简单示例，详细使用见[文件传输入门](https://gitee.com/dotnetchina/RRQMSocket/wikis/5.1%20%E6%A6%82%E8%BF%B0?sort_id=3897485)
```
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

#### 3.2 特点
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

#### 3.3 Demo示例
 **Demo位置：** [RRQMSocket.FileTransfer.Demo](https://gitee.com/RRQM_Home/RRQMSocket.FileTransfer.Demo)

 **说明：** 可以看到，图一正在上传一个Window的系统镜像文件，大约4.2Gb，传输速度已达到346Mb/s，这是因为服务器和客户端在同一电脑上，磁盘性能限制导致的。其次，GC基本上没有释放，性能非常强悍，图二是下载文件，性能依旧非常强悍。

![上传文件](https://images.gitee.com/uploads/images/2021/0409/190350_92a2ad36_8553710.png "上传文件")
![下载文件](https://images.gitee.com/uploads/images/2021/0409/190954_a212982d_8553710.png "下载文件")


## 四、RPC框架
#### 4.1 创建RPC服务
新建类文件，继承于ServerProvider，并将其中公共方法标识为RRQMRPCMethod即可。
```
public class Server: ServerProvider
{
    [RRQMRPCMethod]
    public string TestOne(string str)
    {
        return "若汝棋茗";
    }
 }
```
#### 4.2 启动RPC服务

[启动RPC服务说明](https://gitee.com/dotnetchina/RRQMSocket/wikis/6.3%20%E5%88%9B%E5%BB%BA%E3%80%81%E5%90%AF%E5%8A%A8RPC%E6%9C%8D%E5%8A%A1%E5%99%A8?sort_id=3904370)

#### 4.3 特点
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

#### 4.3 Demo示例
 **Demo位置：** [RRQMSocket.RPC.Demo](https://gitee.com/RRQM_Home/RRQMSocket.RPC.Demo)

 **说明：** 
图一、图二、图三分别为`UDP无反馈调用`、`TCP有反馈调用`、`TCP连接池有反馈调用`。调用次数均为10w次，调用性能非常nice。在无反馈中，吞吐量达14.28w，在有反馈中达2.72w，简直秒杀WCF（WCF使用http协议，在本机测试吞吐量为310）

![输入图片说明](https://images.gitee.com/uploads/images/2021/0409/191343_e5827d04_8553710.png "屏幕截图.png")

![输入图片说明](https://images.gitee.com/uploads/images/2021/0409/191501_abec9e45_8553710.png "屏幕截图.png")

![输入图片说明](https://images.gitee.com/uploads/images/2021/0409/191531_d7f0a8d4_8553710.png "屏幕截图.png")


## 致谢

谢谢大家对我的支持，如果还有其他问题，请加群QQ：234762506讨论。


## 💕 支持本项目
您的支持就是我不懈努力的动力。打赏时请一定留下您的称呼。

 **赞助总金额:366.6￥** 

**赞助名单：** 

（以下排名只按照打赏时间顺序）

> 1.Bobo Joker

> 2.UnitySir

> 3.Coffee

<img src="https://images.gitee.com/uploads/images/2021/0330/234046_7662fb8c_8553710.png" width = "600" height = "400" alt="图片名称" align=center />

