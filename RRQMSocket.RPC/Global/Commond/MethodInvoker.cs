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
        public object ReturnParameter;

        /// <summary>
        /// 参数值集合
        /// </summary>
        public object[] Parameters;

        /// <summary>
        /// 调用状态
        /// </summary>
        public InvokeStatus Status;

        /// <summary>
        /// 调用类型
        /// </summary>
        public InvokeType InvokeType;

        /// <summary>
        /// 自定义调用实例
        /// </summary>
        public IServerProvider CustomServerProvider;

        /// <summary>
        /// 状态消息
        /// </summary>
        public string StatusMessage;

        /// <summary>
        /// 可以传递其他类型的数据容器
        /// </summary>
        public object Flag;

        /// <summary>
        /// 此函数执行者
        /// </summary>
        public ICaller Caller;
    }
}