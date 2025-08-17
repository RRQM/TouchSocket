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
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TouchSocket.Core;

public static class FastBinaryFormatter
{
    public const DynamicallyAccessedMemberTypes DynamicallyAccessed = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties;
    private static readonly DefaultFastSerializerContext s_defaultFastSerializerContext = new DefaultFastSerializerContext();
    public static FastSerializerContext DefaultFastSerializerContext => s_defaultFastSerializerContext;

    #region Converter Register
    public static void AddFastBinaryConverter<[DynamicallyAccessedMembers(DynamicallyAccessed)] TType, [DynamicallyAccessedMembers(DynamicallyAccessed)] TConverter>() where TConverter : IFastBinaryConverter, new()
    {
        AddFastBinaryConverter(typeof(TType), (IFastBinaryConverter)Activator.CreateInstance(typeof(TConverter)));
    }

    public static void AddFastBinaryConverter<[DynamicallyAccessedMembers(DynamicallyAccessed)] TType>(IFastBinaryConverter converter)
    {
        AddFastBinaryConverter(typeof(TType), converter);
    }

    public static void AddFastBinaryConverter([DynamicallyAccessedMembers(DynamicallyAccessed)] Type type, IFastBinaryConverter converter)
    {
        s_defaultFastSerializerContext.AddFastBinaryConverter(type, converter);
    }
    #endregion

    #region Serialize

    public static void Serialize<[DynamicallyAccessedMembers(DynamicallyAccessed)] T>(ByteBlock byteBlock, in T graph, FastSerializerContext serializerContext = null)
    {
        Serialize(ref byteBlock, graph, serializerContext);
    }

    public static void Serialize<TWriter, [DynamicallyAccessedMembers(DynamicallyAccessed)] T>(ref TWriter writer, in T graph, FastSerializerContext serializerContext = null)
        where TWriter : IBytesWriter
    {
        serializerContext ??= s_defaultFastSerializerContext;
        var span = writer.GetSpan(1);
        span[0] = 1; // 魔数(协议头)；反序列化时用于快速校验
        writer.Advance(1);
        SerializeObject(ref writer, graph, serializerContext);
    }

    public static byte[] SerializeToBytes<[DynamicallyAccessedMembers(DynamicallyAccessed)] T>(in T graph, FastSerializerContext serializerContext = null)
    {
        var byteBlock = new ValueByteBlock(1024 * 64);
        try
        {
            Serialize(ref byteBlock, graph, serializerContext);
            return byteBlock.ToArray();
        }
        finally
        {
            byteBlock.Dispose();
        }
    }

    private static void SerializeIListOrArray<TWriter>(ref TWriter writer, in IEnumerable param, FastSerializerContext serializerContext)
        where TWriter : IBytesWriter
    {
        var writerAnchor = new WriterAnchor<TWriter>(ref writer, 4); // 先占位集合元素个数
        uint paramLen = 0;
        foreach (var item in param)
        {
            paramLen++;
            SerializeObject(ref writer, item, serializerContext);
        }
        var span = writerAnchor.Rewind(ref writer, out _);
        span.WriteValue<uint>(paramLen);
    }

    private static void SerializeMutilDimensionalArray<TWriter>(ref TWriter writer, Array array, FastSerializerContext serializerContext)
        where TWriter : IBytesWriter
    {
        var rank = array.Rank;
        for (var i = 0; i < rank; i++)
        {
            WriterExtension.WriteValue<TWriter, int>(ref writer, array.GetLength(i));
        }
        foreach (var item in array)
        {
            SerializeObject(ref writer, item, serializerContext);
        }
    }

    /// <summary>
    /// 写入基础(可直接写)类型。返回 true 表示已完成写入，外层不再处理。
    /// </summary>
    private static bool TryWriteBasic<TWriter, T>(ref TWriter writer, T graph) where TWriter : IBytesWriter
    {
        if (graph is null)
        {
            WriterExtension.WriteNull(ref writer);
            return true;
        }

        WriterExtension.WriteNotNull(ref writer);

        // 原生/基础类型
        if (FastBinaryPrimitiveHelper.TryWritePrimitive(ref writer, graph))
        {
            return true;
        }

        // 枚举
        if (graph is Enum enumValue)
        {
            WriterExtension.WriteEnum(ref writer, enumValue);
            return true;
        }

        return false; // 不是基础类型，后续继续序列化复杂结构
    }

