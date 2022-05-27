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
using RRQMCore.ByteManager;
using System;

namespace RRQMSocket
{
    /// <summary>
    /// 状态
    /// </summary>
    public enum ChannelStatus : byte
    {
        /// <summary>
        /// 默认
        /// </summary>
        Default = 0,

        /// <summary>
        /// 继续下移
        /// </summary>
        Moving = 1,

        /// <summary>
        /// 超时
        /// </summary>
        Overtime = 2,

        /// <summary>
        /// 继续
        /// </summary>
        HoldOn = 3,

        /// <summary>
        /// 取消
        /// </summary>
        Cancel = 4,

        /// <summary>
        /// 完成
        /// </summary>
        Completed = 5,

        /// <summary>
        /// 已释放
        /// </summary>
        Disposed = 6
    }

    /// <summary>
    /// 清理统计类型
    /// </summary>
    [Flags]
    public enum ClearType
    {
        /// <summary>
        /// 从发送统计
        /// </summary>
        Send = 1,

        /// <summary>
        /// 从接收统计
        /// </summary>
        Receive = 2
    }

    /// <summary>
    /// 过滤结果
    /// </summary>
    public enum FilterResult
    {
        /// <summary>
        /// 缓存后续所有<see cref="ByteBlock.CanReadLen"/>数据。
        /// </summary>
        Cache,

        /// <summary>
        /// 操作成功
        /// </summary>
        Success,

        /// <summary>
        /// 继续操作，一般原因是本次数据有部分无效，但已经调整了<see cref="ByteBlock.Pos"/>属性，所以继续后续解析。
        /// <para>或者想放弃当前数据的操作，直接设置<see cref="ByteBlock.Pos"/>与<see cref="ByteBlock.Len"/>相等即可。</para>
        /// </summary>
        GoOn
    }

    /// <summary>
    /// 固定包头类型
    /// </summary>
    public enum FixedHeaderType : byte
    {
        /// <summary>
        /// 以1Byte标识长度，最长接收255
        /// </summary>
        Byte = 1,

        /// <summary>
        /// 以2Byte标识长度，最长接收65535
        /// </summary>
        Ushort = 2,

        /// <summary>
        /// 以4Byte标识长度，最长接收2147483647
        /// </summary>
        Int = 4
    }

    /// <summary>
    /// 接收类型
    /// </summary>
    public enum ReceiveType : byte
    {
        /// <summary>
        /// 该模式下会自动接收数据，然后主动触发。
        /// </summary>
        Auto,

        /// <summary>
        /// 在该模式下，不会投递接收申请，用户可通过<see cref="ITcpClientBase.GetStream"/>，获取到流以后，自己处理接收。
        /// <para>注意：连接端不会感知主动断开</para>
        /// </summary>
        None
    }

    /// <summary>
    /// 服务器状态
    /// </summary>
    public enum ServerState
    {
        /// <summary>
        /// 无状态，指示为初建
        /// </summary>
        None,

        /// <summary>
        /// 正在运行
        /// </summary>
        Running,

        /// <summary>
        /// 已停止
        /// </summary>
        Stopped,

        /// <summary>
        /// 已释放
        /// </summary>
        Disposed
    }

    /// <summary>
    /// 转发工作模式
    /// </summary>
    public enum NATMode
    {
        /// <summary>
        /// 双向转发
        /// </summary>
        TwoWay,

        /// <summary>
        /// 仅由监听地址向目标地址单向转发
        /// </summary>
        OneWay,

        /// <summary>
        /// 仅由目标地址向监听地址单向转发
        /// </summary>
        OneWayToListen
    }
}
