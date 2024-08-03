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
using System.IO;

namespace TouchSocket.Core
{
    /// <summary>
    /// 快速二进制序列化。
    /// </summary>
    public static class FastBinaryFormatter
    {
        /// <summary>
        /// DynamicallyAccessed
        /// </summary>
        public const DynamicallyAccessedMemberTypes DynamicallyAccessed = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties;

        private static readonly DefaultFastSerializerContext s_defaultFastSerializerContext = new DefaultFastSerializerContext();

        /// <summary>
        /// 默认的<see cref="FastSerializerContext"/>
        /// </summary>
        public static FastSerializerContext DefaultFastSerializerContext => s_defaultFastSerializerContext;

        /// <summary>
        /// 添加转换器。
        /// </summary>
        public static void AddFastBinaryConverter<[DynamicallyAccessedMembers(DynamicallyAccessed)] TType, [DynamicallyAccessedMembers(DynamicallyAccessed)] TConverter>() where TConverter : IFastBinaryConverter, new()
        {
            AddFastBinaryConverter(typeof(TType), (IFastBinaryConverter)Activator.CreateInstance(typeof(TConverter)));
        }

        /// <summary>
        /// 添加转换器。
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="converter"></param>
        public static void AddFastBinaryConverter<[DynamicallyAccessedMembers(DynamicallyAccessed)] TType>(IFastBinaryConverter converter)
        {
            AddFastBinaryConverter(typeof(TType), converter);
        }

        /// <summary>
        /// 添加转换器。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="converter"></param>
        public static void AddFastBinaryConverter([DynamicallyAccessedMembers(DynamicallyAccessed)] Type type, IFastBinaryConverter converter)
        {
            s_defaultFastSerializerContext.AddFastBinaryConverter(type, converter);
        }

        #region Serialize

