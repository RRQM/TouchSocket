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

#### 传统IOCP和RRQMSocket

RRQMSocket的IOCP和传统也不一样，就以微软官方示例为例，使用MemoryBuffer开辟一块内存，均分，然后给每个会话分配一个区接收，等收到数据后，再**复制**源数据，然后把复制的数据进行处理。而RRQMSocket是每次接收之前，从内存池拿一个可用内存块，然后**直接用于接收**，等收到数据以后，直接就把这个内存块抛出处理，这样就避免了**复制操作**，虽然只是细小的设计，但是在传输**1000w**次**64kb**的数据时，性能相差了**10倍**。

#### 数据处理适配器

相信大家都使用过其他的Socket产品，例如HPSocket，SuperSocket等，那么RRQMSocket在设计时也是借鉴了其他产品的优秀设计理念，数据处理适配器就是其中之一，但和其他产品的设计不同的是，RRQMSocket的适配器功能更加强大，它不仅可以提前解析数据包，还可以解析数据对象。例如：可以使用固定包头对数据进行预处理，从而解决数据分包、粘包的问题。也可以直接解析HTTP协议，经过适配器处理后传回一个HttpRequest对象等。

#### 兼容性与适配

RRQMSocket提供多种框架模型，能够完全兼容基于TCP、UDP协议的所有协议。例如：TcpService与TcpClient，其基础功能和Socket一模一样，只是增强了框架的**坚固性**和**并发性**，将**连接**和**接收数据**通过事件的形式抛出，让使用者能够更加友好的使用。

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

## 链接目录
- [ API首页 ](https://gitee.com/RRQM_OS/RRQM/wikis/pages)
- [说明](https://gitee.com/RRQM_OS/RRQM/wikis/pages?sort_id=3984529&doc_id=1402901)
- [ 历史更新 ](https://gitee.com/RRQM_OS/RRQM/wikis/pages?sort_id=4255623&doc_id=1402901)
- [ 商业运营 ](https://gitee.com/RRQM_OS/RRQM/wikis/pages?sort_id=4215572&doc_id=1402901)
- [Config说明](https://gitee.com/RRQM_OS/RRQM/wikis/pages?sort_id=4164467&doc_id=1402901)
- [疑难解答](https://gitee.com/RRQM_OS/RRQM/wikis/pages?sort_id=4245320&doc_id=1402901)
- [【RRQMCore】RRQMSocket基类库基础用法，包含内存池、对象池、高性能序列化等](https://blog.csdn.net/qq_40374647/article/details/121485490)
- [【RRQMSocket】C# 创建高并发、高性能TCP服务器，可用于解析HTTP、FTP、MQTT、搭建游戏服务器等](https://blog.csdn.net/qq_40374647/article/details/110679663)
- [【RRQMSocket】C# 创建TCP客户端，对应RRQM服务器或其他服务器](https://blog.csdn.net/qq_40374647/article/details/121469610)
- [【RRQMSocket】C# 利用数据处理适配器解决粘包、分包问题](https://blog.csdn.net/qq_40374647/article/details/110680179)
- [【RRQMSocket】C# 搭建Token系TCP服务器和客户端](https://blog.csdn.net/qq_40374647/article/details/115586146)
- [【RRQMSocket】C# 创建多租户模式的Token服务器](https://blog.csdn.net/qq_40374647/article/details/121471982)
- [【RRQMSocket】C# 搭建TCP自定义协议、Protocol服务器和客户端](https://blog.csdn.net/qq_40374647/article/details/121472932)
- [【RRQMSocket】C# 基于TCP、自定义协议Channel持续传输大数据](https://blog.csdn.net/qq_40374647/article/details/121494319)
- [【RRQMSocket】C# 基于TCP自定义协议Stream、文件等发送与接收](https://blog.csdn.net/qq_40374647/article/details/121496385)
- [【RRQMSocket.RPC】C# 创建多协议、多方式、多语言、多平台RPC框架](https://blog.csdn.net/qq_40374647/article/details/109143243)
- [C# 创建基于TCP协议的支持多序列化、ref和out关键字的RPC](https://blog.csdn.net/qq_40374647/article/details/121512993)

## 应用场景模拟
***
 **【场景一】** 

我的服务器（客户端）是物联网硬件（或PLC硬件），我可以使用什么吗？可以的话，应该从哪里学习？

 **【应用一】** 

完全可以使用，一般的硬件通信，都是基于TCP协议的指定协议数据包，可以直接学习搭建**TCP服务器**（或**客户端**），然后使用数据数据处理器解析数据。不过一般的，在RRQM内置的解析器不足以应对所有的数据协议包，所以可能需要自己重写数据处理适配器。

***

 **【场景二】** 

我想搭建游戏服务器，性能够吗？如何选择应用组件？

 **【应用二】** 

首先绝对性能是由物理机决定的，不同的物理机、操作系统，具有不同性能表现。在RRQM中，网络操作模式全遵循IOCP，该模式下在window平台性能显著，最大接收连接数在1w是没有任何问题的。经测试，在个人电脑上（i7-10H，8G RAM），服务器每秒处理数据流量可达1Gb，处理并发数据量200w次（每条4字节）。所以，性能基本上没什么问题的。组件的话，如果客户端也采用RRQM（Unity也可以使用的），那建议使用“TCP+固定包头”，这样解析数据比较方便。如果是其他客户端，就需要自己制定分包策略。

***

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
 7. 泉州市自发****有限公司

#### 免费版提名名单（以下排名只按照时间顺序）（欢迎大家提供信息）
 1. 上海建勖****有限公司

<img src="https://images.gitee.com/uploads/images/2021/0330/234046_7662fb8c_8553710.png" width = "600" height = "400" alt="图片名称" align=center />

