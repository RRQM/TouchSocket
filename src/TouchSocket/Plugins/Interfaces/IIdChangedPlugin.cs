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

namespace TouchSocket.Sockets;


/// <summary>
/// 定义了一个插件接口IIdChangedPlugin，继承自IPlugin。
/// 该接口用于通知实现该接口的插件，某个ID发生了更改。
/// </summary>
[DynamicMethod]
public interface IIdChangedPlugin : IPlugin
{

    /// <summary>
    /// 当客户端ID发生变化时触发的异步事件处理程序。
    /// </summary>
    /// <param name="client">发生ID变更的客户端对象。</param>
    /// <param name="e">包含ID变更详细信息的事件参数。</param>
    /// <remarks>
    /// 该方法用于异步处理客户端ID的变更，当客户端的ID发生变化时会触发此事件。
    /// 通过此事件，系统可以相应地更新与客户端相关联的数据或状态。
    /// </remarks>
    Task OnIdChanged(IClient client, IdChangedEventArgs e);
}