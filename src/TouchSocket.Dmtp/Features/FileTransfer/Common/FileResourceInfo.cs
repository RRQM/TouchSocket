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
using System.Linq;
using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// 文件资源信息
    /// </summary>
    public class FileResourceInfo : PackageBase
    {
        /// <summary>
        /// 初始化一个本地资源
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="fileSectionSize"></param>
        public FileResourceInfo(FileInfo fileInfo, int fileSectionSize)
        {
            if (!File.Exists(fileInfo.FullName))
            {
                throw new FileNotFoundException("文件不存在", fileInfo.FullName);
            }

            this.Create(fileInfo.Map<RemoteFileInfo>(), fileSectionSize);
        }

        /// <summary>
        /// 初始化一个本地资源
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileSectionSize"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public FileResourceInfo(string filePath, int fileSectionSize) : this(new FileInfo(filePath), fileSectionSize)
        {
        }

        /// <summary>
        /// 初始化一个资源
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="fileSectionSize"></param>
        public FileResourceInfo(RemoteFileInfo fileInfo, int fileSectionSize)
        {
            this.Create(fileInfo, fileSectionSize);
        }

        /// <summary>
        /// 资源文件信息
        /// </summary>
        public RemoteFileInfo FileInfo { get; private set; }

        /// <summary>
        /// 资源分块集合。
        /// </summary>
        public FileSection[] FileSections { get; private set; }

        /// <summary>
        /// 文件分块尺寸。
        /// </summary>
        public int FileSectionSize { get; private set; }

        /// <summary>
        /// 资源句柄唯一标识
        /// </summary>
        public int ResourceHandle { get; private set; }

        /// <summary>
        /// 按文件块状态，获取块集合，如果没用找到任何元素，则返回空数组。
        /// </summary>
        /// <param name="fileSectionStatus"></param>
        /// <returns></returns>
        public FileSection[] GetFileSections(FileSectionStatus fileSectionStatus)
        {
            return this.FileSections.Where(a => a.Status == fileSectionStatus).ToArray();
        }

        /// <inheritdoc/>
        public override void Package(ByteBlock byteBlock)
        {
            byteBlock.Write(this.ResourceHandle);
            byteBlock.WritePackage(this.FileInfo);
            byteBlock.Write(this.FileSections.Length);
            foreach (var item in this.FileSections)
            {
                item.Package(byteBlock);
            }
        }

        /// <summary>
        /// 重新设置资源标识。
        /// </summary>
        /// <param name="handle"></param>
        public void ResetResourceHandle(int handle)
        {
            foreach (var item in this.FileSections)
            {
                item.ResourceHandle = handle;
            }
            this.ResourceHandle = handle;
        }

        /// <inheritdoc/>
        public override void Unpackage(ByteBlock byteBlock)
        {
            this.ResourceHandle = byteBlock.ReadInt32();
            this.FileInfo = byteBlock.ReadPackage<RemoteFileInfo>();
            var count = byteBlock.ReadInt32();
            this.FileSections = new FileSection[count];
            for (var i = 0; i < count; i++)
            {
                var fileSection = new FileSection();
                fileSection.Unpackage(byteBlock);
                this.FileSections[i] = fileSection;
            }
        }

        private void Create(RemoteFileInfo fileInfo, long fileSectionSize)
        {
            this.FileSectionSize = (int)fileSectionSize;
            var sectionCount = (int)((fileInfo.Length / fileSectionSize) + 1);
            var sections = new FileSection[sectionCount];
            for (var i = 0; i < sectionCount; i++)
            {
                var fileSection = new FileSection()
                {
                    Offset = i * fileSectionSize,
                    Length = (int)Math.Min(fileInfo.Length - i * fileSectionSize, fileSectionSize),
                    ResourceHandle = this.GetHashCode(),
                    Status = FileSectionStatus.Default,
                    Index = i
                };
                sections[i] = fileSection;
            }
            this.ResourceHandle = this.GetHashCode();
            this.FileInfo = fileInfo;
            this.FileSections = sections;
        }
    }
}