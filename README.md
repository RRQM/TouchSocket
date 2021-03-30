

 ![Logo](/RRQMSocket/RRQM.png) 
# RRQMSocket
 

[![NuGet version (RRQMSocket)](https://img.shields.io/nuget/v/RRQMSocket.svg?style=flat-square)](https://www.nuget.org/packages?q=rrqm)
[![License](https://img.shields.io/badge/license-Apache%202-4EB1BA.svg)](https://www.apache.org/licenses/LICENSE-2.0.html)
[![Download](https://img.shields.io/nuget/dt/RRQMSocket)](https://img.shields.io/nuget/dt/RRQMSocket)

## 一、程序集描述
#### 说明
> **RRQMSocket程序集由作者若汝棋茗开发，所有版权归作者所有，程序集源代码在遵循Apache License 2.0的开源协议以及附加协议下，可供其他开发者二次开发或（商业）使用。**

#### Apache License 2.0开源协议简述
- 永久权利
- 一旦被授权，永久拥有。
- 全球范围的权利
- 在一个国家获得授权，适用于所有国家。假如你在美国，许可是从印度授权的，也没有问题。
- 授权免费，且无版税
- 前期，后期均无任何费用。
- 授权无排他性
- 任何人都可以获得授权
- 授权不可撤消
- 一旦获得授权，没有任何人可以取消。比如，你基于该产品代码开发了衍生产品，你不用担心会在某一天被禁止使用该代码。

#### 附加协议
#### 二次开发须知:
- 二次开发完成后的作品必须附带源作品所有作者信息，包括但不限于作者名、Gitee地址等。
- 完成后的作品（仅RRQMSocket部分）必须将发布时最新源代码提交一份给本作者。

**以上内容必须全部符合，开发授权才成立。**

#### 盈利性（商业）用途使用须知:
- 不得将程序集用作违法犯罪活动。
- 不得将程序集单独包装售卖。
- 不得擦除程序集所有有关作者的信息。
- 开发完成后，必须联系本作者，告知所应用程序集的产品名称及公司名称（或个人姓名）。

**以上内容必须全部符合，使用授权才成立。**


### 联系作者

 - [CSDN博客主页](https://blog.csdn.net/qq_40374647)
 - [哔哩哔哩视频](https://space.bilibili.com/94253567)
 - [源代码仓库主页](https://gitee.com/RRQM_Home) 
 - 交流QQ群：234762506

## 二、程序集功能概述
&emsp;&emsp;RRQMSocket是一个整合性的、超轻量级的网络通信框架。它具有高并发、事件订阅、插件式扩展、自动活性检测、多线程处理等特点，让使用者能够更加简单的、快速的搭建网络框架。

&emsp;&emsp;目前RRQMSocket支持的传输层协议有TCP和UDP两款，基于这两大协议的支持，使得RRQMSocket能够解析绝大部分应用层协议，比如耳熟能详的HTTP、FTP、Telnet、POP3、SMTP、DNS等，不仅如此，RRQMSocket还内置了多种分包算法，使跨语言解析变得更加简单快捷。

&emsp;&emsp;RRQMSocket内部设计大量引用了内存池、对象池、等待池、线程池等众多“池”化设计，使得整个框架在接收和发送数据时基本上避免了“创建-销毁”的性能消耗。而且在接收数据时设计了完美的IOCP模型，不仅避免了接收数据的再复制行为，还让数据接收与数据处理分线程操作，大大减轻了内核的负担，让内核专注于接收，性能由此提升不少。这些优化行为的最终结果就是让RRQMSocket拥有超强的数据接收处理能力，这也就意味着它的适用场景也更加广泛，您可以用它开发分布式服务器、游戏服务器、文件管理服务器、即时通信服务器等。我相信，它的表现一定令您满意。

&emsp;&emsp;除了基本的框架外，程序集内还开发出了一些成熟框架，目前有文件传输框架、RPC框架两种，后续还会不断更新添加其他框架，希望大家多多支持和关注。

**常用类继承图**
![在这里插入图片描述](https://img-blog.csdnimg.cn/20210303103818135.png?x-oss-process=image/watermark,type_ZmFuZ3poZW5naGVpdGk,shadow_10,text_aHR0cHM6Ly9ibG9nLmNzZG4ubmV0L3FxXzQwMzc0NjQ3,size_16,color_FFFFFF,t_70)

## 三、TCP服务框架
### 3.1 服务器类（TcpService）
#### 3.1.1 概述
> TcpService是所有基于TCP协议类的基类，也是在服务器端创建服务器的对象类，但该类是抽象类，必须通过继承才能创建其具体对象，原因是在该类中，只实现了服务器的管理功能，但未实现其数据接收和处理功能，所以必须倚仗服务器辅助类才能实现其实际功能。其简单介绍如下：
#### 3.1.2 属性、事件及方法概述
**a.属性**

- `IP`:绑定服务器所用到的IP地址。
- `Port`：绑定服务器所用到的端口号 。
- `BufferLength`：缓存池长度。
- `Logger`：日志记录器。
- `IsBand`：判断服务器绑定是否成功。
- `IsCheckClientAlive`：是否自动检验客户端活性（心跳检测）。
- `BytePool`：获取内存池实例，该实例不可直接赋值，但是可以设置其参数。
- `SocketClients`：获取已连接并且存活的客户端集合。

**b.事件**
|事件名| 触发 |
|--|--|
| ClientConnected | 当客户端连接成功时 |
| ClientDisconnected| 当客户端断开连接时 |

**c.方法**
|方法名| 功能 |
|--|--|
| Bind | 绑定本地监听IP及端口号 |
| Dispose| 断开绑定并且释放资源 |
#### 3.1.3 创建TCP服务器
具体的创建步骤详见[C# 创建高并发、高性能TCP通信框架](https://blog.csdn.net/qq_40374647/article/details/110679663?spm=1001.2014.3001.5501)
### 3.2 客户端类（TcpClient）
#### 3.2.1 概述
> TcpClient是基于TCP协议的客户端抽象类，一般的在接收和处理普通TCP报文时，客户端是不限类型、语言的。但是为统一使用，本程序集仍提供统一类型，其介绍如下：
#### 3.2.2 属性、事件及方法概述
**a.属性**

- `IP`:连接服务器所用到的IP地址。
- `Port`：连接服务器所用到的端口号 。
- `BufferLength`：缓存池长度。
- `Logger`：日志记录器。
- `MainSocket`：Socket主通信器 。
- `Online`：判断客户端是否连接成功。
- `AllowSend`：是否允许发送数据。
- `BytePool`：获取内存池实例，该实例不可直接赋值，但是可以设置其参数。
- `DataHandlingAdapter`：数据处理适配器。

**b.事件**

|事件名| 触发 |
|--|--|
| ConnectedService| 当客户端连接成功时 |
| DisConnectedService| 当客户端断开连接时 |

**c.方法**

|方法名| 功能 |
|--|--|
| Connect| 连接服务器 |
| Send| 发送数据 |
| Dispose| 断开绑定并且释放资源 |
### 3.3 服务器辅助类（TcpSocketClient）
#### 3.3.1 概述
> TcpSocketClient是基于TCP协议的服务器客户端抽象类，因为服务器类只对连接的客户端进行管理和检测，所以对于数据的处理，必须由辅助类实现，可以认为TcpSocketClient就是一个特殊的客户端类，他具有和TcpClient一样的接口功能，是直接参数数据交换的类，其介绍如下：
#### 3.3.2 属性、事件及方法概述

**a.属性**

- `IP`:连接到服务器的远程客户端的IP地址。
- `Port`：连接到服务器的远程客户端的端口号 。
- `BufferLength`：缓存池长度。
- `Logger`：日志记录器。
- `MainSocket`：Socket主通信器 。
- `Online`：判断TcpSocketClient是否在线。
- `AllowSend`：是否允许发送数据。
- `BytePool`：获取内存池实例，该实例不可直接赋值，但是可以设置其参数。
- `DataHandlingAdapter`：数据处理适配器。

**b.事件**

无

**c.方法**

|方法名| 功能 |
|--|--|
| Send| 发送数据 |
| Dispose| 断开绑定并且释放资源 |
#### 3.3.3 创建TcpSocketClient
具体的创建步骤详见[C# 创建高并发、高性能TCP通信框架](https://blog.csdn.net/qq_40374647/article/details/110679663?spm=1001.2014.3001.5501)

## 四、文件传输模块
### 4.1概述
> 文件传输模块是基于TCP协议的网络文件传输类的整合。它适用于局域网、广域网、组合网等各种网络环境中，其主要功能包括上传文件、下载文件、限速传输、断点续传。快速上传、事件通知等。
### 4.2 文件传输服务器（FileService）
#### 4.2.1 概述
> FileService是基于TcpService的类，其功能基础和TcpService一致。但是也有增强功能，其介绍如下：
#### 4.2.2 属性、事件及方法概述

**a.属性**

- `IP`:连接到服务器的远程客户端的IP地址。
- `Port`：连接到服务器的远程客户端的端口号 。
- `BufferLength`：缓存池长度。
- `Logger`：日志记录器。
- `MainSocket`：Socket主通信器 。
- `Online`：判断TcpSocketClient是否在线。
- `AllowSend`：是否允许发送数据。
- `BytePool`：获取内存池实例，该实例不可直接赋值，但是可以设置其参数。
- `DataHandlingAdapter`：数据处理适配器。
- `DownloadSpeed`：下载总速度。
- `UploadSpeed`：上传总速度。
- `BreakpointResume`：断点续传开关。
- `IsFsatUpload`：快速上传开关。

**b.事件**

|事件名| 触发 |
|--|--|
| ConnectedService| 当客户端连接成功时 |
| DisConnectedService| 当客户端断开连接时 |
| BeforeReceiveFile	|当客户端请求上传文件之前 |
| BeforeSendFile|当客户端请求下载文件之前 |
| ReceiveFileFinished| 接收完客户端上传的文件时 |
| SendFileFinished| 发送完客户端下载的文件时 |
| ReceiveSystemMes| 当收到客户发送的消息时 |
| ReceivedBytesThenReturn| 当收到客户端数据并回传时 |

**c.方法**
无
#### 4.2.3 创建FileService
具体的创建步骤详见[C#、Socket文件传输、大文件续传、可高性能、高并发运行、断点续传、数据对象传递等功能](https://blog.csdn.net/qq_40374647/article/details/100546120?spm=1001.2014.3001.5502)

### 4.3 文件传输客户端（FileClient）
#### 4.3.1 概述
> FileClient是基于TcpClient的类，其功能基础和TcpClient一致。但是也有增强功能，其介绍如下：
#### 4.3.2 属性、事件及方法概述
**a.属性**

- `IP`:连接到服务器的远程客户端的IP地址。
- `Port`：连接到服务器的远程客户端的端口号 。
- `BufferLength`：缓存池长度。
- `Logger`：日志记录器。
- `Online`：判断TcpSocketClient是否在线。
- `BytePool`：获取内存池实例，该实例不可直接赋值，但是可以设置其参数。
- `DataHandlingAdapter`：数据处理适配器。
- `DownloadSpeed`：下载速度。
- `UploadSpeed`：上传速度。
- `UploadFileBlocks`：上传文件包集合。
- `DownloadFileBlocks`：下载文件包集合。
- `Timeout`：单次请求超时时间。
- `UploadProgress`：上传进度。
- `DownloadProgress`：下载进度。
- `UploadFileInfo`：上传文件信息。
- `DownloadFileInfo`：下载文件信息。
- `ReceiveDirectory`：下载文件存放目录。

**b.事件**

|事件名| 触发 |
|--|--|
| ConnectedService| 当客户端连接成功时 |
| DisConnectedService| 当客户端断开连接时 |
| BeforeUploadFile|当客户端请求上传文件之前 |
| BeforeDownloadFile|当客户端请求下载文件之前 |
| UploadFileFinshed| 接收完客户端上传的文件时 |
| DownloadFileFinshed| 发送完客户端下载的文件时 |
| TransferFileError| 当传输文件发生致命性错误时 |


**c.方法**

|方法名| 功能 |
|--|--|
| Connect| 连接服务器 |
| DownloadFile| 下载文件 |
| PauseDownload| 暂停下载 |
| ResumeDownload| 恢复下载 |
| StopDownload| 终止下载 |
| UploadFile| 上传文件 |
| PauseUpload| 暂停上传 |
| ResumeUpload| 恢复上传 |
| StopUpload| 终止上传 |
| SendSystemMessage| 发送消息 |
| SendBytesWaitReturn| 发送流并等待返回 |
| Dispose| 释放资源 |

#### 4.3.3 创建FileClient
具体的创建步骤详见[C#、Socket文件传输、大文件续传、可高性能、高并发运行、断点续传、数据对象传递等功能](https://blog.csdn.net/qq_40374647/article/details/100546120?spm=1001.2014.3001.5502)


## 五、 RPC
RPC是远程过程调用（Remote Procedure Call）的缩写形式。客户程序通过接口调用服务内部的标准或自定义函数，获得函数返回的数据进行处理后显示或打印。类似于WCF和WebService。

### 5.1 RPC服务器（RPCService）
#### 5.1.1 概述
>RPC服务器基本上没有其他的概述，直接使用即可。
#### 5.1.2 属性、事件及方法概述
**a.属性**
无
**b.事件**
无
**c.方法**
无
#### 5.1.3 创建RPCService
具体的创建步骤详见[C# RPC高性能微框架，支持任意序列化、out及ref、可使用http协议跨平台](https://blog.csdn.net/qq_40374647/article/details/109143243?spm=1001.2014.3001.5501)

### 5.2 RPC客户端（RPCClient）
#### 5.2.1 概述
>RPC客户端基本上没有其他的概述，直接使用即可。
#### 5.2.2 属性、事件及方法概述
**a.属性**
无
**b.事件**
无
**c.方法**
无
#### 5.2.3 创建RPCClient
具体的创建步骤详见[C# RPC高性能微框架，支持任意序列化、out及ref、可使用http协议跨平台](https://blog.csdn.net/qq_40374647/article/details/109143243?spm=1001.2014.3001.5501)


## 六、使用其他功能
### 6.1加密和解密

程序集本身不提供加密及解密服务，所以文件，数据对象，文本等在传输过程中都是直接传输的，但是程序集里面整合了3DES的加密方法，具体使用可以直接调用DataLock的静态方法进行数据的加密和解密。

## 七、致谢

谢谢大家对我的支持，如果还有其他问题，请加群讨论。谢谢！

## 七、打赏
您的支持就是我不懈努力的动力。打赏时请一定留下您的称呼。小弟定会张榜展示。
 
**打赏名单：** 

（以下排名只按照打赏时间顺序）

> 1.Bobo Joker

> 2.UnitySir

<img src="https://images.gitee.com/uploads/images/2021/0330/234046_7662fb8c_8553710.png" width = "600" height = "400" alt="图片名称" align=center />

