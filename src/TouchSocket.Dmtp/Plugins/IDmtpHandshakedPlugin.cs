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

namespace TouchSocket.Dmtp;

/// <summary>
/// 定义了一个插件接口，该插件在完成与Dmtp的握手连接后需要被调用。
/// </summary>
[DynamicMethod]
public interface IDmtpHandshakedPlugin : IPlugin
{
    /// <summary>
    /// 在完成握手连接时被调用的方法。
    /// </summary>
    /// <param name="client">参与握手的Dmtp客户端对象。</param>
    /// <param name="e">握手验证事件参数。</param>
    /// <returns>一个Task对象，表示异步操作的结果。</returns>
    Task OnDmtpHandshaked(IDmtpActorObject client, DmtpVerifyEventArgs e);
}