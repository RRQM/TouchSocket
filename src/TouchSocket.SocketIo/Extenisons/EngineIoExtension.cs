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

namespace TouchSocket.SocketIo;

public static class EngineIoExtension
{
    public static bool UpgradeWebSocket(this IHandshakeMessage message)
    {
        if (message.Upgrades == null)
        {
            return false;
        }

        foreach (var upgrade in message.Upgrades)
        {
            if (upgrade.Equals("websocket", System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    public static void Validate(this IHandshakeMessage message)
    {
        if (message.Sid.IsNullOrEmpty())
        {
            throw new System.ArgumentNullException(nameof(message.Sid));
        }

        if (message.PingInterval < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(message.PingInterval));
        }

        if (message.PingTimeout < 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(message.PingTimeout));
        }
    }
}