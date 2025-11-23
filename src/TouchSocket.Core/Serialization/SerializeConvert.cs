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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace TouchSocket.Core;

/// <summary>
/// 高性能序列化器
/// </summary>


public static partial class SerializeConvert
{
#pragma warning disable SYSLIB0011 // 微软觉得不安全，不推荐使用

    #region 普通二进制序列化

    /// <summary>
    /// 普通二进制序列化对象
    /// </summary>
    /// <param name="obj">数据对象</param>
    /// <returns></returns>
    [RequiresUnreferencedCode("BinaryFormatter serialization is not trim compatible because the type of objects being processed cannot be statically discovered.")]
    public static byte[] BinarySerialize(in object obj)
    {
        using (var serializeStream = new MemoryStream())
        {
            var bf = new BinaryFormatter();
            bf.Serialize(serializeStream, obj);
            return serializeStream.ToArray();
        }
    }

    /// <summary>
    /// 二进制序列化对象至文件
    /// </summary>
    /// <param name="obj">数据对象</param>
    /// <param name="path">路径</param>
    [RequiresUnreferencedCode("BinaryFormatter serialization is not trim compatible because the type of objects being processed cannot be statically discovered.")]
    public static void BinarySerializeToFile(in object obj, string path)
    {
        using (var serializeStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            var bf = new BinaryFormatter();
            bf.Serialize(serializeStream, obj);
            serializeStream.Close();
        }
    }

    /// <summary>
    /// 二进制序列化对象
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="obj"></param>
    [RequiresUnreferencedCode("BinaryFormatter serialization is not trim compatible because the type of objects being processed cannot be statically discovered.")]
    public static void BinarySerialize(Stream stream, in object obj)
    {
        var bf = new BinaryFormatter();
        bf.Serialize(stream, obj);
    }

    #endregion 普通二进制序列化

    #region 普通二进制反序列化

    /// <summary>
    /// 从Byte[]中反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <param name="binder"></param>
    /// <returns></returns>
    [RequiresDynamicCode("BinaryFormatter serialization uses dynamic code generation, the type of objects being processed cannot be statically discovered.")]
    [RequiresUnreferencedCode("BinaryFormatter serialization is not trim compatible because the type of objects being processed cannot be statically discovered.")]
    public static T BinaryDeserialize<T>(byte[] data, int offset, int length, SerializationBinder binder = null)
    {
        using (var DeserializeStream = new MemoryStream(data, offset, length))
        {
            DeserializeStream.Position = 0;
            var bf = new BinaryFormatter();
            if (binder != null)
            {
                bf.Binder = binder;
            }
            return (T)bf.Deserialize(DeserializeStream);
        }
    }

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <param name="binder"></param>
    /// <returns></returns>
    [RequiresDynamicCode("BinaryFormatter serialization uses dynamic code generation, the type of objects being processed cannot be statically discovered.")]
    [RequiresUnreferencedCode("BinaryFormatter serialization is not trim compatible because the type of objects being processed cannot be statically discovered.")]
    public static object BinaryDeserialize(byte[] data, int offset, int length, SerializationBinder binder = null)
    {
        using (var DeserializeStream = new MemoryStream(data, offset, length))
        {
            DeserializeStream.Position = 0;
            var bf = new BinaryFormatter();
            if (binder != null)
            {
                bf.Binder = binder;
            }
            return bf.Deserialize(DeserializeStream);
        }
    }

    /// <summary>
    /// 从Stream中反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stream"></param>
    /// <param name="binder"></param>
    /// <returns></returns>
    [RequiresDynamicCode("BinaryFormatter serialization uses dynamic code generation, the type of objects being processed cannot be statically discovered.")]
    [RequiresUnreferencedCode("BinaryFormatter serialization is not trim compatible because the type of objects being processed cannot be statically discovered.")]
    public static T BinaryDeserialize<T>(Stream stream, SerializationBinder binder = null)
    {
        return (T)BinaryDeserialize(stream);
    }

    /// <summary>
    /// 从流中反序列化对象。
    /// </summary>
    /// <param name="stream">包含序列化对象数据的流。</param>
    /// <param name="binder">可选的绑定器，用于控制反序列化过程中的类型绑定。</param>
    /// <returns>反序列化后的对象。</returns>
    [RequiresDynamicCode("BinaryFormatter serialization uses dynamic code generation, the type of objects being processed cannot be statically discovered.")]
    [RequiresUnreferencedCode("BinaryFormatter serialization is not trim compatible because the type of objects being processed cannot be statically discovered.")]
    public static object BinaryDeserialize(Stream stream, SerializationBinder binder = null)
    {
        // 创建BinaryFormatter实例以进行反序列化操作
        var bf = new BinaryFormatter();
        // 如果提供了自定义的SerializationBinder，则将其设置给BinaryFormatter
        if (binder != null)
        {
            bf.Binder = binder;
        }
        // 从流中反序列化对象并返回
        return bf.Deserialize(stream);
    }

