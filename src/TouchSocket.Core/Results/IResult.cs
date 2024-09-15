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

namespace TouchSocket.Core
{
    /// <summary>
    /// 表示结果的接口
    /// </summary>
    public interface IResult
    {
        /// <summary>
        /// 结果代码
        /// </summary>
        ResultCode ResultCode { get; }

        /// <summary>
        /// 结果附加消息
        /// </summary>
        string Message { get; }

        /// <summary>
        /// 是否成功。一般的当<see cref="ResultCode"/>为<see cref="ResultCode.Success"/>时会返回<see langword="true"/>。其余情况返回<see langword="false"/>
        /// </summary>
        bool IsSuccess { get; }
    }
}