//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 函数调用信使
    /// </summary>
    public class MethodInvoker
    {
        /// <summary>
        /// 返回值
        /// </summary>
        public object ReturnParameter { get; set; }

        /// <summary>
        /// 参数值集合
        /// </summary>
        public object[] Parameters { get; set; }

        /// <summary>
        /// 获取调用状态
        /// </summary>
        public InvokeStatus Status { get; set; }

        /// <summary>
        /// 状态消息
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// 可以传递其他类型的数据容器
        /// </summary>
        public object Flag { get; set; }

        /// <summary>
        /// 此函数执行者
        /// </summary>
        public object Caller { get; set; }
    }
}