    /// <summary>
    /// 将二进制文件数据反序列化为指定类型对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    [RequiresDynamicCode("BinaryFormatter serialization uses dynamic code generation, the type of objects being processed cannot be statically discovered.")]
    [RequiresUnreferencedCode("BinaryFormatter serialization is not trim compatible because the type of objects being processed cannot be statically discovered.")]
    public static T BinaryDeserializeFromFile<T>(string path)
    {
        using (var serializeStream = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            var bf = new BinaryFormatter();
            return (T)bf.Deserialize(serializeStream);
        }
    }

    /// <summary>
    /// 将二进制数据反序列化为指定类型对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    [RequiresDynamicCode("BinaryFormatter serialization uses dynamic code generation, the type of objects being processed cannot be statically discovered.")]
    [RequiresUnreferencedCode("BinaryFormatter serialization is not trim compatible because the type of objects being processed cannot be statically discovered.")]
    public static T BinaryDeserialize<T>(byte[] data)
    {
        return BinaryDeserialize<T>(data, 0, data.Length);
    }

    /// <summary>
    /// 从Byte[]中反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="binder"></param>
    /// <returns></returns>
    [RequiresDynamicCode("BinaryFormatter serialization uses dynamic code generation, the type of objects being processed cannot be statically discovered.")]
    [RequiresUnreferencedCode("BinaryFormatter serialization is not trim compatible because the type of objects being processed cannot be statically discovered.")]
    public static T BinaryDeserialize<T>(byte[] data, SerializationBinder binder = null)
    {
        return BinaryDeserialize<T>(data, 0, data.Length, binder);
    }

    #endregion 普通二进制反序列化

#pragma warning restore SYSLIB0011 // 微软觉得不安全，不推荐使用


    #region Xml序列化和反序列化

    /// <summary>
    /// Xml序列化数据对象
    /// </summary>
    /// <param name="obj">数据对象</param>
    /// <param name="encoding">编码格式</param>
    /// <returns></returns>
    [RequiresUnreferencedCode("Members from serialized types may be trimmed if not referenced directly")]
    [RequiresDynamicCode("XML serializer relies on dynamic code generation which is not available with Ahead of Time compilation")]
    public static string XmlSerializeToString(object obj, Encoding encoding)
    {
        return encoding.GetString(XmlSerializeToBytes(obj));
    }

    /// <summary>
    /// Xml序列化数据对象
    /// </summary>
    /// <param name="obj">数据对象</param>
    /// <returns></returns>
    [RequiresUnreferencedCode("Members from serialized types may be trimmed if not referenced directly")]
    [RequiresDynamicCode("XML serializer relies on dynamic code generation which is not available with Ahead of Time compilation")]
    public static string XmlSerializeToString(object obj)
    {
        return XmlSerializeToString(obj, Encoding.UTF8);
    }

    /// <summary>
    /// Xml序列化数据对象
    /// </summary>
    /// <param name="obj">数据对象</param>
    /// <returns></returns>
    [RequiresUnreferencedCode("Members from serialized types may be trimmed if not referenced directly")]
    [RequiresDynamicCode("XML serializer relies on dynamic code generation which is not available with Ahead of Time compilation")]
    public static byte[] XmlSerializeToBytes(object obj)
    {
        using (var fileStream = new MemoryStream())
        {
            var xml = new XmlSerializer(obj.GetType());
            xml.Serialize(fileStream, obj);
            return fileStream.ToArray();
        }
    }

    /// <summary>
    /// Xml序列化至文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="path"></param>
    [RequiresUnreferencedCode("Members from serialized types may be trimmed if not referenced directly")]
    [RequiresDynamicCode("XML serializer relies on dynamic code generation which is not available with Ahead of Time compilation")]
    public static void XmlSerializeToFile(object obj, string path)
    {
        using (var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            var xml = new XmlSerializer(obj.GetType());
            xml.Serialize(fileStream, obj);
            fileStream.Close();
        }
    }

    /// <summary>
    /// Xml反序列化
    /// </summary>
    /// <typeparam name="T">反序列化类型</typeparam>
    /// <param name="dataBytes">数据</param>
    /// <returns></returns>
    [RequiresUnreferencedCode("Members from serialized types may be trimmed if not referenced directly")]
    [RequiresDynamicCode("XML serializer relies on dynamic code generation which is not available with Ahead of Time compilation")]
    public static T XmlDeserializeFromBytes<T>(byte[] dataBytes)
    {
        var xmlSerializer = new XmlSerializer(typeof(T));
        using (Stream xmlStream = new MemoryStream(dataBytes))
        {
            return (T)xmlSerializer.Deserialize(xmlStream);
        }
    }

    /// <summary>
    /// Xml反序列化
    /// </summary>
    /// <param name="dataBytes"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("Members from deserialized types may be trimmed if not referenced directly")]
    [RequiresDynamicCode("XML serializer relies on dynamic code generation which is not available with Ahead of Time compilation")]
    public static object XmlDeserializeFromBytes(byte[] dataBytes, Type type)
    {
        var xmlSerializer = new XmlSerializer(type);
        using (Stream xmlStream = new MemoryStream(dataBytes))
        {
            return xmlSerializer.Deserialize(xmlStream);
        }
    }

