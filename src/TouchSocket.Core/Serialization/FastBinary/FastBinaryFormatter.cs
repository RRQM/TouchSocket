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

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TouchSocket.Core;

/// <summary>
/// 提供快速二进制序列化和反序列化功能的静态工具类。
/// </summary>
/// <remarks>
/// FastBinaryFormatter使用高性能的二进制序列化算法，支持基本数据类型、集合、数组、字典和自定义对象的序列化。
/// 内置魔数校验机制，支持自定义转换器，适用于高频序列化场景。
/// 所有序列化数据都包含协议头，用于反序列化时的快速校验。
/// </remarks>
public static class FastBinaryFormatter
{
    /// <summary>
    /// 动态访问成员类型的常量，指定序列化时需要访问的成员类型。
    /// </summary>
    /// <value>包含公共构造函数、方法、字段和属性的<see cref="DynamicallyAccessedMemberTypes"/>组合。</value>
    public const DynamicallyAccessedMemberTypes DynamicallyAccessed = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties;
    private static readonly DefaultFastSerializerContext s_defaultFastSerializerContext = new DefaultFastSerializerContext();

    /// <summary>
    /// 获取默认的快速序列化上下文。
    /// </summary>
    /// <value>全局共享的<see cref="FastSerializerContext"/>实例。</value>
    /// <remarks>
    /// 此上下文包含了所有注册的转换器和序列化对象缓存，在整个应用程序生命周期内重复使用。
    /// </remarks>
    public static FastSerializerContext DefaultFastSerializerContext => s_defaultFastSerializerContext;

    #region Converter Register

    /// <summary>
    /// 为指定类型添加快速二进制转换器。
    /// </summary>
    /// <typeparam name="TType">要序列化的类型。</typeparam>
    /// <typeparam name="TConverter">实现<see cref="IFastBinaryConverter"/>接口的转换器类型。</typeparam>
    /// <remarks>
    /// 转换器必须有公共无参构造函数。注册后，该类型的所有实例都将使用指定的转换器进行序列化。
    /// </remarks>
    public static void AddFastBinaryConverter<[DynamicallyAccessedMembers(DynamicallyAccessed)] TType, [DynamicallyAccessedMembers(DynamicallyAccessed)] TConverter>() where TConverter : IFastBinaryConverter, new()
    {
        AddFastBinaryConverter(typeof(TType), (IFastBinaryConverter)Activator.CreateInstance(typeof(TConverter)));
    }

    /// <summary>
    /// 为指定类型添加快速二进制转换器实例。
    /// </summary>
    /// <typeparam name="TType">要序列化的类型。</typeparam>
    /// <param name="converter">转换器实例。</param>
    /// <remarks>
    /// 注册后，该类型的所有实例都将使用指定的转换器进行序列化。
    /// </remarks>
    public static void AddFastBinaryConverter<[DynamicallyAccessedMembers(DynamicallyAccessed)] TType>(IFastBinaryConverter converter)
    {
        AddFastBinaryConverter(typeof(TType), converter);
    }

    /// <summary>
    /// 为指定类型添加快速二进制转换器实例。
    /// </summary>
    /// <param name="type">要序列化的<see cref="Type"/>。</param>
    /// <param name="converter">转换器实例。</param>
    /// <remarks>
    /// 注册后，该类型的所有实例都将使用指定的转换器进行序列化。
    /// </remarks>
    public static void AddFastBinaryConverter([DynamicallyAccessedMembers(DynamicallyAccessed)] Type type, IFastBinaryConverter converter)
    {
        s_defaultFastSerializerContext.AddFastBinaryConverter(type, converter);
    }

    #endregion Converter Register

    #region Serialize

    /// <summary>
    /// 将对象序列化到字节块中。
    /// </summary>
    /// <typeparam name="T">要序列化的对象类型。</typeparam>
    /// <param name="byteBlock">目标<see cref="ByteBlock"/>。</param>
    /// <param name="graph">要序列化的对象实例。</param>
    /// <param name="serializerContext">序列化上下文，为 <see langword="null"/> 时使用默认上下文。</param>
    /// <remarks>
    /// 此方法会在序列化数据前写入魔数（协议头），用于反序列化时的校验。
    /// </remarks>
    public static void Serialize<[DynamicallyAccessedMembers(DynamicallyAccessed)] T>(ByteBlock byteBlock, in T graph, FastSerializerContext serializerContext = null)
    {
        Serialize(ref byteBlock, graph, serializerContext);
    }

    /// <summary>
    /// 将对象序列化到字节写入器中。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的写入器类型。</typeparam>
    /// <typeparam name="T">要序列化的对象类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="graph">要序列化的对象实例。</param>
    /// <param name="serializerContext">序列化上下文，为 <see langword="null"/> 时使用默认上下文。</param>
    /// <remarks>
    /// 此方法会在序列化数据前写入魔数（值为1的字节），用于反序列化时的快速校验。
    /// </remarks>
    public static void Serialize<TWriter, [DynamicallyAccessedMembers(DynamicallyAccessed)] T>(ref TWriter writer, in T graph, FastSerializerContext serializerContext = null)
        where TWriter : IBytesWriter
    {
        serializerContext ??= s_defaultFastSerializerContext;
        var span = writer.GetSpan(1);
        span[0] = 1; // 魔数(协议头)；反序列化时用于快速校验
        writer.Advance(1);
        SerializeObject(ref writer, graph, serializerContext);
    }

