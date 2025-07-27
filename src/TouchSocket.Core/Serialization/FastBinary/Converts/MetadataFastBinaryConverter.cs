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

namespace TouchSocket.Core;

/// <summary>
/// MetadataFastBinaryConverter
/// </summary>
internal sealed class MetadataFastBinaryConverter : FastBinaryConverter<Metadata>
{
    protected override Metadata Read<TReader>(ref TReader reader, Type type)
    {
        var count = ReaderExtension.ReadValue<TReader, int>(ref reader);

        var metadata = new Metadata();
        for (var i = 0; i < count; i++)
        {
            var key = ReaderExtension.ReadString<TReader>(ref reader, FixedHeaderType.Ushort);
            var value = ReaderExtension.ReadString<TReader>(ref reader, FixedHeaderType.Ushort);
            metadata.Add(key, value);
        }
        return metadata;
    }

    protected override void Write<TWriter>(ref TWriter writer, in Metadata obj)
    {
        WriterExtension.WriteValue<TWriter, int>(ref writer, obj.Count);
        foreach (var item in obj)
        {
            WriterExtension.WriteString(ref writer, item.Key, FixedHeaderType.Ushort);
            WriterExtension.WriteString(ref writer, item.Value, FixedHeaderType.Ushort);
        }
    }
}