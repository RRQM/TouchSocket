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

namespace TouchSocket.Dmtp;


/// <summary>
/// IDmtpConnectingPlugin接口定义了插件在Dmtp握手过程中需要实现的方法。
/// 它继承自IPlugin接口。
/// </summary>
[DynamicMethod]
public interface IDmtpConnectingPlugin : IPlugin
{
    /// <summary>
    /// 在Dmtp建立握手连接之前执行的操作。
    /// 此方法允许插件在握手过程中进行自定义的验证或处理。
    /// </summary>
    /// <param name="client">正在与之建立握手连接的客户端对象。</param>
    /// <param name="e">包含验证过程中需要的信息的事件参数。</param>
    /// <returns>一个Task对象，表示异步操作的结果。</returns>
    Task OnDmtpConnecting(IDmtpActorObject client, DmtpVerifyEventArgs e);
}