---
title: 1、创建rpc服务
url: https://www.yuque.com/rrqm/touchsocket/4711f210099056a95e8940bed294737e
---

<a name="ttL7V"></a>

## 一、说明

RPC（Remote Procedure Call）远程过程调用协议，一种通过网络从远程计算机上请求服务，而不需要了解底层网络技术的协议。RPC它假定某些协议的存在，例如TPC/UDP等，为通信程序之间携带信息数据。在OSI网络七层模型中，RPC跨越了传输层和应用层，RPC使得开发，包括网络分布式多程序在内的应用程序更加容易。

过程是什么？ 过程就是业务处理、计算任务，更直白的说，就是程序，就是想调用本地方法一样调用远程的过程。

TouchRpc支持服务器与客户端互相调用，也支持客户端之间相互调用。 <a name="ivLCr"></a>

## 二、定义服务

1. 在**被调用**端中新建一个类名为**MyRpcServer**。
2. 继承于RpcServer类、或实现IRpcServer。亦或者将服务器声明为**瞬时生命**的服务，继承TransientRpcServer、或ITransientRpcServer。
3. 在该类中写**公共方法**，并用**TouchRpc**属性标签标记。

```csharp
public class MyRpcServer : RpcServer
{
    [Description("登录")]//服务描述，在生成代理时，会变成注释。
    [TouchRpc("Login")]//服务注册的函数键，此处为显式指定。默认不传参的时候，为该函数类全名+方法名的全小写。
    public bool Login(string account,string password)
    {
        if (account=="123"&&password=="abc")
        {
            return true;
        }

        return false;
    }
}
```

**瞬时生命**的服务，最大的特点就是，每个请求，都会创建一个新的服务类对象。然后可以通过**this.CallContext**直接访问当前的调用上下文。

```csharp
public class MyRpcServer : TransientRpcServer
{
    [Description("登录")]//服务描述，在生成代理时，会变成注释。
    [TouchRpc("Login")]//服务注册的函数键，此处为显式指定。默认不传参的时候，为该函数类全名+方法名的全小写。
    public bool Login(string account,string password)
    {
        if (account=="123"&&password=="abc")
        {
            return true;
        }

        return false;
    }
}
```

<a name="ai1L7"></a>

## 三、启动rpc服务器

以下仅示例基于Tcp协议TouchRpc。其他协议的服务器请看[创建TouchRpc服务器](<..\7.2 创建TouchRpc服务器.md>)

```csharp
var service =new  TcpTouchRpcService();
TouchSocketConfig config=  new TouchSocketConfig()//配置
       .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
       .ConfigureRpcStore(a=> 
       {
           a.RegisterServer<MyRpcServer>();//注册服务
       })
       .SetVerifyToken("TouchRpc");

service.Setup(config)
    .Start();

service.Logger.Info($"{service.GetType().Name}已启动");
```

<a name="cwdGR"></a>

####
