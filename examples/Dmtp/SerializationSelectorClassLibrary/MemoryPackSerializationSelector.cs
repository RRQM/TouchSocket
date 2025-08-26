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

using MemoryPack;
using System;
using TouchSocket.Core;
using TouchSocket.Dmtp.Rpc;

namespace SerializationSelectorClassLibrary;

public class MemoryPackSerializationSelector : ISerializationSelector
{
    public object DeserializeParameter<TByteBlock>(ref TByteBlock byteBlock, SerializationType serializationType, Type parameterType) where TByteBlock : IBytesReader
    {
        var len = ReaderExtension.ReadValue<TByteBlock, int>(ref byteBlock);
        var span = ReaderExtension.ReadToSpan(ref byteBlock, len);
        return MemoryPackSerializer.Deserialize(parameterType, span);
    }

    public void SerializeParameter<TByteBlock>(ref TByteBlock byteBlock, SerializationType serializationType, in object parameter) where TByteBlock : IBytesWriter
    {
        var writerAnchor = new WriterAnchor<TByteBlock>(ref byteBlock, 4);

        var memoryPackWriter = new MemoryPackWriter<TByteBlock>(ref byteBlock, null);

        MemoryPackSerializer.Serialize(parameter.GetType(), ref memoryPackWriter, parameter);

        var span = writerAnchor.Rewind(ref byteBlock, out var len);
        span.WriteValue(memoryPackWriter.WrittenCount);
    }
}

internal sealed class MyDefaultSerializationSelector : DefaultSerializationSelector
{
    public override object DeserializeParameter<TReader>(ref TReader reader, SerializationType serializationType, Type parameterType)
    {
        if ((byte)serializationType == 4)
        {
            var len = ReaderExtension.ReadValue<TReader, int>(ref reader);
            var span = ReaderExtension.ReadToSpan(ref reader, len);
            return MemoryPackSerializer.Deserialize(parameterType, span);
        }
        return base.DeserializeParameter(ref reader, serializationType, parameterType);
    }

    public override void SerializeParameter<TWriter>(ref TWriter writer, SerializationType serializationType, in object parameter)
    {
        if ((byte)serializationType == 4)
        {
            var writerAnchor = new WriterAnchor<TWriter>(ref writer, 4);

            var memoryPackWriter = new MemoryPackWriter<TWriter>(ref writer, null);

            MemoryPackSerializer.Serialize(parameter.GetType(), ref memoryPackWriter, parameter);

            var span = writerAnchor.Rewind(ref writer, out var len);
            span.WriteValue(memoryPackWriter.WrittenCount);
        }
        base.SerializeParameter(ref writer, serializationType, parameter);
    }
}