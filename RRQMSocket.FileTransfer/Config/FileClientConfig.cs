using RRQMCore.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件客户端配置
    /// </summary>
    public class FileClientConfig : TokenClientConfig
    {
        /// <summary>
        /// 默认接收文件的存放目录
        /// </summary>
        public string ReceiveDirectory
        {
            get { return (string)GetValue(ReceiveDirectoryProperty); }
            set 
            {
                if (value==null)
                {
                    value = string.Empty;
                }
                SetValue(ReceiveDirectoryProperty, value); 
            }
        }

        /// <summary>
        /// 默认接收文件的存放目录
        /// </summary>
        public static readonly DependencyProperty ReceiveDirectoryProperty =
            DependencyProperty.Register("ReceiveDirectory", typeof(string), typeof(FileClientConfig), string.Empty);


        /// <summary>
        /// 单次请求超时时间 min=5,max=60 单位：秒
        /// </summary>
        public int Timeout
        {
            get { return (int)GetValue(TimeoutProperty); }
            set 
            {
                value = value < 5 ? 5 : (value > 60 ? 60 : value);
                SetValue(TimeoutProperty, value);
            }
        }

        /// <summary>
        /// 单次请求超时时间 min=5,max=60 单位：秒
        /// </summary>
        public static readonly DependencyProperty TimeoutProperty =
            DependencyProperty.Register("Timeout", typeof(int), typeof(FileClientConfig), 5);


    }
}