    /// <summary>
    /// 序列化对象（包含复杂类型递归）
    /// </summary>
    private static void SerializeObject<TWriter, T>(ref TWriter writer, T graph, FastSerializerContext serializerContext)
        where TWriter : IBytesWriter
    {
        // 基础+枚举+null 处理
        if (TryWriteBasic(ref writer, graph))
        {
            return;
        }

        // 复杂类型：写入长度占位（对象体长度，不含自身4字节）
        var writerAnchor = new WriterAnchor<TWriter>(ref writer, 4);
        var type = graph.GetType();
        var serializeObject = serializerContext.GetSerializeObject(type);

        if (serializeObject.Converter != null)
        {
            serializeObject.Converter.Write(ref writer, graph);
        }
        else
        {
            switch (serializeObject.InstanceType)
            {
                case InstanceType.List:
                    SerializeIListOrArray(ref writer, (IEnumerable)graph, serializerContext);
                    break;
                case InstanceType.Array:
                    {
                        var array = Unsafe.As<T, Array>(ref graph);
                        if (array.Rank == 1)
                        {
                            SerializeIListOrArray(ref writer, array, serializerContext);
                        }
                        else
                        {
                            SerializeMutilDimensionalArray(ref writer, array, serializerContext);
                        }
                        break;
                    }
                case InstanceType.Dictionary:
                    {
                        var writerAnchorDic = new WriterAnchor<TWriter>(ref writer, 4);
                        uint paramLen = 0;
                        foreach (DictionaryEntry item in (IDictionary)graph)
                        {
                            SerializeObject(ref writer, item.Key, serializerContext);
                            SerializeObject(ref writer, item.Value, serializerContext);
                            paramLen++;
                        }
                        var oldPositionSpan = writerAnchorDic.Rewind(ref writer, out _);
                        oldPositionSpan.WriteValue<uint>(paramLen);
                        break;
                    }
                default:
                    {
                        // 普通对象成员
                        for (var i = 0; i < serializeObject.MemberInfos.Length; i++)
                        {
                            var memberInfo = serializeObject.MemberInfos[i];
                            if (serializeObject.EnableIndex)
                            {
                                WriterExtension.WriteValue<TWriter, byte>(ref writer, memberInfo.Index);
                            }
                            else
                            {
                                WriterExtension.WriteString(ref writer, memberInfo.Name, FixedHeaderType.Byte);
                            }
                            SerializeObject(ref writer, serializeObject.MemberAccessor.GetValue(graph, memberInfo.Name), serializerContext);
                        }
                        break;
                    }
            }
        }

        // 回写长度
        var startPositionSpan = writerAnchor.Rewind(ref writer, out var length);
        startPositionSpan.WriteValue<int>(length - 4);
    }

    #endregion Serialize

    #region Deserialize

