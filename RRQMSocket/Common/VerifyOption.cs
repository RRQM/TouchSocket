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

namespace RRQMSocket
{
    /// <summary>
    /// Token连接验证
    /// </summary>
    public class VerifyOption
    {
        /// <summary>
        /// 令箭
        /// </summary>
        public string Token { get; internal set; }

        /// <summary>
        /// 是否接受
        /// </summary>
        public bool Accept { get; set; }

        /// <summary>
        /// 不接受时，返回客户端信息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 标记，会同步至TcpSocketClient
        /// </summary>
        public object Flag { get; set; }
    }
}