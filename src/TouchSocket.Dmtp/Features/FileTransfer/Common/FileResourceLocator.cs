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
using System.Linq;
using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// 文件资源定位器。
    /// </summary>
    public class FileResourceLocator : DisposableObject
    {
        /// <summary>
        /// 缓存文件的扩展名。
        /// </summary>
        public const string ExtensionName = ".dmtpfile";

        /// <summary>
        /// 根据资源信息，初始化一个仅读的定位器。
        /// </summary>
        /// <param name="fileResourceInfo">资源信息对象，包含文件资源的详细信息。</param>
        public FileResourceLocator(FileResourceInfo fileResourceInfo)
        {
            this.FileResourceInfo = fileResourceInfo;
            this.FileAccess = FileAccess.Read;
            this.FileStorage = FilePool.GetFileStorageForRead(fileResourceInfo.FileInfo.FullName);
            this.LocatorPath = fileResourceInfo.FileInfo.FullName;
            this.LastActiveTime = DateTime.UtcNow;
        }

        /// <summary>
        /// 根据资源信息和保存目录，初始化一个可写入的定位器。
        /// </summary>
        /// <param name="fileResourceInfo">资源信息对象，包含文件资源的详细信息。</param>
        /// <param name="locatorPath">定位器路径，即文件资源的保存目录。</param>
        /// <param name="overwrite">是否覆盖现有文件，默认为 true。</param>
        /// <exception cref="ArgumentException">当 locatorPath 为 null 或空字符串时抛出。</exception>
        public FileResourceLocator(FileResourceInfo fileResourceInfo, string locatorPath, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(locatorPath))
            {
                throw new ArgumentException($"“{nameof(locatorPath)}”不能为 null 或空。", nameof(locatorPath));
            }
            if (overwrite)
            {
                FileUtility.Delete(locatorPath);
            }
            this.LocatorPath = locatorPath;
            this.FileResourceInfo = fileResourceInfo;
            this.FileAccess = FileAccess.Write;
            this.FileStorage = FilePool.GetFileStorageForWrite(this.LocatorPath + ExtensionName);
            this.LastActiveTime = DateTime.UtcNow;
        }

        /// <summary>
        /// 文件访问类型。
        /// </summary>
        public FileAccess FileAccess { get; }

        /// <summary>
        /// 资源信息。
        /// </summary>
        public FileResourceInfo FileResourceInfo { get; private set; }

        /// <summary>
        /// 文件访问存储器。
        /// </summary>
        public FileStorage FileStorage { get; }

        /// <summary>
        /// 最后活动时间
        /// </summary>
        public DateTime LastActiveTime { get; private set; }

        /// <summary>
        /// 定位器指向的实际文件的全名称。
        /// </summary>
        public string LocatorPath { get; }

        /// <summary>
        /// 获取一个默认状态，或者失败状态的文件片段
        /// </summary>
        /// <returns>返回默认状态或者失败状态的文件片段</returns>
        public FileSection GetDefaultOrFailFileSection()
        {
            // 更新最后活跃时间
            this.LastActiveTime = DateTime.UtcNow;
            // 如果文件访问模式为读取，则返回默认值
            if (this.FileAccess == FileAccess.Read)
            {
                return default;
            }
            else
            {
                // 否则，返回文件资源信息中状态为默认或失败的第一个文件片段
                return this.FileResourceInfo.FileSections.FirstOrDefault(a => a.Status == FileSectionStatus.Default || a.Status == FileSectionStatus.Fail);
            }
        }

        /// <summary>
        /// 获取没有完成的文件块集合
        /// </summary>
        /// <returns>返回未完成的文件块集合</returns>
        public FileSection[] GetUnfinishedFileSection()
        {
            // 更新最后活跃时间
            this.LastActiveTime = DateTime.UtcNow;
            // 根据文件访问模式决定返回值
            // 如果是读取模式，则返回空数组
            // 否则，返回所有未完成状态的文件片段集合
            return this.FileAccess == FileAccess.Read
                ? (new FileSection[0])
                : this.FileResourceInfo.FileSections.Where(a => a.Status != FileSectionStatus.Finished).ToArray();
        }

        /// <summary>
        /// 从指定位置读取字节并存储到指定的字节跨度中。
        /// </summary>
        /// <param name="pos">要开始读取的文件位置。</param>
        /// <param name="span">用于存储读取的字节的跨度。</param>
        /// <returns>读取的字节数。</returns>
        public int ReadBytes(long pos, Span<byte> span)
        {
            // 更新最后一次活动时间，用于跟踪访问时间
            this.LastActiveTime = DateTime.UtcNow;
            // 调用底层文件存储系统的读取方法，读取字节并返回读取的字节数
            return this.FileStorage.Read(pos, span);
        }

        /// <summary>
        /// 按文件块读取。
        /// </summary>
        /// <param name="fileSection">要读取的文件块信息。</param>
        /// <returns>包含读取结果的FileSectionResult对象。</returns>
        public FileSectionResult ReadFileSection(FileSection fileSection)
        {
            try
            {
                // 更新上次活跃时间，用于跟踪文件访问器的使用情况。
                this.LastActiveTime = DateTime.UtcNow;
                // 检查文件访问权限，确保是读权限。
                if (this.FileAccess != FileAccess.Read)
                {
                    return new FileSectionResult(ResultCode.Error, "该定位器是只写的。", default, fileSection);
                }
                // 检查文件块索引是否超出范围。
                if (fileSection.Index >= this.FileResourceInfo.FileSections.Length)
                {
                    return new FileSectionResult(ResultCode.Error, "实际数据块索引超出，可能写入的文件已经发生变化。", default, fileSection);
                }
                // 获取指定索引的旧文件块信息，以验证文件块的一致性。
                var oldFileSection = this.FileResourceInfo.FileSections[fileSection.Index];
                // 验证传入的文件块与现有的文件块是否一致。
                if (oldFileSection != fileSection)
                {
                    return new FileSectionResult(ResultCode.Error, "数据块不一致。", default, fileSection);
                }
                // 创建一个与文件块长度相同的字节块，用于存储读取的数据。
                var bufferByteBlock = new ByteBlock(fileSection.Length);
                // 从文件存储中读取数据到字节块中。
                var r = this.FileStorage.Read(oldFileSection.Offset, bufferByteBlock.TotalMemory.Span.Slice(0, fileSection.Length));
                // 验证读取的长度是否与预期一致。
                if (r != fileSection.Length)
                {
                    return new FileSectionResult(ResultCode.Error, "读取长度不一致。", default, fileSection);
                }
                else
                {
                    // 调整字节块的实际长度以匹配读取的长度。
                    bufferByteBlock.SetLength(r);
                    // 返回成功的结果，包含读取的字节块。
                    return new FileSectionResult(ResultCode.Success, bufferByteBlock, fileSection);
                }
            }
            catch (Exception ex)
            {
                // 捕获并处理任何异常，返回异常信息。
                return new FileSectionResult(ResultCode.Exception, ex.Message, default, fileSection);
            }
        }

        /// <summary>
        /// 重新载入资源信息，并覆盖现有的分块的所有信息。要求资源路径必须一致。
        /// </summary>
        /// <param name="fileResourceInfo">要重新加载的文件资源信息对象</param>
        /// <exception cref="Exception">如果资源路径不一致，则抛出异常</exception>
        public void ReloadFileResourceInfo(FileResourceInfo fileResourceInfo)
        {
            // 更新资源的最后活跃时间
            this.LastActiveTime = DateTime.UtcNow;
            // 检查新旧资源路径是否一致，如果不一致则抛出异常
            if (fileResourceInfo.FileInfo.FullName != this.FileResourceInfo.FileInfo.FullName)
            {
                throw new Exception("资源指向的地址不一致。");
            }

            // 更新当前实例的文件资源信息
            this.FileResourceInfo = fileResourceInfo;
        }

        /// <summary>
        /// 尝试完成该资源。
        /// </summary>
        /// <returns>返回结果，成功或失败的原因。</returns>
        public Result TryFinished()
        {
            // 更新最后一次活跃时间
            this.LastActiveTime = DateTime.UtcNow;

            // 如果是读取模式，则直接返回成功
            if (this.FileAccess == FileAccess.Read)
            {
                return Result.Success;
            }

            try
            {
                // 检查是否有未完成的文件块
                if (this.GetUnfinishedFileSection().Length > 0)
                {
                    return new Result(ResultCode.Fail, "还有文件块没有完成。");
                }

                // 确保文件长度一致
                if (this.FileStorage.Length != this.FileResourceInfo.FileInfo.Length)
                {
                    return new Result(ResultCode.Fail, "文件长度不一致。");
                }

                // 尝试释放文件，最多尝试10次
                for (var i = 0; i < 10; i++)
                {
                    if (this.FileStorage.TryReleaseFile().IsSuccess)
                    {
                        break;
                    }
                }

                // 确保文件移动并重命名，最多尝试10次
                for (var i = 0; i < 10; i++)
                {
                    File.Move(this.LocatorPath + ExtensionName, this.LocatorPath);
                    if (new FileInfo(this.LocatorPath).Length == this.FileResourceInfo.FileInfo.Length)
                    {
                        break;
                    }
                }

                // 所有检查通过，返回成功
                return Result.Success;
            }
            catch (Exception ex)
            {
                // 异常情况下，返回异常信息
                return new Result(ex);
            }
        }

        /// <summary>
        /// 写入文件块
        /// </summary>
        /// <param name="fileSection">要写入的文件块信息</param>
        /// <param name="value">要写入的字节数据</param>
        /// <returns>写入操作的结果</returns>
        public Result WriteFileSection(FileSection fileSection, ArraySegment<byte> value)
        {
            // 更新最后一次活动时间，用于跟踪文件操作的时间点
            this.LastActiveTime = DateTime.UtcNow;

            // 检查当前文件访问模式是否为写，确保操作的正确性
            if (this.FileAccess != FileAccess.Write)
            {
                return new Result(ResultCode.Error, "该定位器是只读的。");
            }

            // 验证文件块索引是否超出文件资源信息的范围
            if (fileSection.Index >= this.FileResourceInfo.FileSections.Length)
            {
                fileSection.Status = FileSectionStatus.Fail;
                return new Result(ResultCode.Error, "实际数据块索引超出，可能写入的文件已经发生变化。");
            }

            // 获取对应索引处的源文件块信息
            var srcFileSection = this.FileResourceInfo.FileSections[fileSection.Index];

            // 检查传入的数据长度是否与文件块期望的长度一致
            if (value.Count != fileSection.Length)
            {
                fileSection.Status = FileSectionStatus.Fail;
                return new Result(ResultCode.Error, "实际数据长度与期望长度不一致。");
            }

            // 比较传入的文件块信息与源文件块是否一致，确保数据的一致性
            if (!srcFileSection.Equals(fileSection))
            {
                return new Result(ResultCode.Error, "数据块不一致。");
            }

            // 实际写入文件操作
            this.FileStorage.Write(srcFileSection.Offset, value.Array, value.Offset, value.Count);

            // 确保数据写入磁盘
            this.FileStorage.Flush();

            // 更新文件块状态为完成
            fileSection.Status = FileSectionStatus.Finished;
            srcFileSection.Status = FileSectionStatus.Finished;

            // 返回写入成功结果
            return Result.Success;
        }

        /// <summary>
        /// 写入文件块
        /// </summary>
        /// <param name="fileSectionResult">包含文件部分和值的结果对象</param>
        /// <returns>写入操作的结果</returns>
        public Result WriteFileSection(FileSectionResult fileSectionResult)
        {
            // 更新最后一次活跃时间，用于跟踪最近的访问时间
            this.LastActiveTime = DateTime.UtcNow;

            // 检查文件访问权限是否为写，如果是读-only，则返回错误结果
            if (this.FileAccess != FileAccess.Write)
            {
                return new Result(ResultCode.Error, "该定位器是只读的。");
            }
            else
            {
                // 如果文件部分的结果不成功，则返回该结果
                if (!fileSectionResult.IsSuccess)
                {
                    return new Result(fileSectionResult);
                }
                else
                {
                    // 调用内部方法写入文件部分和值
                    return this.WriteFileSection(fileSectionResult.FileSection, fileSectionResult.Value.Memory.GetArray());
                }
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.FileStorage.TryReleaseFile();
            base.Dispose(disposing);
        }
    }
}