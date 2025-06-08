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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer;

/// <summary>
/// 文件资源信息
/// </summary>
public class FileResourceInfo : PackageBase
{
    private FileSection[] m_fileSections;

    /// <summary>
    /// 初始化FileResourceInfo对象的新实例。
    /// </summary>
    /// <param name="fileInfo">要操作的文件信息。</param>
    /// <param name="fileSectionSize">文件的分段大小。</param>
    /// <exception cref="FileNotFoundException">如果指定的文件不存在，则抛出此异常。</exception>
    public FileResourceInfo(FileInfo fileInfo, int fileSectionSize)
    {
        // 检查文件是否存在，如果不存在则抛出FileNotFoundException异常
        if (!File.Exists(fileInfo.FullName))
        {
            throw new FileNotFoundException("文件不存在", fileInfo.FullName);
        }

        // 使用FileInfo对象创建RemoteFileInfo对象，并用其初始化FileResourceInfo对象
        this.PrivateCreate(fileInfo.Map<RemoteFileInfo>(), fileSectionSize);
    }

    /// <summary>
    /// 初始化一个本地资源
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="fileSectionSize">文件分区大小</param>
    /// <exception cref="FileNotFoundException">当指定路径的文件不存在时，抛出此异常</exception>
    public FileResourceInfo(string filePath, int fileSectionSize) : this(new FileInfo(filePath), fileSectionSize)
    {
    }

    /// <summary>
    /// 初始化一个远程资源
    /// </summary>
    /// <param name="fileInfo">远程文件信息对象，包含远程文件的详细信息</param>
    /// <param name="fileSectionSize">文件分段大小，用于指定处理文件时的分段策略</param>
    public FileResourceInfo(RemoteFileInfo fileInfo, int fileSectionSize)
    {
        // 调用Create方法来初始化远程资源，这是因为初始化过程可能涉及到复杂的逻辑，通过调用已有方法可以简化构造函数的代码
        this.PrivateCreate(fileInfo, fileSectionSize);
    }

    /// <summary>
    /// 从内存初始化资源
    /// </summary>
    /// <param name="byteBlock">包含资源信息的字节块</param>
    [Obsolete($"此方法已被弃用，请使用{nameof(FileResourceInfo.Create)}")]
    public FileResourceInfo(in IByteBlock byteBlock)
    {
        // 读取文件区块大小
        this.FileSectionSize = byteBlock.ReadInt32();
        // 读取资源句柄
        this.ResourceHandle = byteBlock.ReadInt32();
        // 读取文件信息
        this.FileInfo = byteBlock.ReadPackage<RemoteFileInfo>();
        // 读取文件区块数量
        var len = byteBlock.ReadInt32();

        // 根据读取的文件区块数量，创建相应的FileSection数组
        var fileSections = new FileSection[len];
        // 遍历每个文件区块，并从字节块中读取具体信息
        for (var i = 0; i < len; i++)
        {
            fileSections[i] = byteBlock.ReadPackage<FileSection>();
        }

        // 将读取的文件区块信息数组赋值给成员变量
        this.m_fileSections = fileSections;
    }

    private FileResourceInfo()
    {
    }

    /// <summary>
    /// 资源文件信息
    /// </summary>
    public RemoteFileInfo FileInfo { get; private set; }

    /// <summary>
    /// 资源分块集合。
    /// </summary>
    public FileSection[] FileSections => this.m_fileSections;

    /// <summary>
    /// 文件分块尺寸。
    /// </summary>
    public int FileSectionSize { get; private set; }

    /// <summary>
    /// 资源句柄唯一标识
    /// </summary>
    public int ResourceHandle { get; private set; }

    /// <summary>
    /// 从字节块创建一个新的 <see cref="FileResourceInfo"/> 实例。
    /// </summary>
    /// <param name="byteBlock">字节块，用于读取文件资源信息。</param>
    /// <returns>返回一个新的 <see cref="FileResourceInfo"/> 实例。</returns>
    public static FileResourceInfo Create(ByteBlock byteBlock)
    {
        return Create(ref byteBlock);
    }

    /// <summary>
    /// 从流中创建一个新的 <see cref="FileResourceInfo"/> 实例。
    /// </summary>
    /// <param name="stream">包含文件资源信息的流。</param>
    /// <returns>返回一个新的 <see cref="FileResourceInfo"/> 实例。</returns>
    public static FileResourceInfo Create(Stream stream)
    {
        return Create(new ByteBlock(stream.ReadAllToByteArray()));
    }

    /// <summary>
    /// 从字节块创建一个新的 <see cref="FileResourceInfo"/> 实例。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型，必须实现 <see cref="IByteBlock"/> 接口。</typeparam>
    /// <param name="byteBlock">引用的字节块，用于读取文件资源信息。</param>
    /// <returns>返回一个新的 <see cref="FileResourceInfo"/> 实例。</returns>
    public static FileResourceInfo Create<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        var fileResourceInfo = new FileResourceInfo();
        // 读取文件区块大小
        fileResourceInfo.FileSectionSize = byteBlock.ReadInt32();
        // 读取资源句柄
        fileResourceInfo.ResourceHandle = byteBlock.ReadInt32();
        // 读取文件信息
        fileResourceInfo.FileInfo = byteBlock.ReadPackage<RemoteFileInfo>();
        // 读取文件区块数量
        var len = byteBlock.ReadInt32();

