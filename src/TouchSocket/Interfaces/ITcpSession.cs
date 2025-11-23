//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

namespace TouchSocket.Sockets;

/// <summary>
/// 定义了一个ITcpSession接口，该接口继承自IClient, IResolverConfigObject, IOnlineClient, IClosableClient等多个接口。
/// 该接口的目的是为TCP会话提供一组标准的方法和属性，以实现TCP会话的创建、管理和关闭等功能。
/// </summary>
public interface ITcpSession : IDependencyClient, IResolverConfigObject, IOnlineClient, IClosableClient
{
    /// <summary>
    /// 数据处理适配器
    /// </summary>
    SingleStreamDataHandlingAdapter DataHandlingAdapter { get; }

    /// <summary>
    /// IP地址
    /// </summary>
    string IP { get; }

    /// <summary>
    /// 端口号
    /// </summary>
    int Port { get; }

    /// <summary>
    /// 使用Ssl加密
    /// </summary>
    bool UseSsl { get; }
}
