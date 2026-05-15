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

internal class SocketIoUtility
{
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
}