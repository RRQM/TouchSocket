//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.IO;

namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// 文件资源控制器。
    /// </summary>
    public interface IFileResourceController : IDisposable
    {
        /// <summary>
        /// 获取全路径
        /// </summary>
        /// <param name="root"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetFullPath(string root, string path);

        /// <summary>
        /// 加载读取的文件资源定位器。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileSectionSize"></param>
        /// <returns></returns>
        FileResourceLocator LoadFileResourceLocatorForRead(string path, int fileSectionSize);

        /// <summary>
        /// 加载写入的文件资源定位器。
        /// </summary>
        /// <param name="savePath"></param>
        /// <param name="fileResourceInfo"></param>
        /// <returns></returns>
        FileResourceLocator LoadFileResourceLocatorForWrite(string savePath, FileResourceInfo fileResourceInfo);

        /// <summary>
        /// 释放文件资源定位器
        /// </summary>
        /// <param name="resourceHandle"></param>
        /// <param name="locator"></param>
        public bool TryRelaseFileResourceLocator(int resourceHandle, out FileResourceLocator locator);

        /// <summary>
        /// 通过文件句柄，获取资源定位器。
        /// </summary>
        /// <param name="resourceHandle"></param>
        /// <param name="fileResourceLocator"></param>
        /// <returns></returns>
        bool TryGetFileResourceLocator(int resourceHandle, out FileResourceLocator fileResourceLocator);

        /// <summary>
        /// 读取文件的所有数据
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        int ReadAllBytes(FileInfo fileInfo, byte[] buffer);

        /// <summary>
        /// 写入数据到文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        void WriteAllBytes(string path, byte[] buffer, int offset, int length);
    }
}