//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Text;

namespace TouchSocket.Core
{
    /// <summary>
    /// 快速二进制序列化。
    /// </summary>
    public static class FastBinaryFormatter
    {
        static FastBinaryFormatter()
        {
            AddFastBinaryConverter<Version, VersionFastBinaryConverter>();
        }
        private static readonly ConcurrentDictionary<Type, SerializObject> m_instanceCache = new ConcurrentDictionary<Type, SerializObject>();

        /// <summary>
        /// 添加转换器。
        /// </summary>
        public static void AddFastBinaryConverter<TType, TConverter>() where TConverter : IFastBinaryConverter, new()
        {
            AddFastBinaryConverter(typeof(TType), (IFastBinaryConverter)Activator.CreateInstance(typeof(TConverter)));
        }

        /// <summary>
        /// 添加转换器。
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="converter"></param>
        public static void AddFastBinaryConverter<TType>(IFastBinaryConverter converter)
        {
            AddFastBinaryConverter(typeof(TType), converter);
        }

        /// <summary>
        /// 添加转换器。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="converter"></param>
        public static void AddFastBinaryConverter(Type type, IFastBinaryConverter converter)
        {
            m_instanceCache.AddOrUpdate(type, new SerializObject(type) { Converter = converter }, (k, v) => v);
        }

        #region Serialize

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="byteBlock">流</param>
        /// <param name="graph">对象</param>
        public static void Serialize<T>(ByteBlock byteBlock, T graph)
        {
            byteBlock.Pos = 1;
            SerializeObject(byteBlock, graph);
            byteBlock.Buffer[0] = 1;
            byteBlock.SetLength(byteBlock.Pos);
        }

        private static int SerializeClass<T>(ByteBlock stream, T obj, Type type)
        {
            var len = 0;
            if (obj != null)
            {
                var serializObject = GetOrAddInstance(type);

                for (var i = 0; i < serializObject.MemberInfos.Length; i++)
                {
                    var memberInfo = serializObject.MemberInfos[i];
                    var propertyBytes = Encoding.UTF8.GetBytes(memberInfo.Name);
                    if (propertyBytes.Length > byte.MaxValue)
                    {
                        throw new Exception($"属性名：{memberInfo.Name}超长");
                    }
                    stream.Write((byte)propertyBytes.Length);
                    stream.Write(propertyBytes, 0, propertyBytes.Length);
                    len += propertyBytes.Length + 1;
                    //len += SerializeObject(stream, property.GetValue(obj));
                    len += SerializeObject(stream, serializObject.MemberAccessor.GetValue(obj, memberInfo.Name));
                }
                //foreach (PropertyInfo property in serializObject.Properties)
                //{
                //}

                //foreach (FieldInfo fieldInfo in serializObject.FieldInfos)
                //{
                //    byte[] propertyBytes = Encoding.UTF8.GetBytes(fieldInfo.Name);
                //    if (propertyBytes.Length > byte.MaxValue)
                //    {
                //        throw new Exception($"属性名：{fieldInfo.Name}超长");
                //    }
                //    byte lenBytes = (byte)propertyBytes.Length;
                //    stream.Write(lenBytes);
                //    stream.Write(propertyBytes, 0, propertyBytes.Length);
                //    len += propertyBytes.Length + 1;
                //    len += SerializeObject(stream, fieldInfo.GetValue(obj));
                //}
            }
            return len;
        }

        private static int SerializeDictionary(ByteBlock stream, IEnumerable param)
        {
            var len = 0;
            if (param != null)
            {
                var oldPosition = stream.Pos;
                stream.Pos += 4;
                len += 4;
                uint paramLen = 0;

                foreach (dynamic item in param)
                {
                    len += SerializeObject(stream, item.Key);
                    len += SerializeObject(stream, item.Value);
                    paramLen++;
                }
                var newPosition = stream.Pos;
                stream.Pos = oldPosition;
                stream.Write(TouchSocketBitConverter.Default.GetBytes(paramLen));
                stream.Pos = newPosition;
            }
            return len;
        }

