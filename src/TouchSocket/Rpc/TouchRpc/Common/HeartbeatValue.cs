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
namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// 心跳参数
    /// </summary>
    public class HeartbeatValue
    {
        /// <summary>
        /// 发送间隔，默认1000ms
        /// </summary>
        public int Interval { get; set; } = 1000;

        /// <summary>
        /// 最大失败次数，默认5。
        /// </summary>
        public int MaxFailCount { get; set; } = 5;
    }
}