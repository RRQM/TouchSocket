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

using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc;

/// <summary>
/// Dmtp RPC 调用上下文接口。
/// 该接口继承自 <see cref="ICallContext"/>，用于表示一次 Dmtp RPC 调用过程中的上下文信息。
/// </summary>
public interface IDmtpRpcCallContext : ICallContext
{
    /// <summary>
    /// 获取用于本次调用的序列化类型。
    /// </summary>
    /// <value>当前调用使用的 <see cref="SerializationType"/>。</value>
    SerializationType SerializationType { get; }

    /// <summary>
    /// 获取调用相关的元数据信息。
    /// </summary>
    /// <value>包含调用键值对信息的 <see cref="Metadata"/> 实例。</value>
    Metadata Metadata { get; }

    /// <summary>
    /// 获取当前调用对应的 Dmtp RPC 请求包。
    /// </summary>
    /// <value>表示请求包的 <see cref="IDmtpRpcRequestPackage"/> 实例。</value>
    IDmtpRpcRequestPackage DmtpRpcPackage { get; }
}