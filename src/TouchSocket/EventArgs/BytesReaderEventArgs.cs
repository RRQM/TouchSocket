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
/// 表示用于在插件之间传递字节读取器的事件参数。
/// </summary>
public class BytesReaderEventArgs : PluginEventArgs
{
    /// <summary>
    /// 使用指定的 <see cref="IBytesReader"/> 初始化一个新的 <see cref="BytesReaderEventArgs"/> 实例。
    /// </summary>
    /// <param name="reader">要传递的字节读取器。</param>
    public BytesReaderEventArgs(IBytesReader reader)
    {
        this.Reader = reader;
    }

    /// <summary>
    /// 初始化一个新的 <see cref="BytesReaderEventArgs"/> 实例。
    /// </summary>
    public BytesReaderEventArgs()
    {

    }

    /// <summary>
    /// 重置事件参数并设置要传递的字节读取器。
    /// </summary>
    /// <param name="reader">要传递的字节读取器。</param>
    /// <returns>返回当前 <see cref="BytesReaderEventArgs"/> 实例以便链式调用。</returns>
    public BytesReaderEventArgs Reset(IBytesReader reader)
    {
        this.Reader = reader;
        base.Reset();
        return this;
    }

    /// <summary>
    /// 获取要传递的字节读取器。
    /// </summary>
    public IBytesReader Reader { get; private set; }
}
