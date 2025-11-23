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

namespace TouchSocket.Dmtp.FileTransfer;


/// <summary>
/// 表示远程文件系统中文件的信息。
/// 继承自RemoteFileSystemInfo类，提供了关于远程文件或文件夹的详细信息。
/// </summary>
public class RemoteFileInfo : RemoteFileSystemInfo
{
    /// <summary>
    /// 初始化一个RemoteFileInfo
    /// </summary>
    public RemoteFileInfo()
    {
    }

    /// <summary>
    /// 从FileInfo初始化一个RemoteFileInfo
    /// </summary>
    /// <param name="fileInfo">用于初始化RemoteFileInfo的FileInfo对象</param>
    public RemoteFileInfo(FileInfo fileInfo)
    {
        this.FullName = fileInfo.FullName; // 设置完整路径
        this.Name = fileInfo.Name; // 设置文件名
        this.Length = fileInfo.Length; // 设置文件长度
        this.Attributes = fileInfo.Attributes; // 设置文件属性
        this.CreationTime = fileInfo.CreationTime; // 设置文件创建时间
        this.LastWriteTime = fileInfo.LastWriteTime; // 设置文件最后写入时间
        this.LastAccessTime = fileInfo.LastAccessTime; // 设置文件最后访问时间
    }

    /// <summary>
    /// 文件大小
    /// </summary>
    public long Length { get; set; }

    /// <summary>
    /// 文件MD5
    /// </summary>
    public string MD5 { get; set; }

    /// <inheritdoc/>
    public override void Package<TWriter>(ref TWriter writer)
    {
        base.Package(ref writer);
        WriterExtension.WriteString(ref writer, this.MD5);
        WriterExtension.WriteValue<TWriter, long>(ref writer, this.Length);
    }

    /// <inheritdoc/>
    public override void Unpackage<TReader>(ref TReader reader)
    {
        base.Unpackage(ref reader);
        this.MD5 = ReaderExtension.ReadString(ref reader);
        this.Length = ReaderExtension.ReadValue<TReader, long>(ref reader);
    }
}