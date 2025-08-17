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

using System.Collections.Generic;

namespace TouchSocket.Core;

/// <summary>
/// 元数据键值对。
/// </summary>
[FastConverter(typeof(MetadataFastBinaryConverter))]
public sealed class Metadata : Dictionary<string, string>, IPackage
{
    /// <summary>
    /// 获取或设置指定键的值。
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public new string this[string key]
    {
        get
        {
            ThrowHelper.ThrowArgumentNullExceptionIf(key, nameof(key));
            return this.TryGetValue(key, out var value) ? value : null;
        }
        set
        {
            ThrowHelper.ThrowArgumentNullExceptionIf(key, nameof(key));
            base[key] = value;
        }
    }

    /// <summary>
    /// 向元数据集合添加一个键值对。如果键已经存在，则覆盖其值。
    /// </summary>
    /// <param name="name">要添加的键。</param>
    /// <param name="value">与键关联的值。</param>
    /// <returns>返回当前元数据对象，以支持链式调用。</returns>
    public new Metadata Add(string name, string value)
    {
        base[name] = value;
        return this;
    }


    /// <inheritdoc/>
    public void Package<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter

    {
        WriterExtension.WriteValue<TWriter, int>(ref writer, this.Count);
        foreach (var item in this)
        {
            WriterExtension.WriteString(ref writer, item.Key, FixedHeaderType.Byte);
            WriterExtension.WriteString(ref writer, item.Value, FixedHeaderType.Byte);
        }
    }

    /// <inheritdoc/>
    public void Unpackage<TReader>(ref TReader reader)
        where TReader : IBytesReader
#if AllowsRefStruct
, allows ref struct
#endif
    {
        var count = ReaderExtension.ReadValue<TReader, int>(ref reader);
        for (var i = 0; i < count; i++)
        {
            var key = ReaderExtension.ReadString(ref reader, FixedHeaderType.Byte);
            var value = ReaderExtension.ReadString(ref reader, FixedHeaderType.Byte);
            this.Add(key, value);
        }
    }


}