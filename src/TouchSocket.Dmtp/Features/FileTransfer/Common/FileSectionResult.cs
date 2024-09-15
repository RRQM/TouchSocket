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
using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// 文件分段上传结果类，继承自ResultBase，实现IDisposable接口
    /// </summary>
    public class FileSectionResult : ResultBase, IDisposable
    {

        /// <summary>
        /// 构造函数：初始化FileSectionResult对象，用于处理文件段结果。
        /// </summary>
        /// <param name="resultCode">结果代码，表示操作的执行情况。</param>
        /// <param name="value">字节块数据，表示处理的结果值。</param>
        /// <param name="fileSection">文件段信息，表示操作涉及的文件段。</param>
        public FileSectionResult(ResultCode resultCode, ByteBlock value, FileSection fileSection) : base(resultCode)
        {
            this.Value = value;
            this.FileSection = fileSection;
        }

        /// <summary>
        /// 构造函数：初始化FileSectionResult对象，用于处理文件段结果，包括错误信息。
        /// </summary>
        /// <param name="resultCode">结果代码，表示操作的执行情况。</param>
        /// <param name="message">错误消息，提供操作失败的详细信息。</param>
        /// <param name="value">字节块数据，表示处理的结果值。</param>
        /// <param name="fileSection">文件段信息，表示操作涉及的文件段。</param>
        public FileSectionResult(ResultCode resultCode, string message, ByteBlock value, FileSection fileSection) : base(resultCode, message)
        {
            this.Value = value;
            this.FileSection = fileSection;
        }

        /// <summary>
        /// 文件块
        /// </summary>
        public FileSection FileSection { get; private set; }

        /// <summary>
        /// 实际数据
        /// </summary>
        public ByteBlock Value { get; private set; }

        /// <summary>
        /// 释放当前对象持有的资源。
        /// </summary>
        public void Dispose()
        {
            // 释放当前对象持有的资源
            this.Value.Dispose();
        }
    }
}