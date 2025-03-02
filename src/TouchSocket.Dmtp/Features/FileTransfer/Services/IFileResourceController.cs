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
using System.IO;

namespace TouchSocket.Dmtp.FileTransfer;

/// <summary>
/// 文件资源控制器。
/// </summary>
public interface IFileResourceController : IDisposable
{
    /// <summary>
    /// 获取全路径
    /// </summary>
    /// <param name="root">根目录路径</param>
    /// <param name="path">相对路径或部分路径</param>
    /// <returns>组合后的全路径</returns>
    string GetFullPath(string root, string path);

    /// <summary>
    /// 加载读取的文件资源定位器。
    /// </summary>
    /// <param name="path">文件路径。</param>
    /// <param name="fileSectionSize">文件分区大小。</param>
    /// <returns>返回加载的文件资源定位器。</returns>
    FileResourceLocator LoadFileResourceLocatorForRead(string path, int fileSectionSize);

    /// <summary>
    /// 加载写入的文件资源定位器。
    /// </summary>
    /// <param name="savePath">保存路径，指示文件将被写入的位置。</param>
    /// <param name="fileResourceInfo">文件资源信息，包含文件的元数据和属性。</param>
    /// <returns>返回一个 <see cref="FileResourceLocator"/> 对象，该对象用于定位和访问文件资源。</returns>
    FileResourceLocator LoadFileResourceLocatorForWrite(string savePath, FileResourceInfo fileResourceInfo);

    /// <summary>
    /// 释放文件资源定位器
    /// </summary>
    /// <param name="resourceHandle">资源句柄，标识需要释放的文件资源定位器</param>
    /// <param name="locator">输出参数，返回被释放的文件资源定位器</param>
    /// <returns>如果成功释放文件资源定位器，则返回true；否则返回false</returns>
    bool TryReleaseFileResourceLocator(int resourceHandle, out FileResourceLocator locator);

    /// <summary>
    /// 通过文件句柄，获取资源定位器。
    /// </summary>
    /// <param name="resourceHandle">文件句柄，用于标识特定的文件资源。</param>
    /// <param name="fileResourceLocator">输出参数，返回文件的资源定位器。</param>
    /// <returns>如果成功获取资源定位器，返回true；否则返回false。</returns>
    bool TryGetFileResourceLocator(int resourceHandle, out FileResourceLocator fileResourceLocator);

    /// <summary>
    /// 读取文件的所有数据
    /// </summary>
    /// <param name="fileInfo">要读取的文件信息</param>
    /// <param name="buffer">用于存储文件内容的字节数组</param>
    /// <returns>读取到的字节数</returns>
    int ReadAllBytes(FileInfo fileInfo, byte[] buffer);

    /// <summary>
    /// 写入数据到文件
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="buffer">包含要写入的数据的字节数组</param>
    /// <param name="offset">字节数组中要开始写入数据的索引</param>
    /// <param name="length">要写入的字节数</param>
    void WriteAllBytes(string path, byte[] buffer, int offset, int length);
}