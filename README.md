@[TOC](目录)

## 一、程序集描述

### 说明
>RRQMSocket程序集由作者若汝棋茗开发，所有版权归作者所有，程序集源代码在遵循Apache License 2.0的开源协议下，可供其他开发者二次开发及（商业）使用。
### 交流方式

 - [CSDN博客主页](https://blog.csdn.net/qq_40374647)
 - [哔哩哔哩视频](https://space.bilibili.com/94253567)
 - [源代码仓库主页](https://gitee.com/RRQM_Home) 
 - 交流QQ群：234762506

### 版本更新历史
***
      更新时间：2021.03.02
      更新内容:
      ①TCP框架
      1.全面引入RRQMCore中的优质内容，例如内存池，等待数据回传，消息通知等。
      2.TCP传出数据由Byte替换为ByteBlock，具有和流一样的操作。
      3.TcpService采用泛型，避免拆装箱操作。
      4.修复服务器多线程模式下数据解析错误。
      5.增加固定包头，固定长度，终止字符分割三种不同的数据解析功能。

      ②文件传输模块：
      1.极致优化性能，更少的内存申请、更大的传输单元，传输速度达500Mb/s

      ③RPC模块
      1.重新架构RPC，代理生成所有文件，避免元数据泄露问题。
      2.取消反向RPC内容，改为SendBytesWaitReturn方法替代。
      3.重新定义序列化接口，采用分块序列化方式，将主要序列化工作交由系统。
***
## 二、程序集功能概述
RRQMSocket是一个整合性网络通信框架，特点是支持高并发、事件驱动、易用性强、二次开发难度低等。其中主要内容包括:TCP服务通信框架、文件传输、RPC等内容，后续还会继续扩展其他组件，希望大家多多支持。

**常用类继承图**
![在这里插入图片描述](https://img-blog.csdnimg.cn/20210303103818135.png?x-oss-process=image/watermark,type_ZmFuZ3poZW5naGVpdGk,shadow_10,text_aHR0cHM6Ly9ibG9nLmNzZG4ubmV0L3FxXzQwMzc0NjQ3,size_16,color_FFFFFF,t_70)

## 三、TCP服务框架
### 3.1 服务器类（TcpService）
#### 3.1.1 概述
> TcpService是所有基于TCP协议类的基类，也是在服务器端创建服务器的对象类，但该类是抽象类，必须通过继承才能创建其具体对象，原因是在该类中，只实现了服务器的管理功能，但未实现其数据接收和处理功能，所以必须倚仗服务器辅助类才能实现其实际功能。其简单介绍如下：
#### 3.1.2 属性、事件及方法概述
**a.属性**
`IP`:绑定服务器所用到的IP地址。
`Port`：绑定服务器所用到的端口号 。
`BufferLength`：缓存池长度。
`Logger`：日志记录器。
`MainSocket`：Socket主通信器 。
`IsBand`：判断服务器绑定是否成功。
`IsCheckClientAlive`：是否自动检验客户端活性（心跳检测）。
`BytePool`：获取内存池实例，该实例不可直接赋值，但是可以设置其参数。
`SocketClients`：获取已连接并且存活的客户端集合。
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
`IP`:连接服务器所用到的IP地址。
`Port`：连接服务器所用到的端口号 。
`BufferLength`：缓存池长度。
`Logger`：日志记录器。
`MainSocket`：Socket主通信器 。
`Online`：判断客户端是否连接成功。
`AllowSend`：是否允许发送数据。
`BytePool`：获取内存池实例，该实例不可直接赋值，但是可以设置其参数。
`DataHandlingAdapter`：数据处理适配器。
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
`IP`:连接到服务器的远程客户端的IP地址。
`Port`：连接到服务器的远程客户端的端口号 。
`BufferLength`：缓存池长度。
`Logger`：日志记录器。
`MainSocket`：Socket主通信器 。
`Online`：判断TcpSocketClient是否在线。
`AllowSend`：是否允许发送数据。
`BytePool`：获取内存池实例，该实例不可直接赋值，但是可以设置其参数。
`DataHandlingAdapter`：数据处理适配器。
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
`IP`:连接到服务器的远程客户端的IP地址。
`Port`：连接到服务器的远程客户端的端口号 。
`BufferLength`：缓存池长度。
`Logger`：日志记录器。
`MainSocket`：Socket主通信器 。
`Online`：判断TcpSocketClient是否在线。
`AllowSend`：是否允许发送数据。
`BytePool`：获取内存池实例，该实例不可直接赋值，但是可以设置其参数。
`DataHandlingAdapter`：数据处理适配器。
`DownloadSpeed`：下载总速度。
`UploadSpeed`：上传总速度。
`BreakpointResume`：断点续传开关。
`IsFsatUpload`：快速上传开关。
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
`IP`:连接到服务器的远程客户端的IP地址。
`Port`：连接到服务器的远程客户端的端口号 。
`BufferLength`：缓存池长度。
`Logger`：日志记录器。
`MainSocket`：Socket主通信器 。
`Online`：判断TcpSocketClient是否在线。
`AllowSend`：是否允许发送数据。
`BytePool`：获取内存池实例，该实例不可直接赋值，但是可以设置其参数。
`DataHandlingAdapter`：数据处理适配器。
`DownloadSpeed`：下载速度。
`UploadSpeed`：上传速度。
`UploadFileBlocks`：上传文件包集合。
`DownloadFileBlocks`：下载文件包集合。
`Timeout`：单次请求超时时间。
`UploadProgress`：上传进度。
`DownloadProgress`：下载进度。
`UploadFileInfo`：上传文件信息。
`DownloadFileInfo`：下载文件信息。
`ReceiveDirectory`：下载文件存放目录。
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

