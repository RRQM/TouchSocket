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
    /// FileTransferingEventArgs
    /// </summary>
    public class FileTransferingEventArgs : MsgPermitEventArgs
    {
        /// <summary>
        /// FileTransferingEventArgs
        /// </summary>
        /// <param name="transferType"></param>
        /// <param name="metadata"></param>
        /// <param name="fileInfo"></param>
        public FileTransferingEventArgs(TransferType transferType, Metadata metadata, RemoteFileInfo fileInfo)
        {
            this.TransferType = transferType;
            this.FileInfo = fileInfo;
            this.Metadata = metadata;
        }

        /// <summary>
        /// 存放路径，
        /// 可输入绝对路径，也可以输入相对路径。
        /// 但是必须包含文件名及扩展名。
        /// </summary>
        public string SavePath { get; set; }

        /// <summary>
        /// 请求文件路径，
        /// 可输入绝对路径，也可以输入相对路径。
        /// </summary>
        public string ResourcePath { get; set; }

        /// <summary>
        /// 元数据
        /// </summary>
        public Metadata Metadata { get; private set; }

        /// <summary>
        /// 文件信息
        /// </summary>
        public RemoteFileInfo FileInfo { get; private set; }

        /// <summary>
        /// 传输类型
        /// </summary>
        public TransferType TransferType { get; private set; }
    }
}