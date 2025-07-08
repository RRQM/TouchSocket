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
using System.Threading;

namespace TouchSocket.Core;

/// <summary>
/// 文件写入器。
/// </summary>
public partial class FileStorageWriter : SafetyDisposableObject
{
    private long m_position;


    /// <summary>
    /// 初始化FileStorageWriter的实例。
    /// </summary>
    /// <param name="fileStorage">文件存储服务的实例，用于后续的文件写入操作。</param>
    public FileStorageWriter(FileStorage fileStorage)
    {
        // 当fileStorage为<see langword="null"/>时，抛出带有参数名称的ArgumentException，确保fileStorage参数不是null
        this.FileStorage = ThrowHelper.ThrowArgumentNullExceptionIf(fileStorage, nameof(fileStorage));
    }

    /// <summary>
    /// 析构函数
    /// </summary>
    ~FileStorageWriter()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        this.Dispose(disposing: false);
    }

    /// <summary>
    /// 文件存储器
    /// </summary>
    public FileStorage FileStorage { get; private set; }

    /// <summary>
    /// 游标位置
    /// </summary>
    public long Position { get => this.m_position; set => this.m_position = value; }


    /// <summary>
    /// 将文件指针移动到文件末尾。
    /// </summary>
    /// <returns>返回文件末尾的位置。</returns>
    public long SeekToEnd()
    {
        // 设置文件指针到文件末尾，并返回该位置
        return this.Position = this.FileStorage.Length;
    }

    /// <summary>
    /// 将一组只读字节写入文件存储。
    /// </summary>
    /// <param name="span">要写入的只读字节范围。</param>
    public void Write(ReadOnlySpan<byte> span)
    {
        // 调用FileStorage的Write方法，将当前Position位置的span长度字节写入。
        this.FileStorage.Write(this.Position, span);
        // 使用Interlocked类的Add方法，线程安全地更新当前Position。
        Interlocked.Add(ref this.m_position, span.Length);
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        FilePool.TryReleaseFile(this.FileStorage.Path);
        this.FileStorage = null;
    }
}