        private static int SerializeIListOrArray(ByteBlock stream, IEnumerable param)
        {
            var len = 0;
            if (param != null)
            {
                var oldPosition = stream.Pos;
                stream.Pos += 4;
                len += 4;
                uint paramLen = 0;

                foreach (var item in param)
                {
                    paramLen++;
                    len += SerializeObject(stream, item);
                }
                var newPosition = stream.Pos;
                stream.Pos = oldPosition;
                stream.Write(TouchSocketBitConverter.Default.GetBytes(paramLen));
                stream.Pos = newPosition;
            }
            return len;
        }

        private static int SerializeObject<T>(ByteBlock byteBlock, T graph)
        {
            var len = 0;
            byte[] data = null;

            var startPosition = byteBlock.Pos;
            int endPosition;
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
                        case string value:
                            {
                                data = Encoding.UTF8.GetBytes(value);
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
                                    data = enumValType == TouchSocketCoreUtility.shortType
                                        ? TouchSocketBitConverter.Default.GetBytes(Convert.ToInt16(graph))
                                        : enumValType == TouchSocketCoreUtility.intType
                                                                            ? TouchSocketBitConverter.Default.GetBytes(Convert.ToInt32(graph))
                                                                            : TouchSocketBitConverter.Default.GetBytes(Convert.ToInt64(graph));
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
                                byteBlock.Pos += 4;
                                var serializeObj = GetOrAddInstance(type);
                                if (serializeObj.Converter != null)
                                {
                                    len += serializeObj.Converter.Write(byteBlock, graph);
                                }
                                else
                                {
                                    switch (serializeObj.InstanceType)
                                    {
                                        case InstanceType.List:
                                            len += SerializeIListOrArray(byteBlock, (IEnumerable)graph);
                                            break;

                                        case InstanceType.Array:
                                            len += SerializeIListOrArray(byteBlock, (IEnumerable)graph);
                                            break;

                                        case InstanceType.Dictionary:
                                            len += SerializeDictionary(byteBlock, (IEnumerable)graph);
                                            break;

                                        default:
                                        case InstanceType.Class:
                                            len += SerializeClass(byteBlock, graph, type);
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
                    endPosition = byteBlock.Pos;
                }
            }
            else
            {
                endPosition = startPosition + 4;
            }

            var lenBuffer = TouchSocketBitConverter.Default.GetBytes(len);
            byteBlock.Pos = startPosition;
            byteBlock.Write(lenBuffer, 0, lenBuffer.Length);

            if (data != null)
            {
                byteBlock.Write(data, 0, data.Length);
            }
            byteBlock.Pos = endPosition;
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
        public static object Deserialize(byte[] data, int offset, Type type)
        {
            if (data[offset] != 1)
            {
                throw new Exception("Fast反序列化数据流解析错误。");
            }
            offset += 1;
            return Deserialize(type, data, ref offset);
        }

        private static object Deserialize(Type type, byte[] datas, ref int offset)
        {
            var nullable = type.IsNullableType();
            if (nullable)
            {
                type = type.GenericTypeArguments[0];
            }
            dynamic obj;
            var len = TouchSocketBitConverter.Default.ToInt32(datas, offset);
            offset += 4;
            if (len > 0)
            {
                if (type == TouchSocketCoreUtility.stringType)
                {
                    obj = Encoding.UTF8.GetString(datas, offset, len);
                }
                else if (type == TouchSocketCoreUtility.byteType)
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
                    obj = (TouchSocketBitConverter.Default.ToDouble(datas, offset));
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
                        obj = enumType == typeof(short)
                            ? Enum.ToObject(type, TouchSocketBitConverter.Default.ToInt16(datas, offset))
                            : enumType == typeof(int)
                                                    ? Enum.ToObject(type, TouchSocketBitConverter.Default.ToInt32(datas, offset))
                                                    : (dynamic)Enum.ToObject(type, TouchSocketBitConverter.Default.ToInt64(datas, offset));
                    }
                }
                else if (type == TouchSocketCoreUtility.bytesType)
                {
                    var data = new byte[len];
                    Buffer.BlockCopy(datas, offset, data, 0, len);
                    obj = data;
                }
                else if (type.IsClass || type.IsStruct())
                {
                    var serializeObj = GetOrAddInstance(type);
                    obj = serializeObj.Converter != null ? serializeObj.Converter.Read(datas, offset, len) : (dynamic)DeserializeClass(type, datas, offset, len);
                }
                else
                {
                    throw new Exception("未定义的类型：" + type.ToString());
                }
            }
            else
            {
                obj = nullable ? null : (dynamic)type.GetDefault();
            }
            offset += len;
            return obj;
        }

