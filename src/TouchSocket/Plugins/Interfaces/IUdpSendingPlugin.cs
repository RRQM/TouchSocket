using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 定义了一个UDP发送插件接口，继承自IPlugin接口。
    /// 该接口为实现UDP数据发送功能的插件提供了一套标准的方法和属性。
    /// </summary>
    public interface IUdpSendingPlugin : IPlugin
    {
        /// <summary>
        /// 当即将发送数据时，调用该方法在适配器之后，接下来即会发送数据。
        /// 此方法用于在数据发送前执行额外的处理或检查。
        /// </summary>
        /// <param name="client">正在与之通信的UDP会话客户端。</param>
        /// <param name="e">包含发送事件相关数据的参数对象。</param>
        /// <returns>一个Task对象，代表异步操作的结果。</returns>
        Task OnUdpSending(IUdpSessionBase client, UdpSendingEventArgs e);
    }
}
