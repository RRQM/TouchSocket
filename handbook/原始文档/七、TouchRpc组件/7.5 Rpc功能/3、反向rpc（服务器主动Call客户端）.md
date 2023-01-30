---
title: 3、反向rpc（服务器主动Call客户端）
url: https://www.yuque.com/rrqm/touchsocket/c04fcb27ca323f0fb63072bd8234ce14
---

<a name="MY0r2"></a>

## 一、说明

一般的rpc服务都是客户端发起，服务器响应。但是有时候也需要服务器主动调用客户端，所以需要反向rpc。 <a name="FlEWm"></a>

## 二、定义、发布反向RPC服务

实际上，所有的Rpc客户端（**TcpTouchRpcClient**、**UdpTouchRpc**（不区分客户端）、**HttpTouchRpcClient**、**WSTouchRpcClient**）也实现了**IRpcParser**接口，这意味着反向RPC其实**也是RPC**，所以，所有操作一模一样。因为当客户端和服务器建立连接以后，就不再区分谁是客户端，谁是服务器了。只关心，**谁能提供服务，谁在调用服务**。

下列就以简单的示例下，由客户端声明服务，服务器调用服务。

具体步骤：

1. 在**客户端项目**中定义服务
2. 用**TouchRpc**标记

```csharp
public class ReverseCallbackServer : RpcServer
{
    [TouchRpc]
    public string SayHello(string name)
    {
        return $"{name},hi";
    }
}
```

**【客户端发布服务】**
发布服务，实际上是让TcpTouchRpcClient也拥有提供RPC的能力。

```csharp
TcpTouchRpcClient client = new TcpTouchRpcClient();
client.Setup(new TouchSocketConfig()
    .SetRemoteIPHost("127.0.0.1:7789")
    .ConfigureContainer(a =>
    {
        a.AddConsoleLogger();
        a.AddFileLogger();
    })
    .ConfigureRpcStore(a =>
    {
        a.RegisterServer<ReverseCallbackServer>();
    })
    .SetVerifyToken("TouchRpc"));
client.Connect();
```

<a name="bU1Ar"></a>

## 三、调用反向RPC

服务器回调客户端，最终必须通过**服务器辅助类客户端**（ISocketClient的派生类），以TcpTouchRpcService为例，其辅助客户端为TcpTouchRpcSocketClient。

因为，TcpTouchRpcSocketClient已实现IRpcClient接口，意味着，反向RPC也可以使用代理调用。所有用法和RPC一致。
![](..\\..\assets\c04fcb27ca323f0fb63072bd8234ce14\153443_c5ba10c1_8553710.png)

<a name="gcqgW"></a>

## 四、获取辅助类的途径

下列示例以TcpTouchRpcSocketClient为例，其余一致。

<a name="KAi8i"></a>

#### 4.1 通过服务器直接获取

可以获取所有终端。

```csharp
foreach (var item in tcpTouchRpcService.GetClients())
{
    client.Logger.Info(item.Invoke<string>("ReverseRpcConsoleApp.ReverseCallbackServer.SayHello".ToLower(), InvokeOption.WaitInvoke, "张三"));
}
```

也可以先筛选ID，然后再调用。

```csharp
string id = tcpTouchRpcService.GetIDs().FirstOrDefault(a => a.Equals("特定id"));
if (tcpTouchRpcService.TryGetSocketClient(id, out var rpcSocketClient))
{
    rpcSocketClient.Invoke<string>("ReverseRpcConsoleApp.ReverseCallbackServer.SayHello".ToLower(), InvokeOption.WaitInvoke, "张三");
}
```

<a name="ax7IL"></a>

#### 4.2 通过调用上下文获取

具体步骤

1. [设置调用上下文](<3. 调用上下文.md>)
2. 上下文的Caller，即为服务器辅助类终端，进行强转即可。
