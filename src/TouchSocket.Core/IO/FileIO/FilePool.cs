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

using System.Collections.Concurrent;

namespace TouchSocket.Core;

/// <summary>
/// 文件池
/// </summary>
public static partial class FilePool
{
    private static readonly ConcurrentDictionary<string, FileStorage> s_storages = new ConcurrentDictionary<string, FileStorage>();

    /// <summary>
    /// 获取文件存储器
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>文件存储器</returns>
    public static FileStorage GetStorage(string path)
    {
        ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(path, nameof(path));
        var fullPath = Path.GetFullPath(path);

        var storage = s_storages.GetOrAdd(fullPath, p => new FileStorage(p));
        Interlocked.Increment(ref storage.m_referenceCount);
        return storage;
    }

    /// <summary>
    /// 获取文件流
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>文件流</returns>
    public static Stream GetStream(string path)
    {
        var storage = GetStorage(path);
        return new FileStorageStream(storage);
    }

    /// <summary>
    /// 释放文件
    /// </summary>
    /// <param name="storage">文件存储器</param>
    internal static void ReleaseFile(FileStorage storage)
    {
        if (Interlocked.Decrement(ref storage.m_referenceCount) <= 0)
        {
            if (s_storages.TryRemove(storage.Path, out _))
            {
                storage.DisposeInternal();
            }
        }
    }
}
