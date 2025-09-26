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

namespace TouchSocket.Rpc;

/// <summary>
/// Rpc调用异常
/// </summary>
[Serializable]
public class RpcInvokeException : Exception
{

    /// <summary>
    /// 默认构造函数，初始化RpcInvokeException类的实例。
    /// </summary>
    public RpcInvokeException() : base() { }

    /// <summary>
    /// 初始化RpcInvokeException类的实例，并指定异常消息。
    /// </summary>
    /// <param name="message">解释异常原因的错误消息。</param>
    public RpcInvokeException(string message) : base(message) { }

    /// <summary>
    /// 使用指定的错误消息和内部异常初始化RpcInvokeException类的实例。
    /// </summary>
    /// <param name="message">解释异常原因的错误消息。</param>
    /// <param name="inner">导致当前异常的异常。</param>
    public RpcInvokeException(string message, System.Exception inner) : base(message, inner) { }
}