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

using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.SerialPorts
{

    /// <summary>
    /// 定义了串行接收插件的接口。
    /// 继承自通用插件接口IPlugin，专门用于处理串行端口接收操作。
    /// </summary>
    [DynamicMethod]
    public interface ISerialReceivingPlugin : IPlugin
    {
        /// <summary>
        /// 当串行端口接收到数据时触发的异步事件处理方法。
        /// </summary>
        /// <param name="client">触发事件的串行端口会话对象。</param>
        /// <param name="e">包含接收数据的事件参数对象。</param>
        /// <returns>一个Task对象，代表异步操作的结果。</returns>
        Task OnSerialReceiving(ISerialPortSession client, ByteBlockEventArgs e);
    }
}