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
| 名称 |描述|
|---|---|
|[![NuGet version (RRQMSocket)](https://img.shields.io/nuget/v/RRQMSocket.svg?label=RRQMSocket)](https://www.nuget.org/packages/RRQMSocket/)| **RRQMSocket**是一个整合性的、超轻量级的、可以免费商用使用的网络通信服务框架。<br>它具有 **高并发连接** 、 **高并发处理** 、 **事件订阅** 、 **插件式扩展** 、<br> **多线程处理** 、 **内存池** 、 **对象池** 等特点，<br>让使用者能够更加简单的、快速的搭建网络框架。|
|[![NuGet version](https://img.shields.io/nuget/v/RRQMSocketFramework.svg?label=RRQMSocketFramework)](https://www.nuget.org/packages/RRQMSocketFramework/)| **RRQMSocketFramework**是RRQMSocket系列的企业版，<br>两者在功能上几乎没有区别，但是RRQMSocketFramework无任何依赖，<br>且可以提供专属的定制功能。后续也会加入企业已定制的优秀功能，希望大家多多支持。|
| [![NuGet version (RRQMSocket.FileTransfer)](https://img.shields.io/nuget/v/RRQMSocket.FileTransfer.svg?label=RRQMSocket.FileTransfer)](https://www.nuget.org/packages/RRQMSocket.FileTransfer/) |  RRQMSocket.FileTransfer是一个高性能的文件传输框架，<br>您可以用它传输**任意大小**的文件，它可以完美支持**上传下载混合式队列传输**、<br>**断点续传**、 **快速上传** 、**传输限速**、**获取文件信息**、**删除文件**等。<br>在实时测试中，它的传输速率可达1000Mb/s。 |
|[![NuGet version (RRQMSocket.RPC)](https://img.shields.io/nuget/v/RRQMSocket.RPC.svg?label=RRQMSocket.RPC)](https://www.nuget.org/packages/RRQMSocket.RPC/)                            |RPC是一个超轻量、高性能、可扩展的微服务管理平台框架，<br>目前已完成开发**RRQMRPC**、**XmlRpc**、**JsonRpc**、**WebApi**部分。<br> **RRQMRPC**部分使用RRQM专属协议，支持客户端**异步调用**，<br>服务端**异步触发**、以及**out**和**ref**关键字，**函数回调**等。<br>在调用效率上也是非常强悍，在调用空载函数，且返回状态时，<br>**10w**次调用仅用时**3.8**秒，不返回状态用时**0.9**秒。<br>其他协议调用性能详看性能评测。|
|[![NuGet version (RRQMSocket.RPC.WebApi)](https://img.shields.io/nuget/v/RRQMSocket.RPC.WebApi.svg?label=RRQMSocket.RPC.WebApi)](https://www.nuget.org/packages/RRQMSocket.RPC.WebApi/)| WebApi是一个扩展于RRQMSocket.RPC的WebApi组件，<br>可以通过该组件创建WebApi服务解析器，让桌面端、Web端、移动端可以跨语言调用RPC函数。<br>功能支持路由、Get传参、Post传参等。|
|[![NuGet version (RRQMSocket.RPC.XmlRpc)](https://img.shields.io/nuget/v/RRQMSocket.RPC.XmlRpc.svg?label=RRQMSocket.RPC.XmlRpc)](https://www.nuget.org/packages/RRQMSocket.RPC.XmlRpc/)| XmlRpc是一个扩展于RRQMSocket.RPC的XmlRpc组件，<br>可以通过该组件创建XmlRpc服务解析器，完美支持XmlRpc数据类型，类型嵌套，<br>Array等，也能与CookComputing.XmlRpcV2完美对接。不限Web，Android等平台。|
| [![NuGet version (RRQMSocket.RPC.JsonRpc)](https://img.shields.io/nuget/v/RRQMSocket.RPC.JsonRpc.svg?label=RRQMSocket.RPC.JsonRpc)](https://www.nuget.org/packages/RRQMSocket.RPC.JsonRpc/)| JsonRpc是一个扩展于RRQMSocket.RPC的JsonRpc组件，<br>可以通过该组件创建JsonRpc服务解析器，支持JsonRpc全部功能，可与Web，Android等平台无缝对接。|
| [![NuGet version (RRQMSocket.Http)](https://img.shields.io/nuget/v/RRQMSocket.Http.svg?label=RRQMSocket.Http)](https://www.nuget.org/packages/RRQMSocket.Http/)  |  RRQMSocket.Http是一个能够简单解析Http的服务组件，<br>能够快速响应Http服务请求。|


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

对象池在RRQMSocket有很多应用，最主要的两个就是**连接对象池**和**处理对象池**。连接对象池就是当客户端成功连接时，首先会去连接对象池中找SocketClient，然后没有的话，才会创建。如果哪个客户端掉线了，它的SocketClient就会被回收。

然后就是处理对象池，在RRQMSocket中，接收数据的线程和IOCP内核线程是分开的（也可以设置拥塞接收），也就是比如说客户端给服务器发送了1w条数据，但是服务器收到后处理起来很慢，那传统的iocp肯定会放慢接收速率，然后通知客户端的tcp窗口，发生拥塞，然后让客户端暂缓发送。但是在RRQMSocket中会把收到的数据通过队列全都存起来，首先不影响iocp的接收，同时再分配线程去处理收到的报文信息，这样就相当于一个“泄洪湖泊”，能很大程度的提高处理数据的能力。

#### 多线程

由于有**处理对象池**的存在，使多线程处理变得简单。在客户端连接完成时，会自动分配该客户端辅助类（TcpSocketClient）的消息处理逻辑线程，假如服务器线程数量为10，则第一个连接的客户端会被分配到0号线程中，第二个连接将被分配到1号线程中，以此类推，循环分配。当某个客户端收到数据时，会将数据排入当前线程所独自拥有的队列当中，并唤醒线程执行。

#### 传统IOCP和RRQMSocket

RRQMSocket的IOCP和传统也不一样，就以微软官方示例为例，使用MemoryBuffer开辟一块内存，均分，然后给每个会话分配一个区接收，等收到数据后，再**复制**源数据，然后把复制的数据进行处理。而RRQMSocket是每次接收之前，从内存池拿一个可用内存块，然后**直接用于接收**，等收到数据以后，直接就把这个内存块抛出处理，这样就避免了**复制操作**，虽然只是细小的设计，但是在传输**1000w**次**64kb**的数据时，性能相差了**10倍**。

#### 数据处理适配器

相信大家都使用过其他的Socket产品，例如HPSocket，SuperSocket等，那么RRQMSocket在设计时也是借鉴了其他产品的优秀设计理念，数据处理适配器就是其中之一，但和其他产品的设计不同的是，RRQMSocket的适配器功能更加强大，它不仅可以提前解析数据包，还可以解析数据对象。例如：可以使用固定包头对数据进行预处理，从而解决数据分包、粘包的问题。也可以直接解析HTTP协议，经过适配器处理后传回一个HttpRequest对象等。

#### 粘包、分包解决

在RRQMSocket中处理TCP粘包、分包问题是非常简单的。只需要更改不同的**数据处理适配器**即可。例如：使用**固定包头**，只需要给SocketClient和TcpClient配置注入**FixedHeaderDataHandlingAdapter**的实例即可。同样对应的处理器也有**固定长度** 、 **终止字符分割** 等。

#### 兼容性与适配

RRQMSocket提供多种框架模型，能够完全兼容基于TCP、UDP协议的所有协议。例如：TcpService与TcpClient，其基础功能和Socket一模一样，只是增强了框架的**坚固性**和**并发性**，将**连接**和**接收数据**通过事件的形式抛出，让使用者能够更加友好的使用。

其次，RRQMSocket也提供了一些特定的服务器和客户端，如TokenService和TokenClient，这两个就必须配套使用，不然在验证Token时会被主动断开。

## 🔗联系作者

- [CSDN博客主页](https://blog.csdn.net/qq_40374647)
- [哔哩哔哩视频](https://space.bilibili.com/94253567)
- [源代码仓库主页](https://gitee.com/RRQM_Home) 
- 交流QQ群：234762506

## 🍻RRQM系产品

| 名称| 版本（Nuget Version）|下载（Nuget Download）| 描述 |
|------|----------|-------------|-------|
| [RRQMCore](https://gitee.com/RRQM_OS/RRQMCore) | [![NuGet version (RRQMCore)](https://img.shields.io/nuget/v/RRQMCore.svg?style=flat-square)](https://www.nuget.org/packages/RRQMCore/) | [![Download](https://img.shields.io/nuget/dt/RRQMCore)](https://www.nuget.org/packages/RRQMCore/) | RRQMCore是为RRQM系提供基础服务功能的库，其中包含：**内存池**、**对象池**、**等待逻辑池**、**AppMessenger**、**3DES加密**、**Xml快速存储**、**运行时间测量器**、**文件快捷操作**、**高性能序列化器**、**规范日志接口**等。 |
| [RRQMSkin](https://gitee.com/RRQM_OS/RRQMSkin) | [![NuGet version (RRQMSkin)](https://img.shields.io/nuget/v/RRQMSkin.svg?style=flat-square)](https://www.nuget.org/packages/RRQMSkin/) | [![Download](https://img.shields.io/nuget/dt/RRQMSkin)](https://www.nuget.org/packages/RRQMSkin/) | RRQMSkin是WPF的控件样式库，其中包含： **无边框窗体** 、 **圆角窗体** 、 **水波纹按钮** 、 **输入提示筛选框** 、 **控件拖动效果** 、**圆角图片框**、 **弧形文字** 、 **扇形元素** 、 **指针元素** 、 **饼图** 、 **时钟** 、 **速度表盘** 等。|  

## 一、TCP框架

#### 1.1 说明

TCP框架是RRQMSocket最基础的框架，它定制了后继成员的创建、管理，维护、使用等一系列的规则，让使用者无需关心连接、掉线、失活检测、多线程安全等问题，能够专注于数据处理。

#### 1.2 安装

工具 ➨ Nuegt包管理器 ➨ 程序包管理器控制台

```CSharp
Install-Package RRQMSocket
```

#### 1.3 特点

- 简单易用。
- 多线程。
- **多地址监听**（可以一次性监听多个IP及端口）
- 适配器预处理，一键式解决**分包**、**粘包**、对象解析(如HTTP，Json)等。
- 超简单的同步发送、异步发送、接收等操作。
- 基于事件驱动，让每一步操作尽在掌握。
- 高性能（服务器每秒可接收200w条信息）
- **独立线程内存池**（每个线程拥有自己的内存池）

#### 1.4 应用场景

- C/S服务器开发。
- 制造业自动化控制服务器。
- 物联网数据采集服务器。
- 游戏服务器开发。

#### 1.5 API文档

[RRQMSocket API文档](https://gitee.com/RRQM_OS/RRQM/wikis/pages?sort_id=3984527&doc_id=1402901)

#### 1.6 Demo

[RRQMBox](https://gitee.com/RRQM_OS/RRQMBox)

## 二、Token框架

#### 2.1 说明

TokenService框架是RRQMSocket提供的派生自TcpService的基础框架，它在TCP基础之上，通过验证Token的方式，可以规范、筛选连接者。这样可以很大程度的**保护服务器**不疲于非法连接者的攻击。

#### 2.2 安装

工具➨Nuegt包管理器 ➨ 程序包管理器控制台

```CSharp
Install-Package RRQMSocket
```

#### 2.3 特点

- **规范**、**筛选**连接者，保护服务器。
- 客户端与服务器必须**配套**使用。

#### 2.4 应用场景

- C/S服务器开发。
- 制造业自动化控制服务器。
- 游戏服务器开发。

#### 2.5 API文档

[RRQMSocket API文档](https://gitee.com/RRQM_OS/RRQM/wikis/pages?sort_id=3984517&doc_id=1402901)

#### 2.6 Demo

[RRQMBox](https://gitee.com/RRQM_OS/RRQMBox)

## 三、Protocol框架

#### 3.1 说明

ProtocolService框架是RRQMSocket提供的派生自TokenService的基础框架，它在Token基础之上，提供**协议+数据**的形式发送，其中还包括**协议冲突检测**、**协议数据占位**等。

#### 3.2 安装

工具 ➨ Nuegt包管理器 ➨ 程序包管理器控制台

```CSharp
Install-Package RRQMSocket
```

#### 3.3 特点

- 支持**ID同步**。
- 快捷**协议发送**。

#### 3.4 应用场景

- C/S服务器开发。
- 制造业自动化控制服务器。
- 游戏服务器开发。

#### 3.5 API文档

[RRQMSocket API文档](https://gitee.com/RRQM_OS/RRQM/wikis/pages?sort_id=3984517&doc_id=1402901)

#### 3.6 Demo

[RRQMBox](https://gitee.com/RRQM_OS/RRQMBox)

## 四、RPCService框架

#### 4.1 说明

RPCService框架是所有远程过程调用的微服务调用管理平台，在该平台的托管下，使多种协议、多种序列化方式调用成为可能。目前可使用RRQMRPC、WebApi、XmlRpc、JsonRpc共同调用。

#### 4.2 RPC解析器

**说明：** RPCService仅仅是对调用的服务进行管理和维护，并不参与实质性的通信过程。实际上由于通信协议、序列化方式的不同，需要创建相对应的解析器才能完成调用操作。

#### 4.3 RPC解析器之RRQMRPC

##### 4.3.1 说明

RRQMRPC是基于Protocol框架、固定包头解析的远程调用框架，也是RRQM中性能最强悍、使用最简单、功能最强大的RPC框架。

##### 4.3.2 特点

- 支持**自定义**类型参数。
- 支持具有**默认值**的参数设定。
- 支持**out、ref** 关键字参数。
- 支持服务器**回调客户端** 。
- 支持**客户端**之间**相互调用**。
- 支持TCP、UDP等不同的协议调用相同服务。
- 支持异步调用。
- 支持权限管理，让非法调用死在萌芽时期。
- 支持**静态织入调用**，**静态编译调用**，也支持**方法名+参数**调用。
- 支持**调用配置**（类似MQTT的AtMostOnce，AtLeastOnce，ExactlyOnce）。
- **支持EventBus**（企业版支持）。
- 支持**自定义序列化**。
- **全异常反馈** ，服务器调用状态会完整的反馈到客户端（可以设置不反馈）。
- 高性能，在保证送达但不返回的情况下，10w次调用用时0.8s，在返回的情况下，用时3.9s。

##### 4.3.3 安装

工具 ➨ Nuegt包管理器 ➨ 程序包管理器控制台

```CSharp
Install-Package RRQMSocket.RPC
```

##### 4.3.4 RRQMRPC性能测试

 **说明：** 
图一、图二、图三分别为`UDP无反馈调用`、`TCP有反馈调用`、`TCP连接池有反馈调用`。调用次数均为10w次，调用性能非常nice。在无反馈中，吞吐量达14.28w，在有反馈中达2.72w。

![输入图片说明](https://images.gitee.com/uploads/images/2021/0409/191343_e5827d04_8553710.png "屏幕截图.png")

![输入图片说明](https://images.gitee.com/uploads/images/2021/0409/191501_abec9e45_8553710.png "屏幕截图.png")

![输入图片说明](https://images.gitee.com/uploads/images/2021/0409/191531_d7f0a8d4_8553710.png "屏幕截图.png")



#### 4.4 RPC解析器之WebApi

##### 4.4.1 说明

使用WebApi解析器，就可以在RPCService中通过WebApi的调用方式直接调用服务。

##### 4.4.2 特点

- 高性能，100个客户端，10w次调用，仅用时17s。
- **全异常反馈** 。
- 支持大部分路由规则。
- 支持js、Android等调用。

##### 4.4.3 安装

工具 ➨ Nuegt包管理器 ➨ 程序包管理器控制台

```CSharp
Install-Package RRQMSocket.RPC.WebApi
```

#### 4.5 RPC解析器之XmlRpc

##### 4.5.1 说明

使用XmlRpc解析器，就可以在RPCService中通过XmlRpc的调用方式直接调用服务，客户端可以使用**CookComputing.XmlRpcV2**进行对接。

##### 4.5.2 特点

- **异常反馈** 。
- 支持自定义类型。
- 支持类型嵌套。
- 支持Array及自定义Array嵌套。
- 支持js、Android等调用。

##### 4.5.3 安装

工具 ➨ Nuegt包管理器 ➨ 程序包管理器控制台

```CSharp
Install-Package RRQMSocket.RPC.XmlRpc
```

#### 4.6 RPC解析器之JsonRpc

##### 4.6.1 说明

使用JsonRpc解析器，就可以在RPCService中通过Json字符串直接调用服务。

##### 4.6.2 特点

- **异常反馈** 。
- 支持自定义类型。
- 支持类型嵌套。
- 支持js、Android等调用。

##### 4.6.3 安装

工具 ➨ Nuegt包管理器 ➨ 程序包管理器控制台

```CSharp
Install-Package RRQMSocket.RPC.JsonRpc
```

#### 4.7 API文档

[RRQMSocket API文档](https://gitee.com/RRQM_OS/RRQM/wikis/pages?sort_id=3984517&doc_id=1402901)

#### 4.8 Demo

[RRQMBox](https://gitee.com/RRQM_OS/RRQMBox)

## 五、文件传输框架

#### 5.1 说明

RRQMSocket.FileTransfer是一个高性能的文件传输框架，由于它派生自RRQMRPC，所以也具备RPC的全部特性。

#### 5.2 特点

- 简单易用。
- 多线程处理。
- 高性能，传输速度可达**1000Mb/s**。
- 超简单的**传输限速**设置，1k-10Gb 无级调节。
- 超简单的传输速度、传输进度获取。
- 随心所欲的暂停、继续、停止传输。
- 系统化的权限管理，让敏感文件只允许**私有化下载**。
- **RPC交互**，让客户端和服务器交流不延迟。
- 基于**事件驱动**，让每一步操作尽在掌握。
- 可视化的文件块流，可以实现像迅雷一样的**填充式进度条**。
- 超简单的**断点续传**设置，为大文件传输保驾护航。
- 无状态上传断点续传设置，让同一个文件，在不同客户端之间**接力上传**。
- **断网续传**（企业版支持）
- 已经上传的文件，再次上传时，可实现**快速上传**。
- 极少的GC释放。

#### 5.3 安装

工具 ➨ Nuegt包管理器 ➨ 程序包管理器控制台

```CSharp
Install-Package RRQMSocket.FileTransfer
```

#### 5.4 Demo示例

 **Demo位置：** [RRQMBox](https://gitee.com/RRQM_OS/RRQMBox)

#### 5.5 性能测试

 **说明：** 可以看到，下图正在上传一个Window的系统镜像文件，大约4.2Gb，传输速度已达到800Mb/s，性能非常强悍。其次，GC基本上没有释放。
![](https://s3.bmp.ovh/imgs/2021/08/08f17f67b9e06bc7.gif)

## 致谢

谢谢大家对我的支持，如果还有其他问题，请加群QQ：234762506讨论。

[![Giteye chart](https://chart.giteye.net/gitee/dotnetchina/RRQMSocket/RJ9NH249.png)](https://giteye.net/chart/RJ9NH249)

## 💕 支持本项目
您的支持就是我不懈努力的动力。

#### 爱心赞助名单（以下排名只按照打赏时间顺序）

 1. Bobo Joker（200￥）
 2. UnitySir（66￥）
 3. Coffee（100￥）
 4. Ninety（50￥）
 5. *琼（100￥）
 6. **安（5￥）
 7. **文（200￥）
 8. tonychen899（50￥）

#### 商业授权名单（以下排名只按照商业采购时间顺序）

 1. 凯斯得****有限公司
 2. 爱你三千遍
 3. 小老虎
 4. 三只松鼠
 5. 午后的阳光
 6. 菜鸟老何

#### 免费版提名名单（以下排名只按照时间顺序）（欢迎大家提供信息）
 1. 上海建勖****有限公司

<img src="https://images.gitee.com/uploads/images/2021/0330/234046_7662fb8c_8553710.png" width = "600" height = "400" alt="图片名称" align=center />

