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