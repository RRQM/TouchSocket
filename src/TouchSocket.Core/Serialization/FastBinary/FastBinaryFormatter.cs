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
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

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

        private static readonly DefaultFastSerializerContext m_defaultFastSerializerContext = new DefaultFastSerializerContext();

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
            m_defaultFastSerializerContext.AddFastBinaryConverter(type, converter);
        }

        #region Serialize

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="byteBlock">流</param>
        /// <param name="graph">对象</param>
        public static void Serialize<[DynamicallyAccessedMembers(DynamicallyAccessed)] T>(ByteBlock byteBlock, [DynamicallyAccessedMembers(DynamicallyAccessed)] in T graph)
        {
            Serialize(byteBlock, graph, m_defaultFastSerializerContext);
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="byteBlock">流</param>
        /// <param name="graph">对象</param>
        /// <param name="serializerContext"></param>
        public static void Serialize<[DynamicallyAccessedMembers(DynamicallyAccessed)] T>(ByteBlock byteBlock, [DynamicallyAccessedMembers(DynamicallyAccessed)] in T graph, FastSerializerContext serializerContext)
        {
            if (serializerContext is null)
            {
                throw new ArgumentNullException(nameof(serializerContext));
            }

            byteBlock.Position = 1;
            SerializeObject(byteBlock, graph, serializerContext);
            byteBlock.Buffer[0] = 1;
            byteBlock.SetLength(byteBlock.Position);
        }

        private static int SerializeClass<T>(ByteBlock byteBlock, T obj, Type type, FastSerializerContext serializerContext)
        {
            var len = 0;
            if (obj != null)
            {
                var serializObject = serializerContext.GetSerializObject(type);

                for (var i = 0; i < serializObject.MemberInfos.Length; i++)
                {
                    var memberInfo = serializObject.MemberInfos[i];
                    if (serializObject.EnableIndex)
                    {
                        byteBlock.Write((byte)memberInfo.Index);
                        len += 1;
                    }
                    else
                    {
                        var propertyBytes = Encoding.UTF8.GetBytes(memberInfo.Name);
                        if (propertyBytes.Length > byte.MaxValue)
                        {
                            throw new Exception($"属性名：{memberInfo.Name}超长");
                        }
                        byteBlock.Write((byte)propertyBytes.Length);
                        byteBlock.Write(propertyBytes, 0, propertyBytes.Length);
                        len += propertyBytes.Length + 1;
                    }

                    len += SerializeObject(byteBlock, serializObject.MemberAccessor.GetValue(obj, memberInfo.Name), serializerContext);
                }
            }
            return len;
        }

        private static int SerializeDictionary(in ByteBlock byteBlock, in IEnumerable param, FastSerializerContext serializerContext)
        {
            var len = 0;
            if (param != null)
            {
                var oldPosition = byteBlock.Position;
                byteBlock.Position += 4;
                len += 4;
                uint paramLen = 0;

                foreach (var item in param)
                {
                    len += SerializeObject(byteBlock, DynamicMethodMemberAccessor.Default.GetValue(item, "Key"), serializerContext);
                    len += SerializeObject(byteBlock, DynamicMethodMemberAccessor.Default.GetValue(item, "Value"), serializerContext);
                    paramLen++;
                }
                var newPosition = byteBlock.Position;
                byteBlock.Position = oldPosition;
                byteBlock.Write(TouchSocketBitConverter.Default.GetBytes(paramLen));
                byteBlock.Position = newPosition;
            }
            return len;
        }

        private static int SerializeIListOrArray(in ByteBlock byteBlock, in IEnumerable param, FastSerializerContext serializerContext)
        {
            var len = 0;
            if (param != null)
            {
                var oldPosition = byteBlock.Position;
                byteBlock.Position += 4;
                len += 4;
                uint paramLen = 0;

                foreach (var item in param)
                {
                    paramLen++;
                    len += SerializeObject(byteBlock, item, serializerContext);
                }
                var newPosition = byteBlock.Position;
                byteBlock.Position = oldPosition;
                byteBlock.Write(TouchSocketBitConverter.Default.GetBytes(paramLen));
                byteBlock.Position = newPosition;
            }
            return len;
        }

        private static int SerializeObject<T>(in ByteBlock byteBlock, in T graph, FastSerializerContext serializerContext)
        {
            var len = 0;
            byte[] data = null;

            var startPosition = byteBlock.Position;
            long endPosition;
            if (graph != null)
            {
                var type = graph.GetType();
                if (type.IsPrimitive)
                {
                    switch (graph)
                    {
                        case byte value:
                            {
                                data = new byte[] { value };
                                break;
                            }
                        case sbyte value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case bool value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case short value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case ushort value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case int value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case uint value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case long value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case ulong value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case float value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case double value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case char value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        default:
                            {
                                throw new Exception("未知基础类型");
                            }
                    }
                }
                else
                {
                    switch (graph)
                    {
                        case decimal value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case DateTime value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value.Ticks);
                                break;
                            }
                        case Enum _:
                            {
                                var enumValType = Enum.GetUnderlyingType(type);

                                if (enumValType == TouchSocketCoreUtility.byteType)
                                {
                                    data = new byte[] { Convert.ToByte(graph) };
                                }
                                else
                                {
                                    if (enumValType == TouchSocketCoreUtility.shortType)
                                    {
                                        data = TouchSocketBitConverter.Default.GetBytes(Convert.ToInt16(graph));
                                    }
                                    else
                                    {
                                        if (enumValType == TouchSocketCoreUtility.intType)
                                        {
                                            data = TouchSocketBitConverter.Default.GetBytes(Convert.ToInt32(graph));
                                        }
                                        else
                                        {
                                            data = TouchSocketBitConverter.Default.GetBytes(Convert.ToInt64(graph));
                                        }
                                    }
                                }
                                break;
                            }
                        case byte[] value:
                            {
                                data = value;
                                break;
                            }
                        default:
                            {
                                byteBlock.Position += 4;
                                var serializeObj = serializerContext.GetSerializObject(type);
                                if (serializeObj.Converter != null)
                                {
                                    len += serializeObj.Converter.Write(byteBlock, graph);
                                }
                                else
                                {
                                    switch (serializeObj.InstanceType)
                                    {
                                        case InstanceType.List:
                                            len += SerializeIListOrArray(byteBlock, (IEnumerable)graph, serializerContext);
                                            break;

                                        case InstanceType.Array:
                                            len += SerializeIListOrArray(byteBlock, (IEnumerable)graph, serializerContext);
                                            break;

                                        case InstanceType.Dictionary:
                                            len += SerializeDictionary(byteBlock, (IEnumerable)graph, serializerContext);
                                            break;

                                        default:
                                        case InstanceType.Class:
                                            len += SerializeClass(byteBlock, graph, type, serializerContext);
                                            break;
                                    }
                                }
                                break;
                            }
                    }
                }

                if (data != null)
                {
                    len = data.Length;
                    endPosition = len + startPosition + 4;
                }
                else
                {
                    endPosition = byteBlock.Position;
                }
            }
            else
            {
                endPosition = startPosition + 4;
            }

            var lenBuffer = TouchSocketBitConverter.Default.GetBytes(len);
            byteBlock.Position = startPosition;
            byteBlock.Write(lenBuffer, 0, lenBuffer.Length);

            if (data != null)
            {
                byteBlock.Write(data, 0, data.Length);
            }
            byteBlock.Position = endPosition;
            return len + 4;
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
        public static object Deserialize(byte[] data, int offset, [DynamicallyAccessedMembers(DynamicallyAccessed)] Type type)
        {
            if (data[offset] != 1)
            {
                throw new Exception("Fast反序列化数据流解析错误。");
            }
            offset += 1;
            return Deserialize(type, data, ref offset, m_defaultFastSerializerContext);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="type"></param>
        /// <param name="serializerContext"></param>
        /// <returns></returns>
        public static object Deserialize(byte[] data, int offset, [DynamicallyAccessedMembers(DynamicallyAccessed)] Type type, FastSerializerContext serializerContext)
        {
            if (data[offset] != 1)
            {
                throw new Exception("Fast反序列化数据流解析错误。");
            }

            if (serializerContext is null)
            {
                throw new ArgumentNullException(nameof(serializerContext));
            }

            offset += 1;
            return Deserialize(type, data, ref offset, serializerContext);
        }

        private static object Deserialize(Type type, byte[] datas, ref int offset, FastSerializerContext serializerContext)
        {
            var nullable = type.IsNullableType();
            if (nullable)
            {
                type = type.GenericTypeArguments[0];
            }
            object obj;
            var len = TouchSocketBitConverter.Default.ToInt32(datas, offset);
            offset += 4;
            if (len > 0)
            {
                if (type == TouchSocketCoreUtility.byteType)
                {
                    obj = datas[offset];
                }
                else if (type == TouchSocketCoreUtility.sbyteType)
                {
                    obj = (sbyte)(TouchSocketBitConverter.Default.ToInt16(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.boolType)
                {
                    obj = (TouchSocketBitConverter.Default.ToBoolean(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.shortType)
                {
                    obj = (TouchSocketBitConverter.Default.ToInt16(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.ushortType)
                {
                    obj = (TouchSocketBitConverter.Default.ToUInt16(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.intType)
                {
                    obj = (TouchSocketBitConverter.Default.ToInt32(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.uintType)
                {
                    obj = (TouchSocketBitConverter.Default.ToUInt32(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.longType)
                {
                    obj = (TouchSocketBitConverter.Default.ToInt64(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.ulongType)
                {
                    obj = (TouchSocketBitConverter.Default.ToUInt64(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.floatType)
                {
                    obj = (TouchSocketBitConverter.Default.ToSingle(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.doubleType)
                {
                    obj = (TouchSocketBitConverter.Default.ToDouble(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.decimalType)
                {
                    obj = (TouchSocketBitConverter.Default.ToDecimal(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.charType)
                {
                    obj = (TouchSocketBitConverter.Default.ToChar(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.dateTimeType)
                {
                    obj = (new DateTime(TouchSocketBitConverter.Default.ToInt64(datas, offset)));
                }
                else if (type.BaseType == typeof(Enum))
                {
                    var enumType = Enum.GetUnderlyingType(type);

                    if (enumType == typeof(byte))
                    {
                        obj = Enum.ToObject(type, datas[offset]);
                    }
                    else
                    {
                        if (enumType == typeof(short))
                        {
                            obj = Enum.ToObject(type, TouchSocketBitConverter.Default.ToInt16(datas, offset));
                        }
                        else
                        {
                            if (enumType == typeof(int))
                            {
                                obj = Enum.ToObject(type, TouchSocketBitConverter.Default.ToInt32(datas, offset));
                            }
                            else
                            {
                                obj = Enum.ToObject(type, TouchSocketBitConverter.Default.ToInt64(datas, offset));
                            }
                        }
                    }
                }
                else if (type == TouchSocketCoreUtility.bytesType)
                {
                    var data = new byte[len];
                    Array.Copy(datas, offset, data, 0, len);
                    obj = data;
                }
                else if (type.IsClass || type.IsStruct())
                {
                    var serializeObj = serializerContext.GetSerializObject(type);
                    if (serializeObj.Converter != null)
                    {
                        obj = serializeObj.Converter.Read(datas, offset, len);
                    }
                    else
                    {
                        obj = DeserializeClass(type, datas, offset, len, serializerContext);
                    }
                }
                else
                {
                    throw new Exception("未定义的类型：" + type.ToString());
                }
            }
            else
            {
                if (nullable)
                {
                    obj = null;
                }
                else
                {
                    obj = type.GetDefault();
                }
            }
            offset += len;
            return obj;
        }

        private static object DeserializeClass(Type type, byte[] datas, int offset, int length, FastSerializerContext serializerContext)
        {
            var serializObject = serializerContext.GetSerializObject(type);

            object instance;
            switch (serializObject.InstanceType)
            {
                case InstanceType.Class:
                    {
                        instance = serializerContext.GetNewInstance(type);
                        var index = offset;
                        if (serializObject.EnableIndex)
                        {
                            while (offset - index < length && (length > 0))
                            {
                                var propertyNameIndex = datas[offset];
                                offset += 1;
                                if (serializObject.IsStruct)
                                {
                                    if (serializObject.FastMemberInfoDicForIndex.TryGetValue(propertyNameIndex, out var property))
                                    {
                                        var obj = Deserialize(property.Type, datas, ref offset, serializerContext);
                                        property.SetValue(ref instance, obj);
                                    }
                                    else
                                    {
                                        var pLen = TouchSocketBitConverter.Default.ToInt32(datas, offset);
                                        offset += pLen;
                                    }
                                }
                                else
                                {
                                    if (serializObject.FastMemberInfoDicForIndex.TryGetValue(propertyNameIndex, out var property))
                                    {
                                        var obj = Deserialize(property.Type, datas, ref offset, serializerContext);
                                        serializObject.MemberAccessor.SetValue(instance, property.Name, obj);
                                    }
                                    else
                                    {
                                        var pLen = TouchSocketBitConverter.Default.ToInt32(datas, offset);
                                        offset += pLen;
                                    }
                                }
                            }
                        }
                        else
                        {
                            while (offset - index < length && (length >= 4))
                            {
                                int len = datas[offset];
                                var propertyName = Encoding.UTF8.GetString(datas, offset + 1, len);
                                offset += len + 1;
                                if (serializObject.IsStruct)
                                {
                                    if (serializObject.FastMemberInfoDicForName.TryGetValue(propertyName, out var property))
                                    {
                                        var obj = Deserialize(property.Type, datas, ref offset, serializerContext);
                                        property.SetValue(ref instance, obj);
                                    }
                                    else
                                    {
                                        var pLen = TouchSocketBitConverter.Default.ToInt32(datas, offset);
                                        offset += 4;
                                        offset += pLen;
                                    }
                                }
                                else
                                {
                                    if (serializObject.FastMemberInfoDicForName.TryGetValue(propertyName, out var property))
                                    {
                                        var obj = Deserialize(property.Type, datas, ref offset, serializerContext);
                                        serializObject.MemberAccessor.SetValue(instance, property.Name, obj);
                                    }
                                    else
                                    {
                                        var pLen = TouchSocketBitConverter.Default.ToInt32(datas, offset);
                                        offset += 4;
                                        offset += pLen;
                                    }
                                }
                            }
                        }

                        break;
                    }
                case InstanceType.List:
                    {
                        instance = serializerContext.GetNewInstance(type);
                        if (length > 0)
                        {
                            var paramLen = TouchSocketBitConverter.Default.ToUInt32(datas, offset);
                            offset += 4;
                            for (uint i = 0; i < paramLen; i++)
                            {
                                var obj = Deserialize(serializObject.ArgTypes[0], datas, ref offset, serializerContext);
                                serializObject.AddMethod.Invoke(instance, new object[] { obj });
                            }
                        }
                        else
                        {
                            instance = null;
                        }
                        break;
                    }
                case InstanceType.Array:
                    {
                        if (length > 0)
                        {
                            var paramLen = TouchSocketBitConverter.Default.ToUInt32(datas, offset);
                            var array = Array.CreateInstance(serializObject.ArrayType, paramLen);

                            offset += 4;
                            for (uint i = 0; i < paramLen; i++)
                            {
                                var obj = Deserialize(serializObject.ArrayType, datas, ref offset, serializerContext);
                                array.SetValue(obj, i);
                            }
                            instance = array;
                        }
                        else
                        {
                            instance = null;
                        }
                        break;
                    }
                case InstanceType.Dictionary:
                    {
                        instance = serializerContext.GetNewInstance(type);
                        if (length > 0)
                        {
                            var paramLen = TouchSocketBitConverter.Default.ToUInt32(datas, offset);
                            offset += 4;
                            for (uint i = 0; i < paramLen; i++)
                            {
                                var key = Deserialize(serializObject.ArgTypes[0], datas, ref offset, serializerContext);
                                var value = Deserialize(serializObject.ArgTypes[1], datas, ref offset, serializerContext);
                                if (key != null)
                                {
                                    serializObject.AddMethod.Invoke(instance, new object[] { key, value });
                                }
                            }

                            //uint paramLen = TouchSocketBitConverter.Default.ToUInt32(datas, offset);
                            //offset += 4;
                            //for (uint i = 0; i < paramLen; i++)
                            //{
                            //    offset += 4;
                            //    offset += datas[offset] + 1;
                            //    object key = this.Deserialize(instanceObject.ArgTypes[0], datas, ref offset);

                            //    offset += datas[offset] + 1;
                            //    object value = this.Deserialize(instanceObject.ArgTypes[1], datas, ref offset);
                            //    if (key != null)
                            //    {
                            //        instanceObject.AddMethod.Invoke(instance, new object[] { key, value });
                            //    }
                            //}
                        }
                        else
                        {
                            instance = null;
                        }
                        break;
                    }
                default:
                    instance = null;
                    break;
            }

            return instance;
        }

        #endregion Deserialize
    }
}