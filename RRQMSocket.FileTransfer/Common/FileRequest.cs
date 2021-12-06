using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 请求信息
    /// </summary>
    public class FileRequest
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public FileRequest()
        { 
        
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="path"></param>
        /// <param name="savePath"></param>
        public FileRequest(string path,string savePath)
        {
            this.SavePath = savePath ?? throw new ArgumentNullException(nameof(savePath));
            this.Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="path"></param>
        public FileRequest(string path):this(path,System.IO.Path.GetFileName(path))
        {
        }

        /// <summary>
        /// 传输标识
        /// </summary>
        public TransferFlags Flags { get; set; }

        /// <summary>
        /// 文件验证类型
        /// </summary>
        public FileCheckerType FileCheckerType { get; set; }

        /// <summary>
        /// 存放路径，
        /// 可输入绝对路径，也可以输入相对路径。
        /// 但是必须包含文件名及扩展名。
        /// </summary>
        public string SavePath { get; set; }

        /// <summary>
        /// 覆盖保存
        /// </summary>
        public bool Overwrite { get; set; }

        /// <summary>
        /// 文件路径，
        /// 可输入绝对路径，也可以输入相对路径。
        /// </summary>
        public string Path { get; set; }
    }
}
