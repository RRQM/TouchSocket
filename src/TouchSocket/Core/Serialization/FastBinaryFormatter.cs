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
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Extensions;
using TouchSocket.Core.Reflection;

namespace TouchSocket.Core.Serialization
{
    /// <summary>
    /// 该序列化以二进制方式进行，但是不支持接口、抽象类、继承类等成员的序列化。
    /// </summary>
    public class FastBinaryFormatter
    {
        #region Serialize

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="graph">对象</param>
        public void Serialize(ByteBlock stream, object graph)
        {
            stream.Position = 1;
            this.SerializeObject(stream, graph);
            stream.Buffer[0] = 1;
            stream.SetLength(stream.Position);
        }

        private int SerializeObject(ByteBlock stream, object graph)
        {
            int len = 0;
            byte[] data = null;

            long startPosition = stream.Position;
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
                else if (graph is byte[])
                {
                    data = (byte[])graph;
                }
                else
                {
                    stream.Position += 4;
                    Type type = graph.GetType();

                    if (typeof(IEnumerable).IsAssignableFrom(type))
                    {
                        len += this.SerializeIEnumerable(stream, (IEnumerable)graph);
                    }
                    else
                    {
                        len += this.SerializeClass(stream, graph, type);
                    }
                }

                if (data != null)
                {
                    len = data.Length;
                    endPosition = len + startPosition + 4;
                }
                else
                {
                    endPosition = stream.Position;
                }
            }
            else
            {
                endPosition = startPosition + 4;
            }

            byte[] lenBuffer = TouchSocketBitConverter.Default.GetBytes(len);
            stream.Position = startPosition;
            stream.Write(lenBuffer, 0, lenBuffer.Length);

            if (data != null)
            {
                stream.Write(data, 0, data.Length);
            }
            stream.Position = endPosition;
            return len + 4;
        }

        private int SerializeClass(ByteBlock stream, object obj, Type type)
        {
            int len = 0;
            if (obj != null)
            {
                PropertyInfo[] propertyInfos = GetProperties(type);
                foreach (PropertyInfo property in propertyInfos)
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
                    len += this.SerializeObject(stream, property.GetValue(obj, null));
                }
            }
            return len;
        }

        private int SerializeIEnumerable(ByteBlock stream, IEnumerable param)
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
                    len += this.SerializeObject(stream, item);
                }
                long newPosition = stream.Position;
                stream.Position = oldPosition;
                stream.Write(TouchSocketBitConverter.Default.GetBytes(paramLen));
                stream.Position = newPosition;
            }
            return len;
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
        public object Deserialize(byte[] data, int offset, Type type)
        {
            if (data[offset] != 1)
            {
                throw new Exception("数据流解析错误");
            }
            offset += 1;
            return this.Deserialize(type, data, ref offset);
        }

        private dynamic Deserialize(Type type, byte[] datas, ref int offset)
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
                    obj = this.DeserializeClass(type, datas, offset, len);
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

        private dynamic DeserializeClass(Type type, byte[] datas, int offset, int length)
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

                            if (instanceObject.Properties.ContainsKey(propertyName))
                            {
                                PropertyInfo property = instanceObject.Properties[propertyName];
                                object obj = this.Deserialize(property.PropertyType, datas, ref offset);
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
                                object obj = this.Deserialize(instanceObject.ArgTypes[0], datas, ref offset);
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
                                object obj = this.Deserialize(instanceObject.ArrayType, datas, ref offset);
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
                                offset += 4;
                                offset += datas[offset] + 1;
                                object key = this.Deserialize(instanceObject.ArgTypes[0], datas, ref offset);

                                offset += datas[offset] + 1;
                                object value = this.Deserialize(instanceObject.ArgTypes[1], datas, ref offset);
                                if (key != null)
                                {
                                    instanceObject.AddMethod.Invoke(instance, new object[] { key, value });
                                }
                            }
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

        private static PropertyInfo[] GetProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }

        private static readonly ConcurrentDictionary<string, SerializObject> InstanceCache = new ConcurrentDictionary<string, SerializObject>();

        private static SerializObject GetOrAddInstance(Type type)
        {
            if (InstanceCache.TryGetValue(type.FullName, out SerializObject instance))
            {
                return instance;
            }
            if (type.IsArray)//数组
            {
                SerializObject typeInfo = InstanceCache.GetOrAdd(type.FullName, (v) =>
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

                SerializObject serializObject = InstanceCache.GetOrAdd(type.FullName, (v) =>
                {
                    SerializObject instanceObject = new SerializObject();
                    instanceObject.Type = type;
                    if (type.IsGenericType)
                    {
                        instanceObject.AddMethod = new Method(type.GetMethod("Add"));
                        instanceObject.ArgTypes = type.GetGenericArguments();
                        type = type.GetGenericTypeDefinition().MakeGenericType(instanceObject.ArgTypes);

                        if (instanceObject.ArgTypes.Length == 1)
                        {
                            instanceObject.instanceType = InstanceType.List;
                        }
                        else
                        {
                            instanceObject.instanceType = InstanceType.Dictionary;
                        }
                    }
                    else if (TouchSocketCoreUtility.listType.IsAssignableFrom(type))
                    {
                        Type baseType = type.BaseType;
                        while (!baseType.IsGenericType)
                        {
                            baseType = baseType.BaseType;
                        }
                        instanceObject.ArgTypes = baseType.GetGenericArguments();

                        instanceObject.AddMethod = new Reflection.Method(type.GetMethod("Add"));
                        instanceObject.instanceType = InstanceType.List;
                    }
                    else if (TouchSocketCoreUtility.dicType.IsAssignableFrom(type))
                    {
                        Type baseType = type.BaseType;
                        while (!baseType.IsGenericType)
                        {
                            baseType = baseType.BaseType;
                        }
                        instanceObject.ArgTypes = baseType.GetGenericArguments();
                        instanceObject.AddMethod = new Reflection.Method(type.GetMethod("Add"));
                        instanceObject.instanceType = InstanceType.Dictionary;
                    }
                    else
                    {
                        instanceObject.Properties = GetProperties(type).ToDictionary(a => a.Name);
                        instanceObject.instanceType = InstanceType.Class;
                    }
                    return instanceObject;
                });
                return serializObject;
            }
            return null;
        }
    }
}