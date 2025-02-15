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

using TouchSocket.Core;

namespace TouchSocket.JsonRpc;

/// <summary>
/// 表示一个等待结果的JsonRpc类。
/// </summary>
public class JsonRpcWaitResult : JsonRpcBase, IWaitResult
{
    /// <summary>
    /// 获取或设置错误代码。
    /// </summary>
    public int ErrorCode { get; set; }

    /// <summary>
    /// 获取或设置错误消息。
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// 获取或设置消息。
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 获取或设置结果。
    /// </summary>
    public string Result { get; set; }

    /// <summary>
    /// 获取或设置标识。
    /// </summary>
    public int Sign { get => this.Id ?? -1; set => this.Id = value; }

    /// <summary>
    /// 获取或设置状态。
    /// </summary>
    public byte Status { get; set; }
}
