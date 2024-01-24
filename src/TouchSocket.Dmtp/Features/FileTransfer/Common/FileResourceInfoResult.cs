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

using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// FileResourceInfoResult
    /// </summary>
    public class FileResourceInfoResult : ResultBase
    {
        /// <summary>
        /// 创建成功的资源结果
        /// </summary>
        /// <param name="fileResourceInfo"></param>
        public FileResourceInfoResult(FileResourceInfo fileResourceInfo) : base(Result.Success)
        {
            this.FileResourceInfo = fileResourceInfo;
        }

        /// <summary>
        /// 创建失败的资源结果
        /// </summary>
        /// <param name="result"></param>
        public FileResourceInfoResult(Result result) : base(result)
        {
        }

        /// <summary>
        /// 创建失败的资源结果
        /// </summary>
        /// <param name="message"></param>
        /// <param name="code"></param>
        public FileResourceInfoResult(string message, ResultCode code = ResultCode.Error) : base(code, message)
        {
        }

        /// <summary>
        /// 文件资源信息
        /// </summary>
        public FileResourceInfo FileResourceInfo { get; private set; }
    }
}