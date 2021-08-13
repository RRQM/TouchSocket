//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Dependency;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件服务器配置
    /// </summary>
    public class FileServiceConfig : TokenServiceConfig
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public FileServiceConfig()
        {
            this.BufferLength = 64 * 1024;
        }

        /// <summary>
        /// 最大下载速度
        /// </summary>
        public long MaxDownloadSpeed
        {
            get { return (long)GetValue(MaxDownloadSpeedProperty); }
            set { SetValue(MaxDownloadSpeedProperty, value); }
        }

        /// <summary>
        /// 最大下载速度, 所需类型<see cref="long"/>
        /// </summary>
        public static readonly DependencyProperty MaxDownloadSpeedProperty =
            DependencyProperty.Register("MaxDownloadSpeed", typeof(long), typeof(FileServiceConfig), 1024 * 1024L);

        /// <summary>
        /// 最大上传速度
        /// </summary>
        public long MaxUploadSpeed
        {
            get { return (long)GetValue(MaxUploadSpeedProperty); }
            set { SetValue(MaxUploadSpeedProperty, value); }
        }

        /// <summary>
        /// 最大上传速度, 所需类型<see cref="long"/>
        /// </summary>
        public static readonly DependencyProperty MaxUploadSpeedProperty =
            DependencyProperty.Register("MaxUploadSpeed", typeof(long), typeof(FileServiceConfig), 1024 * 1024L);
    }
}