    /// <summary>
    /// 将对象序列化为字节数组。
    /// </summary>
    /// <typeparam name="T">要序列化的对象类型。</typeparam>
    /// <param name="graph">要序列化的对象实例。</param>
    /// <param name="serializerContext">序列化上下文，为 <see langword="null"/> 时使用默认上下文。</param>
    /// <returns>包含序列化数据的字节数组。</returns>
    /// <remarks>
    /// 此方法内部使用64KB的<see cref="ValueByteBlock"/>进行序列化，完成后返回数据副本。
    /// </remarks>
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
        startPositionSpan.WriteValue<int>(length);
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

    #endregion Serialize

    #region Deserialize

    /// <summary>
    /// 从字节块中反序列化对象。
    /// </summary>
    /// <typeparam name="T">要反序列化的对象类型。</typeparam>
    /// <param name="byteBlock">包含序列化数据的<see cref="ByteBlock"/>。</param>
    /// <param name="serializerContext">序列化上下文，为 <see langword="null"/> 时使用默认上下文。</param>
    /// <returns>反序列化的对象实例。</returns>
    /// <exception cref="Exception">当数据流解析错误时抛出。</exception>
    /// <remarks>
    /// 此方法会先校验魔数（协议头），确保数据格式正确。
    /// </remarks>
    public static T Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessed)] T>(ByteBlock byteBlock, FastSerializerContext serializerContext = null)
    {
        return Deserialize<ByteBlock, T>(ref byteBlock, serializerContext);
    }

    /// <summary>
    /// 从值类型字节块中反序列化对象。
    /// </summary>
    /// <typeparam name="T">要反序列化的对象类型。</typeparam>
    /// <param name="byteBlock">包含序列化数据的<see cref="ValueByteBlock"/>。</param>
    /// <param name="serializerContext">序列化上下文，为 <see langword="null"/> 时使用默认上下文。</param>
    /// <returns>反序列化的对象实例。</returns>
    /// <exception cref="Exception">当数据流解析错误时抛出。</exception>
    /// <remarks>
    /// 此方法会先校验魔数（协议头），确保数据格式正确。
    /// </remarks>
    public static T Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessed)] T>(ref ValueByteBlock byteBlock, FastSerializerContext serializerContext = null)
    {
        return Deserialize<ValueByteBlock, T>(ref byteBlock, serializerContext);
    }

    /// <summary>
    /// 从字节数组中反序列化对象。
    /// </summary>
    /// <typeparam name="T">要反序列化的对象类型。</typeparam>
    /// <param name="bytes">包含序列化数据的字节数组。</param>
    /// <param name="serializerContext">序列化上下文，为 <see langword="null"/> 时使用默认上下文。</param>
    /// <returns>反序列化的对象实例。</returns>
    /// <exception cref="Exception">当数据流解析错误时抛出。</exception>
    /// <remarks>
    /// 此方法内部创建<see cref="ValueByteBlock"/>包装字节数组，然后进行反序列化。
    /// </remarks>
    public static T Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessed)] T>(byte[] bytes, FastSerializerContext serializerContext = null)
    {
        var byteBlock = new ValueByteBlock(bytes);
        return Deserialize<ValueByteBlock, T>(ref byteBlock, serializerContext);
    }

    /// <summary>
    /// 从字节读取器中反序列化指定类型的对象。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的读取器类型。</typeparam>
    /// <typeparam name="T">要反序列化的对象类型。</typeparam>
    /// <param name="reader">字节读取器实例。</param>
    /// <param name="serializerContext">序列化上下文，为 <see langword="null"/> 时使用默认上下文。</param>
    /// <returns>反序列化的对象实例。</returns>
    /// <exception cref="Exception">当数据流解析错误时抛出。</exception>
    /// <remarks>
    /// 此方法会先校验魔数（协议头），确保数据格式正确。
    /// </remarks>
    public static T Deserialize<TReader, [DynamicallyAccessedMembers(DynamicallyAccessed)] T>(ref TReader reader, FastSerializerContext serializerContext = null)
        where TReader : IBytesReader
    {
        return (T)Deserialize(ref reader, typeof(T), serializerContext);
    }

    /// <summary>
    /// 从字节读取器中反序列化指定类型的对象。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的读取器类型。</typeparam>
    /// <param name="reader">字节读取器实例。</param>
    /// <param name="type">要反序列化的对象<see cref="Type"/>。</param>
    /// <param name="serializerContext">序列化上下文，为 <see langword="null"/> 时使用默认上下文。</param>
    /// <returns>反序列化的对象实例。</returns>
    /// <exception cref="Exception">当数据流解析错误时抛出。</exception>
    /// <remarks>
    /// 此方法会先校验魔数（协议头），确保数据格式正确。魔数必须为1，否则抛出异常。
    /// </remarks>
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

    #endregion Deserialize
}