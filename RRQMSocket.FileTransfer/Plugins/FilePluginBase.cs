//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMSocket.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer.Plugins
{
    /// <summary>
    /// 文件插件基类
    /// </summary>
    public class FilePluginBase : ProtocolPluginBase, IFilePlugin
    {
        /// <summary>
        /// 当文件传输结束之后。并不意味着完成传输，请通过<see cref="FileTransferStatusEventArgs.Result"/>属性值进行判断。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnFileTransfered(IFileClientBase client, FileTransferStatusEventArgs e)
        {

        }

        /// <summary>
        /// 在文件传输即将进行时触发。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnFileTransfering(IFileClientBase client, FileOperationEventArgs e)
        {
            
        }

        void IFilePlugin.OnFileTransfered(IFileClientBase client, FileTransferStatusEventArgs e)
        {
            this.OnFileTransfered(client, e);
        }

        void IFilePlugin.OnFileTransfering(IFileClientBase client, FileOperationEventArgs e)
        {
            this.OnFileTransfering(client,e);
        }
    }
}
