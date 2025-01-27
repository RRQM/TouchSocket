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

using System;

namespace TouchSocket.JsonRpc;

/// <summary>
/// 当使用自定义适配器时，则可以自定义数据来源。
/// </summary>
public interface IJsonRpcRequestInfo
{
    /// <summary>
    /// 获取JsonRpc数据源。
    /// </summary>
    /// <returns></returns>
    [Obsolete("该方法由于性能问题已被弃用，请使用GetJsonRpcMemory代替", true)]
    string GetJsonRpcString();

    /// <summary>
    /// 获取JsonRpc数据源的内存表示形式。
    /// </summary>
    /// <returns>JsonRpc数据源的只读内存。</returns>
    ReadOnlyMemory<byte> GetJsonRpcMemory();
}