<p></p>
<p></p>
<p align="center">
<img src="logo.png" width = "100" height = "100" alt="图片名称" align=center />
</p>

 <div align="center"> 

[![NuGet(TouchSocket)](https://img.shields.io/nuget/v/TouchSocket.svg?label=TouchSocket)](https://www.nuget.org/packages/TouchSocket/)
[![NuGet(TouchSocket)](https://img.shields.io/nuget/dt/TouchSocket.svg)](https://www.nuget.org/packages/TouchSocket/)
[![License](https://img.shields.io/badge/license-Apache%202-4EB1BA.svg)](https://www.apache.org/licenses/LICENSE-2.0.html)
[![star](https://gitee.com/RRQM_Home/TouchSocket/badge/star.svg?theme=gvp)](https://gitee.com/dotnetchina/TouchSocket/stargazers) 
[![fork](https://gitee.com/RRQM_Home/TouchSocket/badge/fork.svg?theme=gvp)](https://gitee.com/dotnetchina/TouchSocket/members)
<a href="https://jq.qq.com/?_wv=1027&k=gN7UL4fw">
<img src="https://img.shields.io/badge/QQ群-234762506-red" alt="QQ">
</a>
[![NuGet(TouchSocket)](https://img.shields.io/github/stars/RRQM/TouchSocket?logo=github)](https://github.com/RRQM/TouchSocket)

</div>  

<div align="center">

雄关漫道真如铁，而今迈步从头越。从头越，苍山如海，残阳如血。

</div>

English | [中文](README.md)

## 🎀Describe

【Open source version】
| Nuget|Url |Describe|
|---|---|---|
|[![NuGet version (TouchSocket)](https://img.shields.io/nuget/v/TouchSocket.svg?label=TouchSocket)](https://www.nuget.org/packages/TouchSocket)|[Gitee](https://gitee.com/dotnetchina/TouchSocket)<br>[Github](https://github.com/RRQM/TouchSocket)| TouchSocket is a lightweight, comprehensive network communication library that supports plug-in. <br> Basic communication functions include TCP, UDP, SSL, RPC, HTTP, etc. Among them, HTTP <br> server supports extended plugins such as WebSocket, Static web pages, XMLRPC, Webapi, JSONRPC <br>. Touchrpc with a custom protocol, support SSL encryption, asynchronous calls, <br> permissions management, error status return, service recovery, distributed calls, etc. When the empty load function <br> executes, 100,000 calls are only 3.8 seconds, and only 0.9 seconds when the state is not returned.|
| [![NuGet version (TouchSocket.AspNetCore)](https://img.shields.io/nuget/v/TouchSocket.AspNetCore.svg?label=TouchSocket.AspNetCore)](https://www.nuget.org/packages/TouchSocket.AspNetCore)|[Gitee](https://gitee.com/dotnetchina/TouchSocket)<br>[Github](https://github.com/RRQM/TouchSocket) | TouchSocket.AspNetCore is an exclusive version suitable for Aspnetcore.|

【Enterprise version】
| Nuget|Url |Describe|
|---|---|---|
|[![NuGet version (TouchSocketPro)](https://img.shields.io/nuget/v/TouchSocketPro.svg?label=TouchSocketPro)](https://www.nuget.org/packages/TouchSocketPro)|[Gitee](https://gitee.com/dotnetchina/TouchSocketPro)<br>[Github](https://github.com/RRQM/TouchSocketPro)| TouchSocketpro is the enterprise version of TouchSocket, which is based on the original. <br> There are also some corporate versions. For details, please see [Enterprise Edition Related](https://rrqm_home.gitee.io/touchsocket/docs/enterprise/)|
| [![NuGet version (TouchSocketPro.AspNetCore)](https://img.shields.io/nuget/v/TouchSocketPro.AspNetCore.svg?label=TouchSocketPro.AspNetCore)](https://www.nuget.org/packages/TouchSocketPro.AspNetCore)|[Gitee](https://gitee.com/dotnetchina/TouchSocketPro)<br>[Github](https://github.com/RRQM/TouchSocketPro) | TouchSocketpro.aspnetcore is an exclusive version suitable for Aspnetcore.|

## 🖥Support environment
- .NET Framework4.5 Above。
- .NET Core3.1 Above。
- .NET Standard2.0 Above。

## 🥪Support framework
- Console
- WPF
- Winform
- Blazor Server
- Xamarin
- MAUI
- Avalonia 
- Mono
- Unity 3D（Except webgl）
- Others (that is, all C#)


## 🌴TouchSocket Features Speed

#### IOCP mode of traditional IOCP and TouchSocket

TouchSocket's IOCP is not the same as tradition. Take the official example of Microsoft as an example. Receive data，Then process the copy of the data. And TouchSocket takes a available memory block from the memory pool each time before receiving, and then **is directly used to receive**. After receiving the data, it will directly throw this memory block and process it. Operation, although it is only a small design, when the data of the transmission **10W** times **64KB**, the performance is different **10 times**。

#### Data handle adapter

I believe that everyone has used other socket products, so TouchSocket also borrows the excellent design concept of other products during design. Data processing adapter is one of them, but unlike the design of other products, TouchSocket's adapter functions are more powerful. Easy to use and flexible. It can not only analyze the data packet in advance, but also parse the data object, which can be replaced at any time, and then take effect immediately. For example: the data can be pre -processed using a fixed Baotou to solve the problem of **data subcontracting and** sticky bags. You can also directly analyze the **http** data protocol, WebSocket data protocol, etc.

#### Compatibility and adaptation

TouchSocket offers a variety of framework models that can be fully compatible with all protocols in the TCP and UDP protocols. For example: TCPSERVICE and TCPClient, their basic functions are exactly the same as sockets, but they only enhance the framework of the framework and concurrent and connect and to receive data. Out, allowing users to use more friendly use.

## 🔗Contact the author

- [CSDN blog homepage](https://blog.csdn.net/qq_40374647)
- [Bilibili video](https://space.bilibili.com/94253567)
- [Source code warehouse homepage](https://gitee.com/RRQM_Home) 
- Exchange QQ group：234762506

## 🌟Explanation document
- [ Homepage ](http://rrqm_home.gitee.io/touchsocket)

## 👑Functional cousin

<p align="center">
<img src="images/1.png" alt="图片名称" align=center />
</p>

## ✨Simple example

 **_The following only examples are created in the simplest way. For more details, please see [Explanation Document](http://rrqm_home.gitee.io/touchsocket)。_** 

 **【TcpService】** 

```
TcpService service = new TcpService();
service.Connecting = (client, e) => { };//Some client is connecting
service.Connected = (client, e) => { };//There is a client connection
service.Disconnected = (client, e) => { };//There is a client that is cut off and connected
service.Received = (client, byteBlock, requestInfo) =>
{
    //从客户端收到信息
    string mes = byteBlock.ToString();
    Console.WriteLine($"Receive information from {client.id}：{mes}");

    client.Send(mes);//Return the received information directly to the sender

    //client.Send("id",mes);//Return the received information to the client with a specific ID

    var clients = service.GetClients();
    foreach (var targetClient in clients)//Return the received information to all the clients online。
    {
        if (targetClient.ID != client.ID)
        {
            targetClient.Send(mes);
        }
    }
};

service.Setup(new TouchSocketConfig()//Load
    .SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })//At the same time, listen to two addresses
    .SetMaxCount(10000)
    .SetThreadCount(100))
    .Start();//start up
```

 **【TcpClient】** 
```
TcpClient tcpClient = new TcpClient();
tcpClient.Connected = (client, e) => { };//Successfully connect to the server
tcpClient.Disconnected = (client, e) => { };//The connection is disconnected from the server, and it will not be triggered when the connection is unsuccessful.
tcpClient.Received = (client, byteBlock, requestInfo) =>
{
    //Receive information from the server
    string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
    Console.WriteLine($"Receive information: {MES}");
};

//Declaration configuration
TouchSocketConfig config = new TouchSocketConfig();
config.SetRemoteIPHost(new IPHost("127.0.0.1:7789"))
    .UsePlugin()
    .SetBufferLength(1024 * 10);

//Load
tcpClient.Setup(config);
tcpClient.Connect();
tcpClient.Send("RRQM");
```

 **【TcpClient Break】**
In the config plug -in configuration, you must first enable the plug -in, and then use the re -connected plug -in.

```
.UsePlugin()
.ConfigurePlugins(a=> 
{
   a.UseReconnection(5, true, 1000);
});
```

 **【FixedHeaderPackageAdapter Pack】**

This adapter mainly solves the problem of TCP adhesion package. The data format adopts a simple and efficient "Baotou+Data Body" mode. Among them, Baotou supports:

- Byte mode (1+n), one -time receives a maximum of 255 bytes.
- USHORT mode (2+n), maximum receiving 65535 bytes.
- INT mode (4+n), a maximum receiving 2G data at a time.

The above data header adopts the default end mode (small end mode) of TouchSocketbitConverter. Users can switch the default end mode according to the requirements.

```
TouchSocketBitConverter.DefaultEndianType = EndianType.Little;
```

 **【CustomFixedHeaderDataHandlingAdapter】** 

Users customize fixed Baotou adapters mainly help users solve data frame information with fixed Baotou. For example: The following data format needs to be implemented with only a few interfaces to complete the analysis. For detailed operations, please refer to the API.

|1|1|1|**********|

 **【CustomUnfixedHeaderDataHandlingAdapter】** 

Users customize non -fixed Baotou adapters mainly help users solve the data frame information with non -fixed Baotou. For example: the most typical HTTP packet, the data head and the data body are separated by "\r\n", and the data header is not fixed due to the different request information of the request request, and the length of the data body is not fixed, and the length of the data body is It is also specified by the value display of the data head, so you can consider using the CustomunfixedHeaderDatahandlingAdapter analysis, which can be achieved only through simple development.

***

## Thank you

Thank you for your support for TouchSocket. If you have any other questions, please submit it, or add a group of QQ: 234762506 discussion.

Thanks for the support of the following tools

- [Visual Studio](https://visualstudio.microsoft.com/zh-hans/)
- [JetBrains](https://www.jetbrains.com/)
- [Visual Studio Code](https://code.visualstudio.com/)

## Support author

[Support entrance](https://rrqm_home.gitee.io/touchsocket/docs/donate)