        // 根据读取的文件区块数量，创建相应的 FileSection 数组
        var fileSections = new FileSection[len];
        // 遍历每个文件区块，并从字节块中读取具体信息
        for (var i = 0; i < len; i++)
        {
            fileSections[i] = byteBlock.ReadPackage<FileSection>();
        }

        // 将读取的文件区块信息数组赋值给成员变量
        fileResourceInfo.m_fileSections = fileSections;

        return fileResourceInfo;
    }

    /// <summary>
    /// 获取尝试续传时的索引。
    /// </summary>
    /// <returns>返回续传索引，如果所有分段都已完成，则返回分段总数。</returns>
    public int GetContinuationIndex()
    {
        // 如果分段列表为空，表示没有分段需要续传，返回0
        if (this.m_fileSections == null)
        {
            return 0;
        }

        // 遍历分段列表，查找第一个未完成的分段索引
        var i = 0;
        foreach (var item in this.m_fileSections)
        {
            // 如果找到未完成的分段，返回当前索引
            if (item.Status != FileSectionStatus.Finished)
            {
                return i;
            }
            // 未找到未完成分段前，继续遍历下一个分段
            i++;
        }

        // 所有分段都已完成，返回分段总数
        return i;
    }

    /// <summary>
    /// 按文件块状态，获取块集合，如果没用找到任何元素，则返回空数组。
    /// </summary>
    /// <param name="fileSectionStatus">文件块的状态，用于筛选特定状态的文件块。</param>
    /// <returns>返回筛选后的文件块集合，如果没有找到任何元素，则返回空数组。</returns>
    public IEnumerable<FileSection> GetFileSections(FileSectionStatus fileSectionStatus)
    {
        // 使用LINQ查询语法，筛选出所有Status为指定fileSectionStatus的FileSection对象
        return this.FileSections.Where(a => a.Status == fileSectionStatus);
    }

    /// <inheritdoc/>
    public override void Package<TByteBlock>(ref TByteBlock byteBlock)
    {
        byteBlock.WriteInt32(this.ResourceHandle);
        byteBlock.WritePackage(this.FileInfo);
    }

    /// <summary>
    /// 重新设置资源标识。
    /// </summary>
    /// <param name="handle">要设置的资源标识。</param>
    public void ResetResourceHandle(int handle)
    {
        // 遍历所有文件段，将资源标识重置为指定的handle。
        foreach (var item in this.FileSections)
        {
            item.ResourceHandle = handle;
        }
        // 将当前对象的资源标识也重置为指定的handle。
        this.ResourceHandle = handle;
    }

    /// <summary>
    /// 将<see cref="FileResourceInfo"/>对象保存到内存。
    /// </summary>
    /// <param name="byteBlock">用于存储文件资源信息的字节块参数。</param>
    public void Save<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        // 写入文件分区大小，以便在加载时正确分配内存。
        byteBlock.WriteInt32(this.FileSectionSize);
        // 写入资源句柄，用于标识文件资源。
        byteBlock.WriteInt32(this.ResourceHandle);
        // 写入文件信息包，包含文件的基本信息。
        byteBlock.WritePackage(this.FileInfo);
        // 写入文件分区数组的长度，以便在加载时正确创建分区。
        byteBlock.WriteInt32(this.m_fileSections.Length);
        // 遍历文件分区数组，将每个分区的信息写入字节块。
        foreach (var item in this.m_fileSections)
        {
            byteBlock.WritePackage(item);
        }
    }

    /// <summary>
    /// 将<see cref="FileResourceInfo"/>对象保存到内存。
    /// </summary>
    /// <param name="byteBlock">用于存储文件资源信息的字节块参数。</param>
    public void Save(ByteBlock byteBlock)
    {
        this.Save(ref byteBlock);
    }

    /// <summary>
    /// 将 <see cref="FileResourceInfo"/> 对象保存到指定的流中。
    /// </summary>
    /// <param name="stream">目标流，用于存储文件资源信息。</param>
    public void Save(Stream stream)
    {
        using (var byteBlock = new ByteBlock(1024 * 64))
        {
            this.Save(byteBlock);
            // 将字节块的内容写入到指定的流中
            stream.Write(byteBlock.Span);
        }
    }

    /// <inheritdoc/>
    public override void Unpackage<TByteBlock>(ref TByteBlock byteBlock)
    {
        this.ResourceHandle = byteBlock.ReadInt32();
        this.FileInfo = byteBlock.ReadPackage<RemoteFileInfo>();
    }

    private void PrivateCreate(RemoteFileInfo fileInfo, long fileSectionSize)
    {
        this.ResourceHandle = this.GetHashCode();
        this.FileSectionSize = (int)fileSectionSize;
        var sectionCount = (int)((fileInfo.Length / fileSectionSize) + 1);
        var sections = new FileSection[sectionCount];
        for (var i = 0; i < sectionCount; i++)
        {
            var fileSection = new FileSection()
            {
                Offset = i * fileSectionSize,
                Length = (int)Math.Min(fileInfo.Length - i * fileSectionSize, fileSectionSize),
                ResourceHandle = this.ResourceHandle,
                Status = FileSectionStatus.Default,
                Index = i
            };
            sections[i] = fileSection;
        }
        this.FileInfo = fileInfo;
        this.m_fileSections = sections;
    }
}