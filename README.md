**En** | [中文](./README.zh.md)

<p align="center">
  <img src="logo.png" width="100" height="100" />
</p>

<div align="center">

[![NuGet(TouchSocket)](https://img.shields.io/nuget/v/TouchSocket.svg?label=TouchSocket)](https://www.nuget.org/packages/TouchSocket/)
[![NuGet(TouchSocket)](https://img.shields.io/nuget/dt/TouchSocket.svg)](https://www.nuget.org/packages/TouchSocket/)
[![License](https://img.shields.io/badge/license-Apache%202-4EB1BA.svg)](https://www.apache.org/licenses/LICENSE-2.0.html)
[![star](https://gitee.com/RRQM_Home/TouchSocket/badge/star.svg?theme=gvp)](https://gitee.com/RRQM_Home/TouchSocket/stargazers)
[![star](https://gitcode.com/RRQM_Home/TouchSocket/star/badge.svg)](https://gitcode.com/RRQM_Home/TouchSocket)
[![fork](https://gitee.com/RRQM_Home/TouchSocket/badge/fork.svg?theme=gvp)](https://gitee.com/RRQM_Home/TouchSocket/members) <a href="https://jq.qq.com/?_wv=1027&k=gN7UL4fw"> <img src="https://img.shields.io/badge/QQ Group-234762506-red"> </a>
[![NuGet(TouchSocket)](https://img.shields.io/github/stars/RRQM/TouchSocket?logo=github)](https://github.com/RRQM/TouchSocket)

</div>

<div align="center">

纸上得来终觉浅，绝知此事要躬行。

</div>

---

# 🎀 Description

![Alt](https://repobeats.axiom.co/api/embed/7b543e0b31f0488b08dfd319fafca0044dfd1050.svg)

**TouchSocket is a simple, modern, and high-performance .NET networking framework**, supporting C#, VB.NET, F#, and more.
It helps you build powerful communication applications such as **TCP / UDP / MQTT / WebSocket / SSL / HTTP / Modbus / RPC** with ease.

The framework features a highly optimized IOCP/Socket implementation, robust memory pooling, a flexible data-adapter pipeline, and a rich plugin ecosystem including heartbeat, reconnection, SSL, RPC, and more.

---

# 🌟 Documentation

* [Documentation Home](https://touchsocket.net/)
* [Getting Started](https://touchsocket.net/docs/current/startguide)
* [API Reference](https://touchsocket.net/api/)

---

# 🖥 Supported Platforms

* .NET Framework ≥ **4.6.2**
* .NET Standard ≥ **2.0**
* .NET ≥ **6.0**

# 🥪 Supported Application Types

Console / WinForm / WPF / MAUI / Avalonia / Blazor / Xamarin / Unity (non-WebGL) / Mono / All C#-based platforms

---

# 🌴 TouchSocket at a Glance (Updated & Revised)

### 🚀 1. A Truly High-Performance IOCP Implementation

TouchSocket provides a deeply optimized IOCP design.

| Implementation                          | Memory Handling                                                                | Performance Impact                                 |
| --------------------------------------- | ------------------------------------------------------------------------------ | -------------------------------------------------- |
| **Traditional IOCP (Microsoft sample)** | Uses a fixed shared buffer; received data must be **copied** to another buffer | Extra copy → High overhead under load              |
| **TouchSocket IOCP**                    | Allocates a fresh memory block from the **memory pool** for each receive       | **Zero extra copy** → Significant performance gain |

In stress tests (100k messages × 64KB), TouchSocket achieved **up to 10× faster throughput** compared to the traditional model.

---

### 🔧 2. Data Adapter System — The Heart of the Framework

The data-adapter pipeline is one of TouchSocket’s core strengths:

* Parses **headers and payloads**
* Handles **sticky packets & fragmentation**
* Converts directly to **data objects**
* Hot-swappable adapters
* Built-in templates: fixed header, fixed length, terminator, HTTP, WebSocket, etc.

Adapters make protocol development clean, modular, and highly reusable.

---

### 🧩 3. Pluggable Extension System

TouchSocket’s **Plugins** system allows extending communication behavior across the entire lifecycle:

* Auto reconnection
* Heartbeat detection
* SSL validation
* Logging
* Authentication
* Custom data pipelines

All via:

```csharp
.ConfigurePlugins(a => { ... });
```

---

### 🛠 4. Robust TCP/UDP Abstraction

TouchSocket maintains full compatibility with native Socket semantics while improving:

* Stability
* Concurrency and throughput
* Connection lifecycle management
* Exception handling
* Unified event model (Connected / Received / Closed)

You can migrate existing Socket code with minimal changes.

---

### 📦 5. Unified Client/Server API

Across TCP, UDP, WebSocket, and others, TouchSocket exposes a consistent set of APIs:

```csharp
client.ConnectAsync(...)
client.SendAsync(...)
client.Received += ...
```

This ensures a low learning curve and fast development.

---

### 🧠 6. High-Efficiency Memory Pool & Span-Based Processing

The framework is optimized with:

* **ByteBlock** reusable high-performance buffers
* **MemoryPool**
* **Span<T> / Memory<T>**

Ensuring minimal allocations and low GC pressure during heavy workloads.

---

# ✨ Basic Examples

> The following examples show only the simplest usage. Refer to the documentation for more advanced scenarios.

## TcpService

```csharp
TcpService service = new TcpService();

service.Connected = (client, e) => EasyTask.CompletedTask;
service.Closed = (client, e) => EasyTask.CompletedTask;

service.Received = (client, e) =>
{
    string str = e.Memory.Span.ToString(Encoding.UTF8);
    Console.WriteLine($"Received: {str}");
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

## TcpClient Auto-Reconnection

```csharp
.ConfigurePlugins(a =>
{
    a.UseReconnection<TcpClient>();
});
```

---

# 🧩 Fixed-Header Package Mode

Solves packet fragmentation & merging issues.

Supports:

* **Byte = 1 + n** (≤255B)
* **Ushort = 2 + n** (≤65535B)
* **Int = 4 + n** (≤2GB)

Endianness is configurable:

```csharp
TouchSocketBitConverter.DefaultEndianType = EndianType.Little;
```

---

# 🧱 Custom Adapters

## CustomFixedHeaderDataHandlingAdapter

For fixed-header formats such as:

```
| 1 | 1 | 1 | ********** |
```

## CustomUnfixedHeaderDataHandlingAdapter

For variable-header protocols such as HTTP:

* Header ends with `\r\n\r\n`
* Body length from `Content-Length`

A small amount of code can build a complete parser.

---

# 👑 Feature Overview Diagram

<p align="center">
  <img src="images/1.png" />
</p>

---

# 🔗 Contact

* [CSDN Blog](https://blog.csdn.net/qq_40374647)
* [Bilibili Videos](https://space.bilibili.com/94253567)
* [Source Repositories](https://gitee.com/RRQM_Home)
* QQ Group: **234762506**

---

# 🙏 Acknowledgements

Thank you all for supporting TouchSocket.
If you have questions, feel free to submit an issue or join the QQ group.

Special thanks to:

* Visual Studio
* JetBrains
* Visual Studio Code

---

# ❤️ Support the Author

* [Donate](https://touchsocket.net/docs/current/donate)
* [Pro Edition](https://touchsocket.net/docs/current/enterprise)

---

# 📢 Statement

TouchSocket is a member of the **dotNET China** organization.

<p align="center">
  <img src="https://images.gitee.com/uploads/images/2021/0324/120117_2da9922c_416720.png" width="300"/>
</p>
