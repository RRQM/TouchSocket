**En** | [中](./README.zh.md)
<p></p>
<p></p>
<p align="center">
<img src="logo.png" width = "100" height = "100" alt="The name of the image" align=center />
</p>

 <div align="center">  

[![NuGet(TouchSocket)](https://img.shields.io/nuget/v/TouchSocket.svg?label=TouchSocket)](https://www.nuget.org/packages/TouchSocket/)
[![NuGet(TouchSocket)](https://img.shields.io/nuget/dt/TouchSocket.svg)](https://www.nuget.org/packages/TouchSocket/)
[![License](https://img.shields.io/badge/license-Apache%202-4EB1BA.svg)](https://www.apache.org/licenses/LICENSE-2.0.html)
[![star](https://gitee.com/RRQM_Home/TouchSocket/badge/star.svg?theme=gvp)](https://gitee.com/RRQM_Home/TouchSocket) 
[![star](https://gitcode.com/RRQM_Home/TouchSocket/star/badge.svg)](https://gitcode.com/RRQM_Home/TouchSocket) 
[![fork](https://gitee.com/RRQM_Home/TouchSocket/badge/fork.svg?theme=gvp)](https://gitee.com/RRQM_Home/TouchSocket/members)
<a href="https://jq.qq.com/?_wv=1027&k=gN7UL4fw">
<img src="https://img.shields.io/badge/QQ group-234762506-red" alt="QQ">
</a>
[![NuGet(TouchSocket)](https://img.shields.io/github/stars/RRQM/TouchSocket?logo=github)](https://github.com/RRQM/TouchSocket)


</div>  

<div align="center">

三十功名尘与土，八千里路云和月。

</div>

## 🎀Description

![Alt](https://repobeats.axiom.co/api/embed/7b543e0b31f0488b08dfd319fafca0044dfd1050.svg "Repobeats analytics image")

'TouchSocket' is a powerful and easy-to-use.NET network communication framework for languages such as C#, VB.Net, and F#. It provides a variety of communication modules, including TCP, UDP, SSL, WebSocket, Modbus, etc. It supports solving the problem of TCP packet subcontracting and UDP large packet fragment combination. The framework supports a variety of protocol templates to quickly parse data packets such as fixed headers, fixed lengths, and interval characters.

***

## 🌟Related Documentation

- [The first page of the document](https://touchsocket.net/)
- [Getting started as a newbie](https://touchsocket.net/docs/current/startguide)
- [API documentation](https://touchsocket.net/api/)

***

## 🖥Supporting the environment

- .NET Framework 4.5 or later.
- .NET 6.0 and above.
- .NET Standard 2.0 or later.

## 🥪Support frameworks

- Console
- WPF
- Winform
- Blazor
- Xamarin
- MAUI
- Avalonia 
- Mono
- Unity 3D(except WebGL)
- Other (i.e. all C# faculties)


## 🌴A quick overview of TouchSocket features

#### Traditional IOCP and TouchSocket IOCP modes

The IOCP of TouchSocket is not the same as the traditional one, just take Microsoft's official example as an example, he uses MemoryBuffer to open up a piece of memory, divide it evenly, and then assign a zone to each session to receive, and then **copy** the received data, and then process the copied data. The TouchSocket is to take a usable memory block from the memory pool before each reception, and then use it directly for receiving, and after receiving the data, the memory block is directly thrown out for processing, so as to avoid the **copy operation**, although it is only a small design, but when transmitting **10w**times**64kb**data, the performance difference is **10 times**.

#### Data Processing Adapters

I believe that everyone has used other Socket products, so TouchSocket is also designed to learn from the excellent design concept of other products, [data processing adapter](https://touchsocket.net/docs/current/adapterdescription) is one of them, but unlike the design of other products, the adapter of TouchSocket is more powerful and easy to use. and flexible. Not only can it parse packets in advance, but it can also parse data objects, which can be replaced at any time and then take effect immediately. For example, you can use a fixed packet header to preprocess the data, so as to solve the problem of data subcontracting and sticky packet. It can also directly parse the HTTP data protocol, WebSocket data protocol, etc.

#### Compatibility and adaptation

TouchSocket provides a variety of framework models and is fully compatible with all protocols based on TCP and UDP protocols. For example, TcpService and TcpClient have the same basic functions as Sockets, but they enhance the robustness and concurrency of the framework, and throw the connection and received data in the form of events, so that users can use it more user-friendly.

## 🔗Contact the author

- [Blog Homepage](https://blog.csdn.net/qq_40374647)
- [Bilibili video](https://space.bilibili.com/94253567)
- [Source code repository home page](https://gitee.com/RRQM_Home) 
- QQ group:[234762506](https://jq.qq.com/?_wv=1027&k=gN7UL4fw)


## 👑Functional mapping

<p align="center">
<img src="images/1.png" alt="The name of the image" align=center />
</p>

## ✨Simple example

The following is just a simple example of how to create an example, please see [Documentation](https://touchsocket.net/) for more details.

#### TcpService

```
TcpService service = new TcpService();
service.Connected = (client, e) => { return EasyTask.CompletedTask; };//有客户端连接
service.Closed = (client, e) => { return EasyTask.CompletedTask; };//有客户端断开连接
service.Received = (client, e) =>
{
    //Received information from the client
    string mes = e.ByteBlock.Span.ToString(Encoding.UTF8);
    Console.WriteLine($"From {client.Id} received message：{mes}");
    return EasyTask.CompletedTask;
};
await service.StartAsync(7789);//Start listening
```

#### TcpClient

```
TcpClient tcpClient = new TcpClient();
tcpClient.Connected = (client, e) => { return EasyTask.CompletedTask; };//Successfully connected to the server
tcpClient.Closed = (client, e) => { return EasyTask.CompletedTask; };//Disconnect from the server, which is not triggered when the connection is unsuccessful.
tcpClient.Received = (client, e) =>
{
    //Information is received from the server
    string mes = e.ByteBlock.Span.ToString(Encoding.UTF8);
    Console.WriteLine($"Message received: {mes}");
    return EasyTask.CompletedTask;
};

await tcpClient.ConnectAsync("127.0.0.1:7789");

//Send a message to the server
await tcpClient.SendAsync("Hello");
```

#### TcpClient reconnect

In the plug-in configuration of Config, you can use the reconnection plug-in.

```
.ConfigurePlugins(a=> 
{
   a.UseReconnection(5, true, 1000);
});
```

#### FixedHeaderPackageAdapter package

The adapter mainly solves the problem of Tcp sticking and packing, and the data format adopts a simple and efficient "Baotou + Data Body" mode, in which Baotou supports:

- Byte mode (1+n), a maximum of 255 bytes of data can be received at one time.
- Ushort mode (2+n), the maximum number of bytes received at a time is 65535 bytes.
- Int mode (4+n), maximum reception of 2G data at a time.

The above data headers all use the default side mode (little-end mode) of TouchSocketBitConverter, and users can switch the default side mode according to their needs.

```
TouchSocketBitConverter.DefaultEndianType = EndianType.Little;
```

#### CustomFixedHeaderDataHandlingAdapter

The user-defined fixed header adapter mainly helps users solve the data frame information with fixed headers. For example, the following data formats only need to implement a few interfaces to complete the parsing, please refer to the API for details.

|1|1|1|**********|

#### CustomUnfixedHeaderDataHandlingAdapter

The user-defined non-fixed header adapter mainly helps users solve the problem of data frame information with unfixed headers. For example, the most typical HTTP packets have a data header separated from the data body by "\r\n", and the data header is not fixed because of the different request information of the requester, and the length of the data body is also explicitly specified by the value of the ContentLength of the data header, so you can consider using CustomUnfixedHeaderDataHandlingAdapter to parse, which can also be achieved through simple development.

***

## Thanks

Thank you for your support of TouchSocket, if you have any other questions, please submit an Issue, or join the group QQ: [234762506](https://jq.qq.com/?_wv=1027&k=gN7UL4fw) to discuss.

Thanks to the support of the following tools

- [Visual Studio](https://visualstudio.microsoft.com/zh-hans/)
- [JetBrains](https://www.jetbrains.com/)
- [Visual Studio Code](https://code.visualstudio.com/)

## Support the author

- [Tipping support](https://touchsocket.net/docs/current/donate)
- [Pro support](https://touchsocket.net/docs/current/enterprise)

## Special Statement

The TouchSocket project has been added to the [dotNET China](https://gitee.com/dotnetchina) organization.


![dotnetchina](https://images.gitee.com/uploads/images/2021/0324/120117_2da9922c_416720.png "132645_21007ea0_974299.png")

