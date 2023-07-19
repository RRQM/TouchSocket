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
using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// FileSectionResult
    /// </summary>
    public class FileSectionResult : ResultBase
    {
        /// <summary>
        /// FileSectionResult
        /// </summary>
        /// <param name="resultCode"></param>
        /// <param name="value"></param>
        /// <param name="fileSection"></param>
        public FileSectionResult(ResultCode resultCode, byte[] value, FileSection fileSection) : base(resultCode)
        {
            this.Value = value;
            this.FileSection = fileSection;
        }

        /// <summary>
        /// FileSectionResult
        /// </summary>
        /// <param name="resultCode"></param>
        /// <param name="message"></param>
        /// <param name="value"></param>
        /// <param name="fileSection"></param>
        public FileSectionResult(ResultCode resultCode, string message, byte[] value, FileSection fileSection) : base(resultCode, message)
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
        public byte[] Value { get; private set; }
    }
}