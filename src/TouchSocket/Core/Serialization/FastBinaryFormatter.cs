//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TouchSocket.Core
{
    /// <summary>
    /// 快速二进制序列化。
    /// </summary>
    public static class FastBinaryFormatter
    {
        private static readonly ConcurrentDictionary<string, SerializObject> m_instanceCache = new ConcurrentDictionary<string, SerializObject>();
        private static readonly ConcurrentDictionary<Type, IFastBinaryConverter> m_typeToConverter = new ConcurrentDictionary<Type, IFastBinaryConverter>();

        static FastBinaryFormatter()
        {
            AddFastBinaryConverter(typeof(Metadata), new MetadataFastBinaryConverter());
        }

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
            m_typeToConverter.AddOrUpdate(type, converter, (k, v) => converter);
        }

        #region Serialize

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="byteBlock">流</param>
        /// <param name="graph">对象</param>
        public static void Serialize(ByteBlock byteBlock, object graph)
        {
            byteBlock.Position = 1;
            SerializeObject(byteBlock, graph);
            byteBlock.Buffer[0] = 1;
            byteBlock.SetLength(byteBlock.Position);
        }

        private static int SerializeClass(ByteBlock stream, object obj, Type type)
        {
            int len = 0;
            if (obj != null)
            {
                SerializObject serializObject = GetOrAddInstance(type);
                foreach (PropertyInfo property in serializObject.Properties)
                {
                    if (property.GetCustomAttribute<FastNonSerializedAttribute>() != null)
                    {
                        continue;
                    }
                    byte[] propertyBytes = Encoding.UTF8.GetBytes(property.Name);
                    if (propertyBytes.Length > byte.MaxValue)
                    {
                        throw new Exception($"属性名：{property.Name}超长");
                    }
                    byte lenBytes = (byte)propertyBytes.Length;
                    stream.Write(lenBytes);
                    stream.Write(propertyBytes, 0, propertyBytes.Length);
                    len += propertyBytes.Length + 1;
                    len += SerializeObject(stream, property.GetValue(obj, null));
                }

                foreach (FieldInfo fieldInfo in serializObject.FieldInfos)
                {
                    if (fieldInfo.GetCustomAttribute<FastNonSerializedAttribute>() != null)
                    {
                        continue;
                    }
                    byte[] propertyBytes = Encoding.UTF8.GetBytes(fieldInfo.Name);
                    if (propertyBytes.Length > byte.MaxValue)
                    {
                        throw new Exception($"属性名：{fieldInfo.Name}超长");
                    }
                    byte lenBytes = (byte)propertyBytes.Length;
                    stream.Write(lenBytes);
                    stream.Write(propertyBytes, 0, propertyBytes.Length);
                    len += propertyBytes.Length + 1;
                    len += SerializeObject(stream, fieldInfo.GetValue(obj));
                }
            }
            return len;
        }

        private static int SerializeDictionary(ByteBlock stream, IEnumerable param)
        {
            int len = 0;
            if (param != null)
            {
                long oldPosition = stream.Position;
                stream.Position += 4;
                len += 4;
                uint paramLen = 0;

                foreach (dynamic item in param)
                {
                    len += SerializeObject(stream, item.Key);
                    len += SerializeObject(stream, item.Value);
                    paramLen++;
                }
                long newPosition = stream.Position;
                stream.Position = oldPosition;
                stream.Write(TouchSocketBitConverter.Default.GetBytes(paramLen));
                stream.Position = newPosition;
            }
            return len;
        }

        private static int SerializeIListOrArray(ByteBlock stream, IEnumerable param)
        {
            int len = 0;
            if (param != null)
            {
                long oldPosition = stream.Position;
                stream.Position += 4;
                len += 4;
                uint paramLen = 0;

                foreach (object item in param)
                {
                    paramLen++;
                    len += SerializeObject(stream, item);
                }
                long newPosition = stream.Position;
                stream.Position = oldPosition;
                stream.Write(TouchSocketBitConverter.Default.GetBytes(paramLen));
                stream.Position = newPosition;
            }
            return len;
        }

        private static int SerializeObject(ByteBlock byteBlock, object graph)
        {
            int len = 0;
            byte[] data = null;

            long startPosition = byteBlock.Position;
            long endPosition;
            if (graph != null)
            {
                if (graph is string str)
                {
                    data = Encoding.UTF8.GetBytes(str);
                }
                else if (graph is byte by)
                {
                    data = new byte[] { by };
                }
                else if (graph is sbyte sby)
                {
                    data = TouchSocketBitConverter.Default.GetBytes(sby);
                }
                else if (graph is bool b)
                {
                    data = TouchSocketBitConverter.Default.GetBytes(b);
                }
                else if (graph is short s)
                {
                    data = TouchSocketBitConverter.Default.GetBytes(s);
                }
                else if (graph is ushort us)
                {
                    data = TouchSocketBitConverter.Default.GetBytes(us);
                }
                else if (graph is int i)
                {
                    data = TouchSocketBitConverter.Default.GetBytes(i);
                }
                else if (graph is uint ui)
                {
                    data = TouchSocketBitConverter.Default.GetBytes(ui);
                }
                else if (graph is long l)
                {
                    data = TouchSocketBitConverter.Default.GetBytes(l);
                }
                else if (graph is ulong ul)
                {
                    data = TouchSocketBitConverter.Default.GetBytes(ul);
                }
                else if (graph is float f)
                {
                    data = TouchSocketBitConverter.Default.GetBytes(f);
                }
                else if (graph is double d)
                {
                    data = TouchSocketBitConverter.Default.GetBytes(d);
                }
                else if (graph is DateTime time)
                {
                    data = Encoding.UTF8.GetBytes(time.Ticks.ToString());
                }
                else if (graph is char c)
                {
                    data = TouchSocketBitConverter.Default.GetBytes(c);
                }
                else if (graph is Enum)
                {
                    var enumValType = Enum.GetUnderlyingType(graph.GetType());

                    if (enumValType == TouchSocketCoreUtility.byteType)
                    {
                        data = new byte[] { (byte)graph };
                    }
                    else if (enumValType == TouchSocketCoreUtility.shortType)
                    {
                        data = TouchSocketBitConverter.Default.GetBytes((short)graph);
                    }
                    else if (enumValType == TouchSocketCoreUtility.intType)
                    {
                        data = TouchSocketBitConverter.Default.GetBytes((int)graph);
                    }
                    else
                    {
                        data = TouchSocketBitConverter.Default.GetBytes((long)graph);
                    }
                }
                else if (graph is byte[] bytes)
                {
                    data = bytes;
                }
                else
                {
                    byteBlock.Position += 4;
                    Type type = graph.GetType();
                    if (m_typeToConverter.TryGetValue(type, out IFastBinaryConverter converter))
                    {
                        len += converter.Write(byteBlock, graph);
                    }
                    else
                    {
                        var serializeObj = GetOrAddInstance(type);
                        switch (serializeObj.instanceType)
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

            byte[] lenBuffer = TouchSocketBitConverter.Default.GetBytes(len);
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
            bool nullable = type.IsNullableType();
            if (nullable)
            {
                type = type.GenericTypeArguments[0];
            }
            dynamic obj;
            int len = TouchSocketBitConverter.Default.ToInt32(datas, offset);
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
                    obj = (new DateTime(long.Parse(Encoding.UTF8.GetString(datas, offset, len))));
                }
                else if (type.BaseType == typeof(Enum))
                {
                    Type enumType = Enum.GetUnderlyingType(type);

                    if (enumType == typeof(byte))
                    {
                        obj = Enum.ToObject(type, datas[offset]);
                    }
                    else if (enumType == typeof(short))
                    {
                        obj = Enum.ToObject(type, TouchSocketBitConverter.Default.ToInt16(datas, offset));
                    }
                    else if (enumType == typeof(int))
                    {
                        obj = Enum.ToObject(type, TouchSocketBitConverter.Default.ToInt32(datas, offset));
                    }
                    else
                    {
                        obj = Enum.ToObject(type, TouchSocketBitConverter.Default.ToInt64(datas, offset));
                    }
                }
                else if (type == TouchSocketCoreUtility.bytesType)
                {
                    byte[] data = new byte[len];
                    Buffer.BlockCopy(datas, offset, data, 0, len);
                    obj = data;
                }
                else if (type.IsClass || type.IsStruct())
                {
                    if (m_typeToConverter.TryGetValue(type, out IFastBinaryConverter converter))
                    {
                        obj = converter.Read(datas, offset, len);
                    }
                    else
                    {
                        if (type.GetCustomAttribute<FastConverterAttribute>() is FastConverterAttribute attribute)
                        {
                            IFastBinaryConverter binaryConverter = (IFastBinaryConverter)Activator.CreateInstance(attribute.Type);
                            AddFastBinaryConverter(type, binaryConverter);
                            obj = binaryConverter.Read(datas, offset, len);
                        }
                        else
                        {
                            obj = DeserializeClass(type, datas, offset, len);
                        }
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

        private static object DeserializeClass(Type type, byte[] datas, int offset, int length)
        {
            SerializObject instanceObject = GetOrAddInstance(type);

            object instance;
            switch (instanceObject.instanceType)
            {
                case InstanceType.Class:
                    {
                        instance = instanceObject.GetInstance();
                        int index = offset;
                        while (offset - index < length && (length >= 4))
                        {
                            int len = datas[offset];
                            string propertyName = Encoding.UTF8.GetString(datas, offset + 1, len);
                            offset += len + 1;

                            if (instanceObject.PropertiesDic.ContainsKey(propertyName))
                            {
                                PropertyInfo property = instanceObject.PropertiesDic[propertyName];
                                object obj = Deserialize(property.PropertyType, datas, ref offset);
                                property.SetValue(instance, obj);
                            }
                            else if (instanceObject.FieldInfosDic.ContainsKey(propertyName))
                            {
                                FieldInfo property = instanceObject.FieldInfosDic[propertyName];
                                object obj = Deserialize(property.FieldType, datas, ref offset);
                                property.SetValue(instance, obj);
                            }
                            else
                            {
                                int pLen = TouchSocketBitConverter.Default.ToInt32(datas, offset);
                                offset += 4;
                                offset += pLen;
                            }
                        }
                        break;
                    }
                case InstanceType.List:
                    {
                        instance = instanceObject.GetInstance();
                        if (length > 0)
                        {
                            uint paramLen = TouchSocketBitConverter.Default.ToUInt32(datas, offset);
                            offset += 4;
                            for (uint i = 0; i < paramLen; i++)
                            {
                                object obj = Deserialize(instanceObject.ArgTypes[0], datas, ref offset);
                                instanceObject.AddMethod.Invoke(instance, new object[] { obj });
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
                            uint paramLen = TouchSocketBitConverter.Default.ToUInt32(datas, offset);
                            Array array = Array.CreateInstance(instanceObject.ArrayType, paramLen);

                            offset += 4;
                            for (uint i = 0; i < paramLen; i++)
                            {
                                object obj = Deserialize(instanceObject.ArrayType, datas, ref offset);
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
                        instance = instanceObject.GetInstance();
                        if (length > 0)
                        {
                            uint paramLen = TouchSocketBitConverter.Default.ToUInt32(datas, offset);
                            offset += 4;
                            for (uint i = 0; i < paramLen; i++)
                            {
                                object key = Deserialize(instanceObject.ArgTypes[0], datas, ref offset);
                                object value = Deserialize(instanceObject.ArgTypes[1], datas, ref offset);
                                if (key != null)
                                {
                                    instanceObject.AddMethod.Invoke(instance, new object[] { key, value });
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
            if (m_instanceCache.TryGetValue(type.FullName, out SerializObject instance))
            {
                return instance;
            }
            if (type.IsArray)//数组
            {
                SerializObject typeInfo = m_instanceCache.GetOrAdd(type.FullName, (v) =>
                {
                    SerializObject instanceObject = new SerializObject();
                    instanceObject.Type = type;
                    instanceObject.ArrayType = type.GetElementType();
                    instanceObject.instanceType = InstanceType.Array;
                    return instanceObject;
                });
                return typeInfo;
            }
            else if (type.IsClass || type.IsStruct())
            {
                if (type.IsNullableType())
                {
                    type = type.GetGenericArguments()[0];
                }

                SerializObject serializObject = m_instanceCache.GetOrAdd(type.FullName, (v) =>
                {
                    SerializObject instanceObject = new SerializObject();
                    instanceObject.Type = type;
                    if (TouchSocketCoreUtility.listType.IsAssignableFrom(type))
                    {
                        Type genericType = type;
                        while (true)
                        {
                            if (genericType.IsGenericType)
                            {
                                break;
                            }
                            genericType = genericType.BaseType;
                            if (genericType == TouchSocketCoreUtility.objType)
                            {
                                break;
                            }
                        }
                        instanceObject.ArgTypes = genericType.GetGenericArguments();

                        instanceObject.AddMethod = new Method(type.GetMethod("Add"));
                        instanceObject.instanceType = InstanceType.List;
                    }
                    else if (TouchSocketCoreUtility.dicType.IsAssignableFrom(type))
                    {
                        Type genericType = type;
                        while (true)
                        {
                            if (genericType.IsGenericType)
                            {
                                break;
                            }
                            genericType = genericType.BaseType;
                            if (genericType == TouchSocketCoreUtility.objType)
                            {
                                break;
                            }
                        }
                        instanceObject.ArgTypes = genericType.GetGenericArguments();
                        instanceObject.AddMethod = new Method(type.GetMethod("Add"));
                        instanceObject.instanceType = InstanceType.Dictionary;
                    }
                    else
                    {
                        instanceObject.instanceType = InstanceType.Class;
                    }
                    instanceObject.PropertiesDic = GetProperties(type).ToDictionary(a => a.Name);
                    instanceObject.FieldInfosDic = GetFieldInfos(type).ToDictionary(a => a.Name);
                    if (type.IsGenericType)
                    {
                        instanceObject.ArgTypes = type.GetGenericArguments();
                    }
                    return instanceObject;
                });
                return serializObject;
            }
            return null;
        }

        private static PropertyInfo[] GetProperties(Type type)
        {
            //return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            //    .ToArray();

            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                          .Where(p => p.CanWrite && p.CanRead).ToArray();
        }

        private static FieldInfo[] GetFieldInfos(Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(p => !p.IsInitOnly).ToArray();
        }
    }
}