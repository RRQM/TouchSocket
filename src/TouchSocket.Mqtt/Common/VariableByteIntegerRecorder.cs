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

using System;
using TouchSocket.Core;

namespace TouchSocket.Mqtt;

internal ref struct VariableByteIntegerRecorder
{
    private int m_dataPosition;
    private int m_minimumCount;
    private int m_startPosition;

    public void CheckOut(ref BytesWriter writer, int minimum = 0)
#if AllowsRefStruct
,allows ref struct
#endif
    {
        this.m_minimumCount = MqttExtension.GetVariableByteIntegerCount(minimum);
        this.m_startPosition = writer.Position;
        writer.Position += this.m_minimumCount;
        this.m_dataPosition = writer.Position;
    }

    public readonly int CheckIn(ref BytesWriter writer)
#if AllowsRefStruct
,allows ref struct
#endif
    {
        var endPosition = writer.Position;

        var len = endPosition - this.m_dataPosition;
        var lenCount = MqttExtension.GetVariableByteIntegerCount((int)len);
        if (lenCount > this.m_minimumCount)
        {
            var moveCount = lenCount - this.m_minimumCount;
            var span = writer.TotalMemory.Span.Slice(this.m_dataPosition);
            ShiftWithRight(span, moveCount);

            writer.Position = this.m_startPosition;
            MqttExtension.WriteVariableByteInteger(ref writer, (uint)len);
            writer.Position=(endPosition+ moveCount);
        }
        else
        {
            writer.Position = this.m_startPosition;
            MqttExtension.WriteVariableByteInteger(ref writer, (uint)len);
            writer.Position = endPosition;
        }
        return len;
    }

    private static void ShiftWithRight(Span<byte> span, int shiftCount)
    {
        var length = span.Length;
        for (var i = length - 1; i >= shiftCount; i--)
        {
            span[i] = span[i - shiftCount];
        }
    }
}

//internal ref struct VariableByteIntegerRecorder
//{
//    private int m_dataPosition;
//    private int m_minimumCount;
//    private int m_startPosition;

//    public void CheckOut<TWriter>(ref TWriter writer, int minimum = 0)
//        where TWriter : IByteBlockWriter
//#if AllowsRefStruct
//,allows ref struct
//#endif
//    {
//        this.m_minimumCount = MqttExtension.GetVariableByteIntegerCount(minimum);
//        this.m_startPosition = writer.Position;
//        writer.Position += this.m_minimumCount;
//        this.m_dataPosition = writer.Position;
//    }

//    public readonly int CheckIn<TWriter>(ref TWriter writer)
//        where TWriter : IByteBlockWriter
//#if AllowsRefStruct
//,allows ref struct
//#endif
//    {
//        var endPosition = writer.Position;

//        var len = endPosition - this.m_dataPosition;
//        var lenCount = MqttExtension.GetVariableByteIntegerCount((int)len);
//        if (lenCount > this.m_minimumCount)
//        {
//            var moveCount = lenCount - this.m_minimumCount;
//            writer.ExtendSize(moveCount);
//            var span = writer.TotalMemory.Span.Slice(this.m_dataPosition);
//            ShiftWithRight(span, moveCount);

//            writer.Position = this.m_startPosition;
//            MqttExtension.WriteVariableByteInteger(ref writer, (uint)len);
//            writer.SetLength(endPosition + moveCount);
//            writer.SeekToEnd();
//        }
//        else
//        {
//            writer.Position = this.m_startPosition;
//            MqttExtension.WriteVariableByteInteger(ref writer, (uint)len);
//            writer.SeekToEnd();
//        }
//        return len;
//    }

//    private static void ShiftWithRight(Span<byte> span, int shiftCount)
//    {
//        var length = span.Length;
//        for (var i = length - 1; i >= shiftCount; i--)
//        {
//            span[i] = span[i - shiftCount];
//        }
//    }
//}