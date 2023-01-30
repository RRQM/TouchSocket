---
title: 4、客户端相互调用rpc
url: https://www.yuque.com/rrqm/touchsocket/apei4l
---

<a name="MY0r2"></a>

## 一、说明

除了正向RPC，反向RPC，TouchRpc还支持**客户端**之间互Call RPC。服务的定义与Rpc一样。 <a name="bU1Ar"></a>

## 二、互Call RPC

客户端A调用客户端B的方法，需要知道对方的**ID**。和**方法名**。然后使用下列函数调用即可。
![](..\\..\assets\apei4l\1649305598912-0d4781a5-39b8-4e87-a61e-433b4f60e86e.png)
**互Call RPC也支持调用上下文。**

**注意：客户端互Call的时候，每个请求，都需要服务同意路由，才可以被转发。所以服务器需要做一些允许操作。**

```csharp
internal class MyTouchRpcPlugin : TouchRpcPluginBase
{
    protected override void OnRouting(ITouchRpc client, PackageRouterEventArgs e)
    {
        if (e.RouterType== RouteType.Rpc)
        {
            e.IsPermitOperation = true;
        }
        base.OnRouting(client, e);
    }
}
```
