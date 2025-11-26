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

纸上得来终觉浅，绝知此事要躬行。

</div>


---

# 🎀 描述

![Alt](https://repobeats.axiom.co/api/embed/7b543e0b31f0488b08dfd319fafca0044dfd1050.svg)

**TouchSocket 是一个简洁、现代且高性能的 .NET 网络通信框架**，支持 C#、VB.NET、F# 等语言。
你可以用它快速构建 **TCP / UDP / MQTT / WebSocket / SSL / HTTP / Modbus / RPC** 等各种网络应用。

框架提供高吞吐的 IOCP/Socket 实现、完善的内存池、灵活的数据适配器体系，并为多种场景准备了丰富的扩展插件（心跳、重连、SSL、RPC 等）。

---

# 🌟 文档导航

* [文档首页](https://touchsocket.net/)
* [快速入门](https://touchsocket.net/docs/current/startguide)
* [API 文档](https://touchsocket.net/api/)

---

# 🖥 支持环境

* .NET Framework ≥ **4.6.2**
* .NET Standard ≥ **2.0**
* .NET ≥ **6.0**

# 🥪 支持框架

Console / WinForm / WPF / MAUI / Avalonia / Blazor / Xamarin / Unity(非 WebGL) / Mono / 其他所有 C# 生态

---

# 🌴 TouchSocket 特点速览

### 🚀 1. 真正高性能的 IOCP 实现

TouchSocket 在 IOCP 设计上进行了深度优化，与传统示例代码不同：

| 实现方式                 | 内存处理方式                       | 性能影响                   |
| -------------------- | ---------------------------- | ---------------------- |
| **传统 IOCP（官方示例）**    | 接收区固定，收到数据后需要 **复制** 出来再处理   | 多一次复制 → 高并发场景拖慢速度      |
| **TouchSocket IOCP** | 每次接收前从 **内存池** 获取可写块，并直接用于接收 | **零额外复制** → 大流量下显著提升性能 |

实测在连续传输 **10 万次、每次 64KB** 数据的场景中，性能可达到传统实现的 **约 10 倍**。

---

### 🔧 2. 适配器体系（数据处理的“插件化中枢”）

TouchSocket 最关键的模块之一——**数据处理适配器**，相比其他框架更灵活：

* 可解析 **协议头/协议体**
* 可自动处理 **粘包 / 分包**
* 可直接转换 **数据对象**
* 可随时替换适配器并立即生效
* 内置多种协议模板：固定包头、固定长度、终止字符、HTTP、WebSocket …

只需配置适配器即可让复杂协议瞬间变得可控。

---

### 🧩 3. 可插拔的框架扩展体系

TouchSocket 的 **插件系统（Plugins）** 允许在整个通信生命周期中注入行为，如：

* 自动重连
* 心跳检测
* 日志
* SSL 认证
* 自定义鉴权
* Pipeline 数据过滤

通过配置 `.ConfigurePlugins()` 即可轻松挂载。

---

### 🛠 4. 完整的 TCP/UDP 抽象与强兼容性

TCP/UDP 的使用方式与原生 Socket 一致，但 TouchSocket 提供更健壮的：

* 异常处理
* 高并发底层能力
* 安全的连接管理
* 标准化事件模型（Connected/Received/Closed 等）

你可以无缝迁移现有 Socket 逻辑。

---

### 📦 5. 统一的客户端/服务端模型

无论 TCP、UDP、WebSocket，TouchSocket 的 API 都尽量保持一致，让开发体验更加通用：

```csharp
client.SendAsync(...)
client.Received += ...
client.ConnectAsync(...)
```

学习成本极低。

---

### 🧠 6. 强大的内存池与 Span/Memory 优化

整个框架深度使用：

* **ByteBlock（高效可复用的内存块）**
* **内存池 MemoryPool**
* **Span<byte> / ReadOnlySpan<byte>**

确保在高流量下保持低 GC 血压。

---

# ✨ 简单示例

> 以下仅展示最基础的入门代码，更多示例请查看文档。

## TcpService

```csharp
TcpService service = new TcpService();

service.Connected = (client, e) => EasyTask.CompletedTask;
service.Closed = (client, e) => EasyTask.CompletedTask;

service.Received = (client, e) =>
{
    string str = e.Memory.Span.ToString(Encoding.UTF8);
    Console.WriteLine($"收到：{str}");
    return EasyTask.CompletedTask;
};

await service.StartAsync(7789);
```

## TcpClient

```csharp
TcpClient client = new TcpClient();

client.Connected = (c, e) => EasyTask.CompletedTask;
client.Closed = (c, e) => EasyTask.CompletedTask;

client.Received = (c, e) =>
{
    Console.WriteLine(e.Memory.Span.ToString());
    return EasyTask.CompletedTask;
};

await client.ConnectAsync("127.0.0.1:7789");
await client.SendAsync("Hello");
```

## TcpClient 断线重连

```csharp
.ConfigurePlugins(a =>
{
    a.UseReconnection<TcpClient>();
});
```

---

# 🧩 固定包头模式（FixedHeaderPackageAdapter）

适用于处理粘包/分包。

支持：

* **Byte = 1 + n**（≤255B）
* **Ushort = 2 + n**（≤65535B）
* **Int = 4 + n**（≤2GB）

端序由 TouchSocketBitConverter 控制：

```csharp
TouchSocketBitConverter.DefaultEndianType = EndianType.Little;
```

---

# 🧱 自定义适配器

## CustomFixedHeaderDataHandlingAdapter

适用于固定包头结构，例如：

```
| 1 | 1 | 1 | ********** |
```

## CustomUnfixedHeaderDataHandlingAdapter

适用于不固定头结构，如 HTTP：

* 头以 `\r\n\r\n` 分隔
* Content-Length 决定数据体长度

可用少量代码完成解析。

---

# 👑 功能导图

<p align="center">
  <img src="images/1.png" />
</p>

---

# 🔗 联系作者

* [CSDN 博客](https://blog.csdn.net/qq_40374647)
* [B 站视频](https://space.bilibili.com/94253567)
* [源代码仓库](https://gitee.com/RRQM_Home)
* QQ 群：234762506

---

# 🙏 致谢

感谢大家对 TouchSocket 的支持。
如有问题，欢迎 Issue 或加入 QQ 群交流。

特别感谢以下开发工具：

* Visual Studio
* JetBrains
* VS Code

---

# ❤️ 支持作者

* [打赏支持](https://touchsocket.net/docs/current/donate)
* [Pro 版本支持](https://touchsocket.net/docs/current/enterprise)

---

# 📢 特别声明

TouchSocket 已加入 **dotNET China** 组织。

<p align="center">
  <img src="https://images.gitee.com/uploads/images/2021/0324/120117_2da9922c_416720.png" width="300"/>
</p>
