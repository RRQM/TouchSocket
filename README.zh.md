**中** | [En](./README.md)
<p></p>
<p></p>
<p align="center">
<img src="logo.png" width = "100" height = "100" alt="图片名称" align=center />
</p>

 <div align="center"> 

[![NuGet(TouchSocket)](https://img.shields.io/nuget/v/TouchSocket.svg?label=TouchSocket)](https://www.nuget.org/packages/TouchSocket/)
[![NuGet(TouchSocket)](https://img.shields.io/nuget/dt/TouchSocket.svg)](https://www.nuget.org/packages/TouchSocket/)
[![License](https://img.shields.io/badge/license-Apache%202-4EB1BA.svg)](https://www.apache.org/licenses/LICENSE-2.0.html)
[![star](https://gitee.com/RRQM_Home/TouchSocket/badge/star.svg?theme=gvp)](https://gitee.com/RRQM_Home/TouchSocket/stargazers) 
[![star](https://gitcode.com/RRQM_Home/TouchSocket/star/badge.svg)](https://gitcode.com/RRQM_Home/TouchSocket) 
[![fork](https://gitee.com/RRQM_Home/TouchSocket/badge/fork.svg?theme=gvp)](https://gitee.com/RRQM_Home/TouchSocket/members)
<a href="https://jq.qq.com/?_wv=1027&k=gN7UL4fw">
<img src="https://img.shields.io/badge/QQ群-234762506-red" alt="QQ">
</a>
[![NuGet(TouchSocket)](https://img.shields.io/github/stars/RRQM/TouchSocket?logo=github)](https://github.com/RRQM/TouchSocket)


</div>  

<div align="center">

三十功名尘与土，八千里路云和月。

</div>

## 🎀描述

![Alt](https://repobeats.axiom.co/api/embed/7b543e0b31f0488b08dfd319fafca0044dfd1050.svg "Repobeats analytics image")

`TouchSocket`是一个功能强大且易于使用的.NET 网络通信框架，适用于C#、VB.Net 和 F#等语言。它提供了多种通信模块，包括TCP、UDP、SSL、WebSocket、Modbus等。支持解决TCP黏包分包问题和UDP大数据包分片组合问题。框架支持多种协议模板，快速实现固定包头、固定长度和区间字符等数据报文解析。

***

## 🌟相关文档

- [文档首页](https://touchsocket.net/)
- [新手入门](https://touchsocket.net/docs/current/startguide)
- [API文档](https://touchsocket.net/api/)

***

## 🖥支持环境

- .NET Framework4.5及以上。
- .NET 6.0及以上。
- .NET Standard2.0及以上。

## 🥪支持框架

- Console
- WPF
- Winform
- Blazor
- Xamarin
- MAUI
- Avalonia 
- Mono
- Unity 3D（除WebGL）
- 其他（即所有C#系）


## 🌴TouchSocket特点速览

#### 传统IOCP和TouchSocket的IOCP模式

TouchSocket的IOCP和传统也不一样，就以微软官方示例为例，他是使用MemoryBuffer开辟一块内存，均分，然后给每个会话分配一个区接收，等收到数据后，再**复制**接收的数据，然后把复制的数据进行处理。而TouchSocket是每次接收之前，从内存池拿一个可用内存块，然后**直接用于接收**，等收到数据以后，直接就把这个内存块抛出处理，这样就避免了**复制操作**，虽然只是细小的设计，但是在传输**10w**次**64kb**的数据时，性能相差了**10倍**。

#### 数据处理适配器

相信大家都使用过其他的Socket产品，那么TouchSocket在设计时也是借鉴了其他产品的优秀设计理念，[数据处理适配器](https://touchsocket.net/docs/current/adapterdescription)就是其中之一，但和其他产品的设计不同的是，TouchSocket的适配器功能更加强大，易用，且灵活。它不仅可以提前解析数据包，还可以解析数据对象，可以随时替换，然后立即生效。例如：可以使用固定包头对数据进行预处理，从而解决**数据分包**、**粘包**的问题。也可以直接解析**HTTP**数据协议、WebSocket数据协议等。

#### 兼容性与适配

TouchSocket提供多种框架模型，能够完全兼容基于TCP、UDP协议的所有协议。例如：TcpService与TcpClient，其基础功能和Socket一模一样，只是增强了框架的**坚固性**和**并发性**，将**连接**和**接收数据**通过事件的形式抛出，让使用者能够更加友好的使用。

## 🔗联系作者

- [CSDN博客主页](https://blog.csdn.net/qq_40374647)
- [哔哩哔哩视频](https://space.bilibili.com/94253567)
- [源代码仓库主页](https://gitee.com/RRQM_Home) 
- 交流QQ群：[234762506](https://jq.qq.com/?_wv=1027&k=gN7UL4fw)


## 👑功能导图

<p align="center">
<img src="images/1.png" alt="图片名称" align=center />
</p>

## ✨简单示例

 **_以下仅以最简方式创建示例，更多详情请查看[说明文档](https://touchsocket.net/)。_** 

#### TcpService

```
TcpService service = new TcpService();
service.Connected = (client, e) => { return EasyTask.CompletedTask; };//有客户端连接
service.Closed = (client, e) => { return EasyTask.CompletedTask; };//有客户端断开连接
service.Received = (client, e) =>
{
    //从客户端收到信息
    string mes = e.ByteBlock.Span.ToString(Encoding.UTF8);
    Console.WriteLine($"已从{client.Id}接收到信息：{mes}");
    return EasyTask.CompletedTask;
};
await service.StartAsync(7789);//启动
```

#### TcpClient

```
TcpClient tcpClient = new TcpClient();
tcpClient.Connected = (client, e) => { return EasyTask.CompletedTask; };//成功连接到服务器
tcpClient.Closed = (client, e) => { return EasyTask.CompletedTask; };//从服务器断开连接，当连接不成功时不会触发。
tcpClient.Received = (client, e) =>
{
    //从服务器收到信息
    string mes = e.ByteBlock.Span.ToString(Encoding.UTF8);
    Console.WriteLine($"接收到信息：{mes}");
    return EasyTask.CompletedTask;
};

await tcpClient.ConnectAsync("127.0.0.1:7789");

//发送信息到服务器
await tcpClient.SendAsync("Hello");
```

#### TcpClient 断线重连

在Config的插件配置中，使用重连插件即可。

```
.ConfigurePlugins(a=> 
{
   a.UseReconnection(5, true, 1000);
});
```

#### FixedHeaderPackageAdapter包模式

该适配器主要解决Tcp粘分包问题，数据格式采用简单而高效的“包头+数据体”的模式，其中包头支持：

- Byte模式（1+n），一次性最大接收255字节的数据。
- Ushort模式（2+n），一次最大接收65535字节。
- Int模式（4+n），一次最大接收2G数据。

以上数据头均采用TouchSocketBitConverter的默认端模式（小端模式），使用者可以根据需求切换默认端模式。

```
TouchSocketBitConverter.DefaultEndianType = EndianType.Little;
```

#### CustomFixedHeaderDataHandlingAdapter

用户自定义固定包头适配器，主要帮助用户解决具有固定包头的数据帧信息。例如：下列数据格式，仅需要实现几个接口，就能完成解析，详细操作请参照API。

|1|1|1|**********|

#### CustomUnfixedHeaderDataHandlingAdapter

用户自定义不固定包头适配器，主要帮助用户解决具有包头不固定的数据帧信息。例如：最典型的HTTP数据包，其数据头和数据体由“\r\n”隔开，而数据头又因为请求者的请求信息的不同，头部数据也不固定，而数据体的长度，也是由数据头的ContentLength的值显式指定的，所以可以考虑使用CustomUnfixedHeaderDataHandlingAdapter解析，也是仅通过简单的开发，就能实现。

***

## 致谢

谢谢大家对TouchSocket的支持，如果还有其他问题，请提交Issue，或者加群QQ：[234762506](https://jq.qq.com/?_wv=1027&k=gN7UL4fw)讨论。

感谢下列工具软件的支持

- [Visual Studio](https://visualstudio.microsoft.com/zh-hans/)
- [JetBrains](https://www.jetbrains.com/)
- [Visual Studio Code](https://code.visualstudio.com/)

## 支持作者

- [打赏支持](https://touchsocket.net/docs/current/donate)
- [Pro支持](https://touchsocket.net/docs/current/enterprise)

## 特别声明

TouchSocket项目已加入[dotNET China](https://gitee.com/dotnetchina) 组织。


![dotnetchina](https://images.gitee.com/uploads/images/2021/0324/120117_2da9922c_416720.png "132645_21007ea0_974299.png")

