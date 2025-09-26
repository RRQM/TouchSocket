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

namespace TouchSocket.Core;

/// <summary>
/// 表示字节块的接口，提供字节缓冲区的读写和管理功能。
/// 继承自<see cref="IByteBlockReader"/>、<see cref="IByteBlockWriter"/>和<see cref="IDisposable"/>接口。
/// </summary>
/// <remarks>
/// IByteBlock接口结合了字节读取、写入和资源管理的功能，是字节块操作的核心接口。
/// 实现此接口的类型应该提供完整的字节缓冲区管理能力。
/// </remarks>
public interface IByteBlock : IByteBlockReader, IByteBlockWriter, IDisposable
{
    /// <summary>
    /// 获取一个值，该值指示字节块当前是否正在使用中。
    /// </summary>
    /// <value>如果字节块正在使用中，则为 <see langword="true"/>；否则为 <see langword="false"/>。</value>
    bool Using { get; }

    /// <summary>
    /// 清除字节块中的所有数据，将所有字节设置为零。
    /// </summary>
    /// <remarks>
    /// 此方法不会改变字节块的容量或位置，只是清零数据内容。
    /// </remarks>
    void Clear();

    /// <summary>
    /// 重置字节块到初始状态，将位置和长度重置为零。
    /// </summary>
    /// <remarks>
    /// 调用此方法后，字节块可以重新开始写入操作。
    /// </remarks>
    void Reset();
}