    public static T Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessed)] T>(ByteBlock byteBlock, FastSerializerContext serializerContext = null)
    {
        return Deserialize<ByteBlock, T>(ref byteBlock, serializerContext);
    }

    public static T Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessed)] T>(ref ValueByteBlock byteBlock, FastSerializerContext serializerContext = null)
    {
        return Deserialize<ValueByteBlock, T>(ref byteBlock, serializerContext);
    }

    public static T Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessed)] T>(byte[] bytes, FastSerializerContext serializerContext = null)
    {
        var byteBlock = new ValueByteBlock(bytes);
        return Deserialize<ValueByteBlock, T>(ref byteBlock, serializerContext);
    }

    public static T Deserialize<TReader, [DynamicallyAccessedMembers(DynamicallyAccessed)] T>(ref TReader reader, FastSerializerContext serializerContext = null)
        where TReader : IBytesReader
    {
        return (T)Deserialize(ref reader, typeof(T), serializerContext);
    }

    public static object Deserialize<TReader>(ref TReader reader, [DynamicallyAccessedMembers(DynamicallyAccessed)] Type type, FastSerializerContext serializerContext = null)
        where TReader : IBytesReader
    {
        if (ReaderExtension.ReadValue<TReader, byte>(ref reader) != 1)
        {
            throw new Exception("Fast反序列化数据流解析错误。");
        }

        serializerContext ??= s_defaultFastSerializerContext;
        return Deserialize(type, ref reader, serializerContext);
    }

    /// <summary>
    /// 读取基础类型（含 null、枚举、原生数值等）。返回 true 表示已完成，外层无需再处理。
    /// </summary>
    private static bool TryReadBasic<TReader>(Type type, bool nullable, ref TReader reader, out object value) where TReader : IBytesReader
    {
        // Null 标记
        if (ReaderExtension.ReadIsNull(ref reader))
        {
            value = nullable ? null : type.GetDefault();
            return true;
        }

        // 枚举
        if (type.IsEnum)
        {
            value = ReaderExtension.ReadEnum<TReader>(ref reader, type);
            return true;
        }

        // 原生/基础
        if (FastBinaryPrimitiveHelper.TryReadPrimitive(ref reader, type, out var primitiveObj))
        {
            value = primitiveObj;
            return true;
        }

        value = null;
        return false;
    }

    private static object Deserialize<TReader>(Type type, ref TReader reader, FastSerializerContext serializerContext)
        where TReader : IBytesReader
    {
        var nullable = type.IsNullableType();
        if (nullable)
        {
            type = type.GenericTypeArguments[0];
        }

        // 基础类型统一处理
        if (TryReadBasic(type, nullable, ref reader, out var basicValue))
        {
            return basicValue;
        }

        // 复杂类型：先读体长度
        var len = ReaderExtension.ReadValue<TReader, int>(ref reader);
        var serializeObj = serializerContext.GetSerializeObject(type);
        if (serializeObj.Converter != null)
        {
            return serializeObj.Converter.Read(ref reader, type);
        }
        return DeserializeClass(type, serializeObj, ref reader, len, serializerContext);
    }

    private static object DeserializeClass<TReader>(Type type, SerializObject serializeObject, ref TReader reader, int length, FastSerializerContext serializerContext)
        where TReader : IBytesReader
    {
        object instance;
        switch (serializeObject.InstanceType)
        {
            case InstanceType.Class:
                {
                    instance = serializerContext.GetNewInstance(type);
                    var endIndex = reader.BytesRead + length;
                    if (serializeObject.EnableIndex)
                    {
                        while (reader.BytesRead < endIndex)
                        {
                            var propertyNameIndex = ReaderExtension.ReadValue<TReader, byte>(ref reader);
                            if (serializeObject.IsStruct)
                            {
                                if (serializeObject.FastMemberInfoDicForIndex.TryGetValue(propertyNameIndex, out var property))
                                {
                                    var obj = Deserialize(property.Type, ref reader, serializerContext);
                                    property.SetValue(ref instance, obj);
                                }
                                else
                                {
                                    IgnoreLength(ref reader, type);
                                }
                            }
                            else
                            {
                                if (serializeObject.FastMemberInfoDicForIndex.TryGetValue(propertyNameIndex, out var property))
                                {
                                    var obj = Deserialize(property.Type, ref reader, serializerContext);
                                    serializeObject.MemberAccessor.SetValue(instance, property.Name, obj);
                                }
                                else
                                {
                                    IgnoreLength(ref reader, type);
                                }
                            }
                        }
                    }
                    else
                    {
                        while (reader.BytesRead < endIndex)
                        {
                            var propertyName = ReaderExtension.ReadString(ref reader, FixedHeaderType.Byte);
                            if (serializeObject.IsStruct)
                            {
                                if (serializeObject.FastMemberInfoDicForName.TryGetValue(propertyName, out var property))
                                {
                                    var obj = Deserialize(property.Type, ref reader, serializerContext);
                                    property.SetValue(ref instance, obj);
                                }
                                else
                                {
                                    IgnoreLength(ref reader, type);
                                }
                            }
                            else
                            {
                                if (serializeObject.FastMemberInfoDicForName.TryGetValue(propertyName, out var property))
                                {
                                    var obj = Deserialize(property.Type, ref reader, serializerContext);
                                    serializeObject.MemberAccessor.SetValue(instance, property.Name, obj);
                                }
                                else
                                {
                                    IgnoreLength(ref reader, type);
                                }
                            }
                        }
                    }
                    break;
                }
            case InstanceType.List:
                {
                    instance = serializerContext.GetNewInstance(type);
                    var paramLen = ReaderExtension.ReadValue<TReader, uint>(ref reader);
                    for (uint i = 0; i < paramLen; i++)
                    {
                        var obj = Deserialize(serializeObject.ArgTypes[0], ref reader, serializerContext);
                        serializeObject.AddMethod.Invoke(instance, new object[] { obj });
                    }
                    break;
                }
            case InstanceType.Array:
                {
                    var rank = serializeObject.Type.GetArrayRank();
                    if (rank == 1)
                    {
                        var paramLen = ReaderExtension.ReadValue<TReader, uint>(ref reader);
                        var array = Array.CreateInstance(serializeObject.ArgTypes[0], paramLen);
                        for (uint i = 0; i < paramLen; i++)
                        {
                            var obj = Deserialize(serializeObject.ArgTypes[0], ref reader, serializerContext);
                            array.SetValue(obj, i);
                        }
                        instance = array;
                    }
                    else
                    {
                        var rankArray = new int[rank];
                        for (var i = 0; i < rank; i++)
                        {
                            rankArray[i] = ReaderExtension.ReadValue<TReader, int>(ref reader);
                        }
                        var array = Array.CreateInstance(serializeObject.ArgTypes[0], rankArray);
                        var indices = new int[rank];
                        FillArrayRecursive(serializeObject, ref reader, serializerContext, array, rankArray, indices, 0);
                        instance = array;
                    }
                    break;
                }
            case InstanceType.Dictionary:
                {
                    instance = serializerContext.GetNewInstance(type);
                    var paramLen = ReaderExtension.ReadValue<TReader, uint>(ref reader);
                    for (uint i = 0; i < paramLen; i++)
                    {
                        var key = Deserialize(serializeObject.ArgTypes[0], ref reader, serializerContext);
                        var value = Deserialize(serializeObject.ArgTypes[1], ref reader, serializerContext);
                        serializeObject.AddMethod.Invoke(instance, new object[] { key, value });
                    }
                    break;
                }
            default:
                instance = null;
                break;
        }
        return instance;
    }

    private static void FillArrayRecursive<TReader>(SerializObject serializObject, ref TReader reader, FastSerializerContext serializerContext, Array array, int[] rankArray, int[] indices, int dimension)
        where TReader : IBytesReader
    {
        if (dimension == rankArray.Length)
        {
            var obj = Deserialize(serializObject.ArgTypes[0], ref reader, serializerContext);
            array.SetValue(obj, indices);
            return;
        }
        for (var i = 0; i < rankArray[dimension]; i++)
        {
            indices[dimension] = i;
            FillArrayRecursive(serializObject, ref reader, serializerContext, array, rankArray, indices, dimension + 1);
        }
    }

    private static void IgnoreLength<TReader>(ref TReader reader, Type type)
        where TReader : IBytesReader
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean: reader.Advance(1); break;
            case TypeCode.Char: reader.Advance(2); break;
            case TypeCode.SByte: reader.Advance(1); break;
            case TypeCode.Byte: reader.Advance(1); break;
            case TypeCode.Int16: reader.Advance(2); break;
            case TypeCode.UInt16: reader.Advance(2); break;
            case TypeCode.Int32: reader.Advance(4); break;
            case TypeCode.UInt32: reader.Advance(4); break;
            case TypeCode.Int64: reader.Advance(8); break;
            case TypeCode.UInt64: reader.Advance(8); break;
            case TypeCode.Single: reader.Advance(4); break;
            case TypeCode.Double: reader.Advance(8); break;
            case TypeCode.Decimal: reader.Advance(16); break;
            case TypeCode.DateTime: reader.Advance(8); break;
            case TypeCode.String: ReaderExtension.ReadString(ref reader); break;
            default:
                var len = ReaderExtension.ReadValue<TReader, int>(ref reader);
                reader.Advance(len);
                break;
        }
    }

    #endregion Deserialize
}