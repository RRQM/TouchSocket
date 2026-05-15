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

public static class EngineIoUtility
{
    /// <summary>
    /// 将 EIO3 轮询响应体按 <c>{length}:{packet}</c> 格式拆分为多个数据包。
    /// </summary>
    public static string[] SplitEIO3(string value)
    {
        var list = new List<string>();
        var startIndex = 0;
        while (true)
        {
            var index = value.IndexOf(':', startIndex);
            if (index == -1)
            {
                break;
            }
            if (int.TryParse(value.Substring(startIndex, index - startIndex), out var length))
            {
                var msg = value.Substring(index + 1, length);
                list.Add(msg);
            }
            else
            {
                break;
            }
            startIndex = index + length + 1;
            if (startIndex >= value.Length)
            {
                break;
            }
        }
        return list.ToArray();
    }

    /// <summary>
    /// 将 EIO3 轮询响应体（UTF-8 字节）按 <c>{length}:{packet}</c> 格式拆分为多个数据包。
    /// </summary>
    public static List<ReadOnlyMemory<byte>> SplitEIO3(ReadOnlyMemory<byte> value)
    {
        var list = new List<ReadOnlyMemory<byte>>();
        var startIndex = 0;
        while (startIndex < value.Length)
        {
            var remaining = value.Slice(startIndex).Span;
            var colonIndex = remaining.IndexOf((byte)':');
            if (colonIndex == -1)
            {
                break;
            }
            var length = 0;
            var valid = true;
            for (var i = 0; i < colonIndex; i++)
            {
                var b = remaining[i];
                if (b < '0' || b > '9') { valid = false; break; }
                length = length * 10 + (b - '0');
            }
            if (!valid)
            {
                break;
            }
            var dataStart = startIndex + colonIndex + 1;
            if (dataStart + length > value.Length)
            {
                break;
            }
            list.Add(value.Slice(dataStart, length));
            startIndex = dataStart + length;
        }
        return list;
    }

    public static EngineIoVersion ParserEIO(string value)
    {
        return string.IsNullOrEmpty(value)
            ? throw new ArgumentException($"“{nameof(value)}”不能为 null 或空。", nameof(value))
            : value.Equals("3", StringComparison.OrdinalIgnoreCase)
            ? EngineIoVersion.V3
            : value.Equals("4", StringComparison.OrdinalIgnoreCase) ? EngineIoVersion.V4 : throw new Exception("未能识别的EIO版本");
    }

    public static EngineIoTransportType ParserTransport(string value)
    {
        return string.IsNullOrEmpty(value)
            ? throw new ArgumentException($"“{nameof(value)}”不能为 null 或空。", nameof(value))
            : value.Equals("polling", StringComparison.OrdinalIgnoreCase)
            ? EngineIoTransportType.Polling
            : value.Equals("websocket", StringComparison.OrdinalIgnoreCase)
                ? EngineIoTransportType.WebSocket
                : throw new Exception("未能识别的通信方式");
    }
}