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
using RRQMCore.Exceptions;
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// 具有验证功能的终端接口
    /// </summary>
    public interface ITokenClient :ITcpClient, ITokenClientBase
    {
        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="RRQMTokenVerifyException"></exception>
        /// <exception cref="RRQMTimeoutException"></exception>
        ITcpClient Connect(string verifyToken, CancellationToken token = default);
    }
}