    /// <summary>
    /// Xml反序列化
    /// </summary>
    /// <param name="xmlStream"></param>
    /// <param name="targetType"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("Members from deserialized types may be trimmed if not referenced directly")]
    [RequiresDynamicCode("XML serializer relies on dynamic code generation which is not available with Ahead of Time compilation")]
    public static object XmlDeserializeFromStream(Stream xmlStream, Type targetType)
    {
        var xmlSerializer = new XmlSerializer(targetType);
        return xmlSerializer.Deserialize(xmlStream);
    }

    /// <summary>
    /// Xml反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="xmlStream"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("Members from deserialized types may be trimmed if not referenced directly")]
    [RequiresDynamicCode("XML serializer relies on dynamic code generation which is not available with Ahead of Time compilation")]
    public static T XmlDeserializeFromStream<T>(Stream xmlStream)
    {
        var xmlSerializer = new XmlSerializer(typeof(T));
        return (T)xmlSerializer.Deserialize(xmlStream);
    }

    /// <summary>
    /// Xml反序列化
    /// </summary>
    /// <param name="xmlString">xml字符串</param>
    /// <param name="targetType"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("Members from deserialized types may be trimmed if not referenced directly")]
    [RequiresDynamicCode("XML serializer relies on dynamic code generation which is not available with Ahead of Time compilation")]
    public static object XmlDeserializeFromString(string xmlString, Type targetType)
    {
        return XmlDeserializeFromStream(new MemoryStream(Encoding.UTF8.GetBytes(xmlString)), targetType);
    }

    /// <summary>
    /// Xml反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="xmlString"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("Members from deserialized types may be trimmed if not referenced directly")]
    [RequiresDynamicCode("XML serializer relies on dynamic code generation which is not available with Ahead of Time compilation")]
    public static T XmlDeserializeFromString<T>(string xmlString)
    {
        return (T)XmlDeserializeFromString(xmlString, typeof(T));
    }

    /// <summary>
    /// Xml反序列化
    /// </summary>
    /// <typeparam name="T">反序列化类型</typeparam>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    [RequiresUnreferencedCode("Members from deserialized types may be trimmed if not referenced directly")]
    [RequiresDynamicCode("XML serializer relies on dynamic code generation which is not available with Ahead of Time compilation")]
    public static T XmlDeserializeFromFile<T>(string path)
    {
        using (Stream xmlStream = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            return (T)xmlSerializer.Deserialize(xmlStream);
        }
    }

    #endregion Xml序列化和反序列化

    #region Json序列化和反序列化

    /// <summary>
    /// 转换为Json
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static string ToJsonString(this object item)
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(item);
    }

    /// <summary>
    /// 从字符串到json
    /// </summary>
    /// <param name="json"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object FromJsonString(this string json, Type type)
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject(json, type);
    }

    /// <summary>
    /// 从字符串到json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    public static T FromJsonString<T>(this string json)
    {
        return (T)FromJsonString(json, typeof(T));
    }

    /// <summary>
    /// Json序列化数据对象
    /// </summary>
    /// <param name="obj">数据对象</param>
    /// <returns></returns>
    public static ReadOnlyMemory<byte> JsonSerializeToBytes(object obj)
    {
        return ToJsonString(obj).ToUtf8Bytes();
    }

    /// <summary>
    /// Json序列化至文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="path"></param>
    public static void JsonSerializeToFile(object obj, string path)
    {
        using (var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            var date = JsonSerializeToBytes(obj);
            fileStream.Write(date.Span);
            fileStream.Close();
        }
    }

    /// <summary>
    /// Json反序列化
    /// </summary>
    /// <typeparam name="T">反序列化类型</typeparam>
    /// <param name="memory">数据</param>
    /// <returns></returns>
    public static T JsonDeserializeFromBytes<T>(ReadOnlyMemory<byte> memory)
    {
        return (T)JsonDeserializeFromBytes(memory, typeof(T));
    }

    /// <summary>
    /// Xml反序列化
    /// </summary>
    /// <param name="memory"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object JsonDeserializeFromBytes(ReadOnlyMemory<byte> memory, Type type)
    {
        return FromJsonString(memory.Span.ToUtf8String(), type);
    }

    /// <summary>
    /// Json反序列化
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="json">json字符串</param>
    /// <returns></returns>
    public static T JsonDeserializeFromString<T>(string json)
    {
        return FromJsonString<T>(json);
    }

    /// <summary>
    /// Json反序列化
    /// </summary>
    /// <typeparam name="T">反序列化类型</typeparam>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    public static T JsonDeserializeFromFile<T>(string path)
    {
        return JsonDeserializeFromString<T>(File.ReadAllText(path));
    }

    #endregion Json序列化和反序列化
}