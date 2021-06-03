using RRQMCore.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件服务器配置
    /// </summary>
    public class FileServiceConfig:TokenServerConfig
    {
        /// <summary>
        /// 是否支持断点续传
        /// </summary>
        public bool BreakpointResume
        {
            get { return (bool)GetValue(BreakpointResumeProperty); }
            set { SetValue(BreakpointResumeProperty, value); }
        }

        /// <summary>
        /// 是否支持断点续传
        /// </summary>
        public static readonly DependencyProperty BreakpointResumeProperty =
            DependencyProperty.Register("BreakpointResume", typeof(bool), typeof(FileServiceConfig), false);


        /// <summary>
        /// 最大下载速度
        /// </summary>
        public long MaxDownloadSpeed
        {
            get { return (long)GetValue(MaxDownloadSpeedProperty); }
            set { SetValue(MaxDownloadSpeedProperty, value); }
        }

        /// <summary>
        /// 最大下载速度
        /// </summary>
        public static readonly DependencyProperty MaxDownloadSpeedProperty =
            DependencyProperty.Register("MaxDownloadSpeed", typeof(long), typeof(FileServiceConfig), 1024*1024L);


        /// <summary>
        /// 最大上传速度
        /// </summary>
        public long MaxUploadSpeed
        {
            get { return (long)GetValue(MaxUploadSpeedProperty); }
            set { SetValue(MaxUploadSpeedProperty, value); }
        }

        /// <summary>
        /// 最大上传速度
        /// </summary>
        public static readonly DependencyProperty MaxUploadSpeedProperty =
            DependencyProperty.Register("MaxUploadSpeed", typeof(long), typeof(FileServiceConfig), 1024 * 1024L);




    }
}
