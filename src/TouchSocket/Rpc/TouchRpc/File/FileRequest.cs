//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;

namespace TouchSocket.Rpc.TouchRpc
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
        public FileRequest(string path, string savePath)
        {
            this.SavePath = savePath ?? throw new ArgumentNullException(nameof(savePath));
            this.Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="path"></param>
        public FileRequest(string path) : this(path, System.IO.Path.GetFileName(path))
        {
        }

        /// <summary>
        /// 传输标识
        /// </summary>
        public TransferFlags Flags { get; set; }

        /// <summary>
        /// 存放路径，
        /// 可输入绝对路径，也可以输入相对路径。
        /// 但是必须包含文件名及扩展名。
        /// </summary>
        public string SavePath { get; set; }

        /// <summary>
        /// 文件源路径，
        /// 可输入绝对路径，也可以输入相对路径。
        /// </summary>
        public string Path { get; set; }
    }
}