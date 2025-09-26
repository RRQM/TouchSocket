// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

namespace TouchSocket.SerialPorts;
/// <summary>
/// 提供扩展方法以支持串口插件管理功能。
/// </summary>
public static class SerialPortPluginManagerExtension
{
    /// <summary>
    /// 使用<see cref="ISerialPortSession"/>检查连接客户端活性插件。
    /// <para>当在设置的周期内，没有接收/发送任何数据，则判定该客户端掉线。执行清理。默认配置：60秒为一个周期，同时检测发送和接收。</para>
    /// 服务器、客户端均适用。
    /// </summary>
    /// <param name="pluginManager">插件管理器对象，用于管理插件。</param>
    /// <returns>返回一个<see cref="CheckClearPlugin{TClient}"/>类型的插件实例，用于执行客户端活性检查及清理操作。</returns>
    public static CheckClearPlugin<ISerialPortSession> UseSerialPortSessionCheckClear(this IPluginManager pluginManager)
    {
        return pluginManager.UseCheckClear<ISerialPortSession>();
    }
}
