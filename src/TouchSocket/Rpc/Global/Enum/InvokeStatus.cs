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

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 调用状态
    /// </summary>
    public enum InvokeStatus : byte
    {
        /// <summary>
        /// 就绪
        /// </summary>
        Ready,

        /// <summary>
        /// 未找到服务
        /// </summary>
        UnFound,

        /// <summary>
        /// 不可用
        /// </summary>
        UnEnable,

        /// <summary>
        /// 成功调用
        /// </summary>
        Success,

        /// <summary>
        /// 调用内部异常
        /// </summary>
        InvocationException,

        /// <summary>
        /// 其他异常
        /// </summary>
        Exception
    }
}