        private static object DeserializeClass(Type type, byte[] datas, int offset, int length)
        {
            var serializObject = GetOrAddInstance(type);

            object instance;
            switch (serializObject.InstanceType)
            {
                case InstanceType.Class:
                    {
                        instance = serializObject.GetNewInstance();
                        var index = offset;
                        while (offset - index < length && (length >= 4))
                        {
                            int len = datas[offset];
                            var propertyName = Encoding.UTF8.GetString(datas, offset + 1, len);
                            offset += len + 1;
                            if (serializObject.IsStruct)
                            {
                                if (serializObject.PropertiesDic.ContainsKey(propertyName))
                                {
                                    var property = serializObject.PropertiesDic[propertyName];
                                    var obj = Deserialize(property.PropertyType, datas, ref offset);
                                    property.SetValue(instance, obj);
                                }
                                else if (serializObject.FieldInfosDic.ContainsKey(propertyName))
                                {
                                    var property = serializObject.FieldInfosDic[propertyName];
                                    var obj = Deserialize(property.FieldType, datas, ref offset);
                                    property.SetValue(instance, obj);
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
                                if (serializObject.PropertiesDic.TryGetValue(propertyName, out var property))
                                {
                                    var obj = Deserialize(property.PropertyType, datas, ref offset);
                                    serializObject.MemberAccessor.SetValue(instance, property.Name, obj);
                                }
                                else if (serializObject.FieldInfosDic.TryGetValue(propertyName, out var fieldInfo))
                                {
                                    var obj = Deserialize(fieldInfo.FieldType, datas, ref offset);
                                    serializObject.MemberAccessor.SetValue(instance, fieldInfo.Name, obj);
                                }
                                else
                                {
                                    var pLen = TouchSocketBitConverter.Default.ToInt32(datas, offset);
                                    offset += 4;
                                    offset += pLen;
                                }
                            }
                        }
                        break;
                    }
                case InstanceType.List:
                    {
                        instance = serializObject.GetNewInstance();
                        if (length > 0)
                        {
                            var paramLen = TouchSocketBitConverter.Default.ToUInt32(datas, offset);
                            offset += 4;
                            for (uint i = 0; i < paramLen; i++)
                            {
                                var obj = Deserialize(serializObject.ArgTypes[0], datas, ref offset);
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
                                var obj = Deserialize(serializObject.ArrayType, datas, ref offset);
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
                        instance = serializObject.GetNewInstance();
                        if (length > 0)
                        {
                            var paramLen = TouchSocketBitConverter.Default.ToUInt32(datas, offset);
                            offset += 4;
                            for (uint i = 0; i < paramLen; i++)
                            {
                                var key = Deserialize(serializObject.ArgTypes[0], datas, ref offset);
                                var value = Deserialize(serializObject.ArgTypes[1], datas, ref offset);
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

        private static SerializObject GetOrAddInstance(Type type)
        {
            if (m_instanceCache.TryGetValue(type, out var instance))
            {
                return instance;
            }
            if (type.IsArray)//数组
            {
                var instanceObject = new SerializObject(type);
                m_instanceCache.TryAdd(type, instanceObject);
                return instanceObject;
            }
            else if (type.IsClass || type.IsStruct())
            {
                if (type.IsNullableType())
                {
                    type = type.GetGenericArguments()[0];
                }

                var instanceObject = new SerializObject(type);
                m_instanceCache.TryAdd(type, instanceObject);
                return instanceObject;
            }
            return null;
        }
    }
}