// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

namespace TouchSocket.Core;

/// <summary>
/// 表示字节块读取器接口，提供字节块的读取功能。
/// 继承自<see cref="IBytesReader"/>和<see cref="IByteBlockCore"/>接口。
/// </summary>
/// <remarks>
/// IByteBlockReader接口结合了通用字节读取和字节块核心功能，
/// 为字节块的读取操作提供了专门的接口定义。
/// </remarks>
public interface IByteBlockReader : IBytesReader, IByteBlockCore
{
    /// <summary>
    /// 获取当前可读取的字节长度。
    /// </summary>
    /// <value>表示从当前位置到数据末尾还可以读取的字节数。</value>
    /// <remarks>
    /// 此值等于<see cref="IByteBlockCore.Length"/>与<see cref="IByteBlockCore.Position"/>的差值。
    /// </remarks>
    int CanReadLength { get; }
}