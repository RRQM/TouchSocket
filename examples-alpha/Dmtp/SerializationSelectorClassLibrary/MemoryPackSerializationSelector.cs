using MemoryPack;
using System;
using System.IO;
using TouchSocket.Core;
using TouchSocket.Dmtp.Rpc;

namespace SerializationSelectorClassLibrary
{
    public class MemoryPackSerializationSelector : ISerializationSelector
    {
        public object DeserializeParameter<TByteBlock>(ref TByteBlock byteBlock, SerializationType serializationType, Type parameterType) where TByteBlock : IByteBlock
        {
            var len = byteBlock.ReadInt32();
            var span = byteBlock.ReadToSpan(len);
            return MemoryPackSerializer.Deserialize(parameterType, span);
        }

        public void SerializeParameter<TByteBlock>(ref TByteBlock byteBlock, SerializationType serializationType, in object parameter) where TByteBlock : IByteBlock
        {
            var pos = byteBlock.Position;
            byteBlock.Seek(4, SeekOrigin.Current);
            var memoryPackWriter = new MemoryPack.MemoryPackWriter<TByteBlock>(ref byteBlock, null);

            MemoryPackSerializer.Serialize(parameter.GetType(), ref memoryPackWriter, parameter);

            var newPos = byteBlock.Position;
            byteBlock.Position = pos;
            byteBlock.WriteInt32(memoryPackWriter.WrittenCount);
            byteBlock.Position = newPos;
        }
    }
}