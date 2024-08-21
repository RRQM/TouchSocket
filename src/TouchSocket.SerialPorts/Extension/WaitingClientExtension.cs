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

using TouchSocket.Sockets;

namespace TouchSocket.SerialPorts
{
    /// <summary>
    /// 提供扩展方法以简化创建等待客户端的代码。
    /// </summary>
    public static class WaitingClientExtension
    {
        /// <summary>
        /// 创建一个等待客户端，用于处理串口通信中的接收操作。
        /// </summary>
        /// <param name="client">发起请求的串口客户端。</param>
        /// <param name="waitingOptions">等待选项，配置等待行为。</param>
        /// <returns>返回一个具备特定等待行为的串口客户端实例。</returns>
        public static IWaitingClient<ISerialPortClient, IReceiverResult> CreateWaitingClient(this ISerialPortClient client, WaitingOptions waitingOptions)
        {
            // 使用给定的等待选项创建一个等待客户端实例
            return client.CreateWaitingClient<ISerialPortClient, IReceiverResult>(waitingOptions);
        }
    }
}