        public static byte[] SerializeToBytes<[DynamicallyAccessedMembers(DynamicallyAccessed)] T>([DynamicallyAccessedMembers(DynamicallyAccessed)] in T graph)
        {
            var byteBlock = new ValueByteBlock(1024 * 64);
            try
            {
                Serialize(ref byteBlock, graph);
                return byteBlock.ToArray();
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        public static byte[] SerializeToBytes<[DynamicallyAccessedMembers(DynamicallyAccessed)] T>([DynamicallyAccessedMembers(DynamicallyAccessed)] in T graph, FastSerializerContext serializerContext)
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

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="byteBlock">流</param>
        /// <param name="graph">对象</param>
        public static void Serialize<TByteBlock, [DynamicallyAccessedMembers(DynamicallyAccessed)] T>(ref TByteBlock byteBlock, [DynamicallyAccessedMembers(DynamicallyAccessed)] in T graph)
            where TByteBlock : IByteBlock
        {
            Serialize(ref byteBlock, graph, s_defaultFastSerializerContext);
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="byteBlock">流</param>
        /// <param name="graph">对象</param>
        /// <param name="serializerContext"></param>
        public static void Serialize<TByteBlock, [DynamicallyAccessedMembers(DynamicallyAccessed)] T>(ref TByteBlock byteBlock, in T graph, FastSerializerContext serializerContext)
            where TByteBlock : IByteBlock
        {
            ThrowHelper.ThrowArgumentNullExceptionIf(serializerContext);

            var startPosition = byteBlock.Position;

            byteBlock.Position = startPosition + 1;
            SerializeObject(ref byteBlock, graph, serializerContext);

            var pos = byteBlock.Position;
            byteBlock.Position = startPosition;
            byteBlock.WriteByte(1);

            byteBlock.Position = pos;
        }

        private static void SerializeIListOrArray<TByteBlock>(ref TByteBlock byteBlock, in IEnumerable param, FastSerializerContext serializerContext) where TByteBlock : IByteBlock
        {
            if (param != null)
            {
                var oldPosition = byteBlock.Position;
                byteBlock.Position += 4;
                uint paramLen = 0;

                foreach (var item in param)
                {
                    paramLen++;
                    SerializeObject(ref byteBlock, item, serializerContext);
                }
                var newPosition = byteBlock.Position;
                byteBlock.Position = oldPosition;
                byteBlock.Write(TouchSocketBitConverter.Default.GetBytes(paramLen));
                byteBlock.Position = newPosition;
            }
        }

        private static void SerializeObject<TByteBlock, T>(ref TByteBlock byteBlock, in T graph, FastSerializerContext serializerContext)
            where TByteBlock : IByteBlock
        {
            if (graph is null)
            {
                byteBlock.WriteNull();
                return;
            }

            byteBlock.WriteNotNull();
            switch (graph)
            {
                case byte value:
                    {
                        byteBlock.WriteByte(value);
                        return;
                    }
                case sbyte value:
                    {
                        byteBlock.WriteInt16(value);
                        return;
                    }
                case bool value:
                    {
                        byteBlock.WriteBoolean(value);
                        return;
                    }
                case short value:
                    {
                        byteBlock.WriteInt16(value);
                        return;
                    }
                case ushort value:
                    {
                        byteBlock.WriteUInt16(value);
                        return;
                    }
                case int value:
                    {
                        byteBlock.WriteInt32(value);
                        return;
                    }
                case uint value:
                    {
                        byteBlock.WriteUInt32(value);
                        return;
                    }
                case long value:
                    {
                        byteBlock.WriteInt64(value);
                        return;
                    }
                case ulong value:
                    {
                        byteBlock.WriteUInt64(value);
                        return;
                    }
                case float value:
                    {
                        byteBlock.WriteFloat(value);
                        return;
                    }
                case double value:
                    {
                        byteBlock.WriteDouble(value);
                        return;
                    }
                case char value:
                    {
                        byteBlock.WriteChar(value);
                        return;
                    }
                case decimal value:
                    {
                        byteBlock.WriteDecimal(value);
                        return;
                    }
                case DateTime value:
                    {
                        byteBlock.WriteDateTime(value);
                        return;
                    }
                case TimeSpan value:
                    {
                        byteBlock.WriteTimeSpan(value);
                        return;
                    }
                case string value:
                    {
                        byteBlock.WriteString(value);
                        return;
                    }
                case Enum _:
                    {
                        var type = graph.GetType();
                        var enumValType = Enum.GetUnderlyingType(type);

                        if (enumValType == typeof(byte))
                        {
                            byteBlock.WriteByte(Convert.ToByte(graph));
                        }
                        else if (enumValType == typeof(sbyte))
                        {
                            byteBlock.WriteInt16(Convert.ToSByte(graph));
                        }
                        else if (enumValType == typeof(short))
                        {
                            byteBlock.WriteInt16(Convert.ToInt16(graph));
                        }
                        else if (enumValType == typeof(ushort))
                        {
                            byteBlock.WriteUInt16(Convert.ToUInt16(graph));
                        }
                        else if (enumValType == typeof(int))
                        {
                            byteBlock.WriteInt32(Convert.ToInt32(graph));
                        }
                        else if (enumValType == typeof(uint))
                        {
                            byteBlock.WriteUInt32(Convert.ToUInt32(graph));
                        }
                        else if (enumValType == typeof(ulong))
                        {
                            byteBlock.WriteUInt64(Convert.ToUInt64(graph));
                        }
                        else
                        {
                            byteBlock.WriteInt64(Convert.ToInt64(graph));
                        }
                        return;
                    }
                case byte[] value:
                    {
                        byteBlock.WriteBytesPackage(value);
                        return;
                    }
                default:
                    {
                        var startPosition = byteBlock.Position;
                        byteBlock.Position += 4;
                        var type = graph.GetType();
                        var serializObject = serializerContext.GetSerializObject(type);
                        if (serializObject.Converter != null)
                        {
                            serializObject.Converter.Write(ref byteBlock, graph);
                        }
                        else
                        {
                            switch (serializObject.InstanceType)
                            {
                                case InstanceType.List:
                                    SerializeIListOrArray(ref byteBlock, (IEnumerable)graph, serializerContext);
                                    break;

                                case InstanceType.Array:
                                    SerializeIListOrArray(ref byteBlock, (IEnumerable)graph, serializerContext);
                                    break;

                                case InstanceType.Dictionary:
                                    {
                                        var oldPosition = byteBlock.Position;
                                        byteBlock.Position += 4;
                                        uint paramLen = 0;

                                        foreach (DictionaryEntry item in (IDictionary)graph)
                                        {
                                            SerializeObject(ref byteBlock, item.Key, serializerContext);
                                            SerializeObject(ref byteBlock, item.Value, serializerContext);
                                            paramLen++;
                                        }
                                        var newPosition = byteBlock.Position;
                                        byteBlock.Position = oldPosition;
                                        byteBlock.Write(TouchSocketBitConverter.Default.GetBytes(paramLen));
                                        byteBlock.Position = newPosition;
                                    }
                                    break;

                                default:
                                    {
                                        for (var i = 0; i < serializObject.MemberInfos.Length; i++)
                                        {
                                            var memberInfo = serializObject.MemberInfos[i];
                                            if (serializObject.EnableIndex)
                                            {
                                                byteBlock.WriteByte(memberInfo.Index);
                                            }
                                            else
                                            {
                                                byteBlock.WriteString(memberInfo.Name, FixedHeaderType.Byte);
                                            }

                                            SerializeObject(ref byteBlock, serializObject.MemberAccessor.GetValue(graph, memberInfo.Name), serializerContext);
                                        }
                                    }

                                    break;
                            }
                        }

                        var endPosition = byteBlock.Position;
                        byteBlock.Position = startPosition;

                        byteBlock.WriteInt32(endPosition - startPosition - 4);
                        byteBlock.Position = endPosition;
                        break;
                    }
            }
        }

        #endregion Serialize

        #region Deserialize

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Deserialize<TByteBlock>(ref TByteBlock byteBlock, [DynamicallyAccessedMembers(DynamicallyAccessed)] Type type)
            where TByteBlock : IByteBlock
        {
            return byteBlock.ReadByte() != 1
                ? throw new Exception("Fast反序列化数据流解析错误。")
                : Deserialize(type, ref byteBlock, s_defaultFastSerializerContext);
        }

        public static T Deserialize<TByteBlock, [DynamicallyAccessedMembers(DynamicallyAccessed)] T>(ref TByteBlock byteBlock)
            where TByteBlock : IByteBlock
        {
            return (T)Deserialize(ref byteBlock, typeof(T));
        }

        public static T Deserialize<TByteBlock, [DynamicallyAccessedMembers(DynamicallyAccessed)] T>(ref TByteBlock byteBlock, FastSerializerContext serializerContext)
            where TByteBlock : IByteBlock
        {
            return (T)Deserialize(ref byteBlock, typeof(T), serializerContext);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="type"></param>
        /// <param name="serializerContext"></param>
        /// <returns></returns>
        public static object Deserialize<TByteBlock>(ref TByteBlock byteBlock, [DynamicallyAccessedMembers(DynamicallyAccessed)] Type type, FastSerializerContext serializerContext) where TByteBlock : IByteBlock
        {
            return byteBlock.ReadByte() != 1
                ? throw new Exception("Fast反序列化数据流解析错误。")
                : serializerContext is null
                ? throw new ArgumentNullException(nameof(serializerContext))
                : Deserialize(type, ref byteBlock, serializerContext);
        }

        private static object Deserialize<TByteBlock>(Type type, ref TByteBlock byteBlock, FastSerializerContext serializerContext)
            where TByteBlock : IByteBlock
        {
            var nullable = type.IsNullableType();
            if (nullable)
            {
                type = type.GenericTypeArguments[0];
            }

            #region Null

            if (byteBlock.ReadIsNull())
            {
                return nullable ? null : type.GetDefault();
            }

            #endregion Null

            #region Enum

            if (type.IsEnum)
            {
                var enumType = Enum.GetUnderlyingType(type);

                if (enumType == typeof(byte))
                {
                    return Enum.ToObject(type, byteBlock.ReadByte());
                }
                else if (enumType == typeof(sbyte))
                {
                    return Enum.ToObject(type, byteBlock.ReadInt16());
                }
                else if (enumType == typeof(short))
                {
                    return Enum.ToObject(type, byteBlock.ReadInt16());
                }
                else if (enumType == typeof(ushort))
                {
                    return Enum.ToObject(type, byteBlock.ReadUInt16());
                }
                else if (enumType == typeof(int))
                {
                    return Enum.ToObject(type, byteBlock.ReadInt32());
                }
                else
                {
                    return enumType == typeof(uint)
                        ? Enum.ToObject(type, byteBlock.ReadUInt32())
                        : enumType == typeof(ulong) ? Enum.ToObject(type, byteBlock.ReadUInt64()) : Enum.ToObject(type, byteBlock.ReadInt64());
                }
            }

            #endregion Enum

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return byteBlock.ReadBoolean();

                case TypeCode.Char:
                    return byteBlock.ReadChar();

                case TypeCode.SByte:
                    return (sbyte)byteBlock.ReadInt16();

                case TypeCode.Byte:
                    return byteBlock.ReadByte();

                case TypeCode.Int16:
                    return byteBlock.ReadInt16();

                case TypeCode.UInt16:
                    return byteBlock.ReadUInt16();

                case TypeCode.Int32:
                    return byteBlock.ReadInt32();

                case TypeCode.UInt32:
                    return byteBlock.ReadUInt32();

                case TypeCode.Int64:
                    return byteBlock.ReadInt64();

                case TypeCode.UInt64:
                    return byteBlock.ReadUInt64();

                case TypeCode.Single:
                    return byteBlock.ReadFloat();

                case TypeCode.Double:
                    return byteBlock.ReadDouble();

                case TypeCode.Decimal:
                    return byteBlock.ReadDecimal();

                case TypeCode.DateTime:
                    return byteBlock.ReadDateTime();

                case TypeCode.String:
                    return byteBlock.ReadString();

                default:
                    {
                        if (type == typeof(byte[]))
                        {
                            return byteBlock.ReadBytesPackage();
                        }
                        else if (type.IsClass || type.IsStruct())
                        {
                            var len = byteBlock.ReadInt32();
                            var serializeObj = serializerContext.GetSerializObject(type);
                            return serializeObj.Converter != null
                                ? serializeObj.Converter.Read(ref byteBlock, type)
                                : DeserializeClass(type, ref byteBlock, len, serializerContext);
                        }
                        else
                        {
                            throw new Exception("未定义的类型：" + type.ToString());
                        }
                    }
            }
        }

        private static object DeserializeClass<TByteBlock>(Type type, ref TByteBlock byteBlock, int length, FastSerializerContext serializerContext) where TByteBlock : IByteBlock
        {
            var serializObject = serializerContext.GetSerializObject(type);

            object instance;
            switch (serializObject.InstanceType)
            {
                case InstanceType.Class:
                    {
                        instance = serializerContext.GetNewInstance(type);

                        var index = byteBlock.Position + length;

                        if (serializObject.EnableIndex)
                        {
                            while (byteBlock.Position < index)
                            {
                                var propertyNameIndex = byteBlock.ReadByte();
                                if (serializObject.IsStruct)
                                {
                                    if (serializObject.FastMemberInfoDicForIndex.TryGetValue(propertyNameIndex, out var property))
                                    {
                                        var obj = Deserialize(property.Type, ref byteBlock, serializerContext);
                                        property.SetValue(ref instance, obj);
                                    }
                                    else
                                    {
                                        IgnoreLength(ref byteBlock, type);
                                    }
                                }
                                else
                                {
                                    if (serializObject.FastMemberInfoDicForIndex.TryGetValue(propertyNameIndex, out var property))
                                    {
                                        var obj = Deserialize(property.Type, ref byteBlock, serializerContext);
                                        serializObject.MemberAccessor.SetValue(instance, property.Name, obj);
                                    }
                                    else
                                    {
                                        IgnoreLength(ref byteBlock, type);
                                    }
                                }
                            }
                        }
                        else
                        {
                            while (byteBlock.Position < index)
                            {
                                var propertyName = byteBlock.ReadString(FixedHeaderType.Byte);

                                if (serializObject.IsStruct)
                                {
                                    if (serializObject.FastMemberInfoDicForName.TryGetValue(propertyName, out var property))
                                    {
                                        var obj = Deserialize(property.Type, ref byteBlock, serializerContext);
                                        property.SetValue(ref instance, obj);
                                    }
                                    else
                                    {
                                        IgnoreLength(ref byteBlock, type);
                                    }
                                }
                                else
                                {
                                    if (serializObject.FastMemberInfoDicForName.TryGetValue(propertyName, out var property))
                                    {
                                        var obj = Deserialize(property.Type, ref byteBlock, serializerContext);
                                        serializObject.MemberAccessor.SetValue(instance, property.Name, obj);
                                    }
                                    else
                                    {
                                        IgnoreLength(ref byteBlock, type);
                                    }
                                }
                            }
                        }

                        break;
                    }
                case InstanceType.List:
                    {
                        instance = serializerContext.GetNewInstance(type);
                        var paramLen = byteBlock.ReadUInt32();
                        for (uint i = 0; i < paramLen; i++)
                        {
                            var obj = Deserialize(serializObject.ArgTypes[0], ref byteBlock, serializerContext);
                            serializObject.AddMethod.Invoke(instance, new object[] { obj });
                        }
                        break;
                    }
                case InstanceType.Array:
                    {
                        var paramLen = byteBlock.ReadUInt32();
                        var array = Array.CreateInstance(serializObject.ArgTypes[0], paramLen);

                        for (uint i = 0; i < paramLen; i++)
                        {
                            var obj = Deserialize(serializObject.ArgTypes[0], ref byteBlock, serializerContext);
                            array.SetValue(obj, i);
                        }
                        instance = array;
                        break;
                    }
                case InstanceType.Dictionary:
                    {
                        instance = serializerContext.GetNewInstance(type);
                        var paramLen = byteBlock.ReadUInt32();
                        for (uint i = 0; i < paramLen; i++)
                        {
                            var key = Deserialize(serializObject.ArgTypes[0], ref byteBlock, serializerContext);
                            var value = Deserialize(serializObject.ArgTypes[1], ref byteBlock, serializerContext);
                            serializObject.AddMethod.Invoke(instance, new object[] { key, value });
                        }
                        break;
                    }
                default:
                    instance = null;
                    break;
            }

            return instance;
        }

        private static void IgnoreLength<TByteBlock>(ref TByteBlock byteBlock, Type type) where TByteBlock : IByteBlock
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    byteBlock.Seek(1, SeekOrigin.Current);
                    break;

                case TypeCode.Char:
                    byteBlock.Seek(2, SeekOrigin.Current);
                    break;

                case TypeCode.SByte:
                    byteBlock.Seek(2, SeekOrigin.Current);
                    break;

                case TypeCode.Byte:
                    byteBlock.Seek(1, SeekOrigin.Current);
                    break;

                case TypeCode.Int16:
                    byteBlock.Seek(2, SeekOrigin.Current);
                    break;

                case TypeCode.UInt16:
                    byteBlock.Seek(2, SeekOrigin.Current);
                    break;

                case TypeCode.Int32:
                    byteBlock.Seek(4, SeekOrigin.Current);
                    break;

                case TypeCode.UInt32:
                    byteBlock.Seek(4, SeekOrigin.Current);
                    break;

                case TypeCode.Int64:
                    byteBlock.Seek(8, SeekOrigin.Current);
                    break;

                case TypeCode.UInt64:
                    byteBlock.Seek(8, SeekOrigin.Current);
                    break;

                case TypeCode.Single:
                    byteBlock.Seek(4, SeekOrigin.Current);
                    break;

                case TypeCode.Double:
                    byteBlock.Seek(8, SeekOrigin.Current);
                    break;

                case TypeCode.Decimal:
                    byteBlock.Seek(16, SeekOrigin.Current);
                    break;

                case TypeCode.DateTime:
                    byteBlock.Seek(8, SeekOrigin.Current);
                    break;

                case TypeCode.String:
                    byteBlock.ReadString();
                    break;

                default:
                    var len = byteBlock.ReadInt32();
                    byteBlock.Seek(len, SeekOrigin.Current);
                    break;
            }
        }

        #endregion Deserialize
    }
}