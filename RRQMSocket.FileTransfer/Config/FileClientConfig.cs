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
using RRQMCore.Dependency;
using RRQMSocket.RPC.RRQMRPC;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件客户端配置
    /// </summary>
    public class FileClientConfig : TcpRpcClientConfig
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public FileClientConfig()
        {
            this.BufferLength = 64 * 1024;
        }

        /// <summary>
        /// 根目录
        /// </summary>
        public string RootPath
        {
            get { return (string)GetValue(RootPathProperty); }
            set { SetValue(RootPathProperty, value); }
        }

        /// <summary>
        /// 下载根目录，
        /// 所需类型<see cref="string"/>
        /// </summary>
        public static readonly DependencyProperty RootPathProperty =
            DependencyProperty.Register("RootPath", typeof(string), typeof(FileClientConfig), string.Empty);

        /// <summary>
        /// 允许的响应类型
        /// </summary>
        public ResponseType ResponseType
        {
            get { return (ResponseType)GetValue(ResponseTypeProperty); }
            set { SetValue(ResponseTypeProperty, value); }
        }

        /// <summary>
        /// 允许的响应类型,
        ///  所需类型<see cref="RRQMSocket.FileTransfer.ResponseType"/>
        /// </summary>
        public static readonly DependencyProperty ResponseTypeProperty =
            DependencyProperty.Register("ResponseType", typeof(ResponseType), typeof(FileClientConfig), ResponseType.Both);
    }
}