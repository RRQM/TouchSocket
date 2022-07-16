//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在TouchSocket.Core.XREF命名空间的代码）归作者本人若汝棋茗所有
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

namespace TouchSocket.Core
{
    /// <summary>
    /// 结果类型
    /// </summary>
    public enum ResultCode
    {
        /// <summary>
        /// 未执行的
        /// </summary>
        Default,

        /// <summary>
        /// 错误
        /// </summary>
        Error,

        /// <summary>
        /// 成功
        /// </summary>
        Success,

        /// <summary>
        /// 操作超时
        /// </summary>
        Overtime,

        /// <summary>
        /// 操作取消
        /// </summary>
        Canceled
    }
}