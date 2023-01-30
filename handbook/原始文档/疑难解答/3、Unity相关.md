---
title: 3、Unity相关
url: https://www.yuque.com/rrqm/touchsocket/kedvcp
---

<a name="jsaLT"></a>

## 1.【问】程序集用于Unity 3D时，RPC或其他组件有莫名其妙的异常？

<a name="w7uyB"></a>

#### 【描述】

我将TouchSocket程序集引入到U3D中后，使用了RPC功能，或者其他组件功能，在编辑器界面正常，但是发布到PC、Android等平台时无法使用？ <a name="keA5l"></a>

#### 【解决1】

首先查看项目是否设置了**IL2CPP**，如果设置了的话，可以考虑是否能设置为**Mono**，如果能，则OK。
[![image.png](..\assets\kedvcp\1649121705367-22529617-ce88-43f0-b128-6e705f763802.png)](https://sm.ms/image/QP5sTHtpeFkWzCL)

<a name="LBS5j"></a>

#### 【解决2】

需要**unity内link.xml设置(放置在Assets文件夹内)。**[**unity官方文档 托管代码剥离**](https://gitee.com/link?target=https%3A%2F%2Fdocs.unity3d.com%2Fcn%2Fcurrent%2FManual%2FManagedCodeStripping.html%23LinkXML)

**下列仅示例RPC，如果是其他组件，则添加相应程序集名称。**

```csharp
<linker>
	<assembly fullname="TouchSocket" />
</linker>
```

<a name="J2vdM"></a>

#### 【解决4】

下载[TouchSocket发行版源码](https://gitee.com/dotnetchina/RRQMSocket/releases)**(TODO:这里链接需要等新发行版发布后需修改，下面的二次开发协议链接失效)**，然后直接复制到项目使用即可（此处应当遵守[二次开发协议](https://gitee.com/RRQM_OS/RRQM/wikis/%E8%AF%B4%E6%98%8E\(%E4%BD%BF%E7%94%A8%E5%89%8D%E5%BF%85%E8%A6%81%E9%98%85%E8%AF%BB\)?sort_id=3984529)）。
