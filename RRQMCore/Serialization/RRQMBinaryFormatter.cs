//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMCore.Helper;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RRQMCore.Serialization
{
    /// <summary>
    /// 该序列化以二进制方式进行，但是不支持接口、抽象类、继承类等成员的序列化。
    /// </summary>
    public class RRQMBinaryFormatter
    {
        #region Serialize

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="graph">对象</param>
        /// <param name="reserveAttributeName">保留属性名</param>
        public void Serialize(ByteBlock stream, object graph, bool reserveAttributeName)
        {
            this.reserveAttributeName = reserveAttributeName;
            stream.Position = 1;
            SerializeObject(stream, graph);
            if (reserveAttributeName)
            {
                stream.Buffer[0] = 1;
            }
            else
            {
                stream.Buffer[0] = 0;
            }
            stream.SetLength(stream.Position);
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="graph"></param>
        public void Serialize(ByteBlock stream, object graph)
        {
            Serialize(stream, graph, false);
        }

        /// <summary>
        /// 保留属性名
        /// </summary>
        private bool reserveAttributeName;

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
                    data = BitConverter.GetBytes((short)sby);
                }
                else if (graph is bool b)
                {
                    data = BitConverter.GetBytes(b);
                }
                else if (graph is short s)
                {
                    data = BitConverter.GetBytes(s);
                }
                else if (graph is ushort us)
                {
                    data = BitConverter.GetBytes(us);
                }
                else if (graph is int i)
                {
                    data = BitConverter.GetBytes(i);
                }
                else if (graph is uint ui)
                {
                    data = BitConverter.GetBytes(ui);
                }
                else if (graph is long l)
                {
                    data = BitConverter.GetBytes(l);
                }
                else if (graph is ulong ul)
                {
                    data = BitConverter.GetBytes(ul);
                }
                else if (graph is float f)
                {
                    data = BitConverter.GetBytes(f);
                }
                else if (graph is double d)
                {
                    data = BitConverter.GetBytes(d);
                }
                else if (graph is DateTime time)
                {
                    data = Encoding.UTF8.GetBytes(time.Ticks.ToString());
                }
                else if (graph is char c)
                {
                    data = BitConverter.GetBytes(c);
                }
                else if (graph is Enum)
                {
                    var enumValType = Enum.GetUnderlyingType(graph.GetType());

                    if (enumValType == RRQMReadonly.byteType)
                    {
                        data = new byte[] { (byte)graph };
                    }
                    else if (enumValType == RRQMReadonly.shortType)
                    {
                        data = BitConverter.GetBytes((short)graph);
                    }
                    else if (enumValType == RRQMReadonly.intType)
                    {
                        data = BitConverter.GetBytes((int)graph);
                    }
                    else
                    {
                        data = BitConverter.GetBytes((long)graph);
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
                        len += SerializeIEnumerable(stream, (IEnumerable)graph);
                    }
                    else
                    {
                        len += SerializeClass(stream, graph, type);
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

            byte[] lenBuffer = BitConverter.GetBytes(len);
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
                    if (property.GetCustomAttribute<RRQMNonSerializedAttribute>() != null)
                    {
                        continue;
                    }
                    if (reserveAttributeName)
                    {
                        byte[] propertyBytes = Encoding.UTF8.GetBytes(property.Name);
                        if (propertyBytes.Length > byte.MaxValue)
                        {
                            throw new Exceptions.RRQMException($"属性名：{property.Name}超长");
                        }
                        byte lenBytes = (byte)propertyBytes.Length;
                        stream.Write(lenBytes);
                        stream.Write(propertyBytes, 0, propertyBytes.Length);
                        len += propertyBytes.Length + 1;
                    }

                    len += SerializeObject(stream, property.GetValue(obj, null));
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
                    len += SerializeObject(stream, item);
                }
                long newPosition = stream.Position;
                stream.Position = oldPosition;
                stream.Write(BitConverter.GetBytes(paramLen));
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
            if (data[offset] == 0)
            {
                this.reserveAttributeName = false;
            }
            else if (data[offset] == 1)
            {
                this.reserveAttributeName = true;
            }
            else
            {
                throw new Exceptions.RRQMException("数据流解析错误");
            }
            offset += 1;
            return Deserialize(type, data, ref offset);
        }

        private dynamic Deserialize(Type type, byte[] datas, ref int offset)
        {
            dynamic obj;
            int len = BitConverter.ToInt32(datas, offset);
            offset += 4;
            if (len > 0)
            {
                if (type == RRQMReadonly.stringType)
                {
                    obj = Encoding.UTF8.GetString(datas, offset, len);
                }
                else if (type == RRQMReadonly.byteType)
                {
                    obj = datas[offset];
                }
                else if (type == RRQMReadonly.sbyteType)
                {
                    obj = (sbyte)(BitConverter.ToInt16(datas, offset));
                }
                else if (type == RRQMReadonly.boolType)
                {
                    obj = (BitConverter.ToBoolean(datas, offset));
                }
                else if (type == RRQMReadonly.shortType)
                {
                    obj = (BitConverter.ToInt16(datas, offset));
                }
                else if (type == RRQMReadonly.ushortType)
                {
                    obj = (BitConverter.ToUInt16(datas, offset));
                }
                else if (type == RRQMReadonly.intType)
                {
                    obj = (BitConverter.ToInt32(datas, offset));
                }
                else if (type == RRQMReadonly.uintType)
                {
                    obj = (BitConverter.ToUInt32(datas, offset));
                }
                else if (type == RRQMReadonly.longType)
                {
                    obj = (BitConverter.ToInt64(datas, offset));
                }
                else if (type == RRQMReadonly.ulongType)
                {
                    obj = (BitConverter.ToUInt64(datas, offset));
                }
                else if (type == RRQMReadonly.floatType)
                {
                    obj = (BitConverter.ToSingle(datas, offset));
                }
                else if (type == RRQMReadonly.doubleType)
                {
                    obj = (BitConverter.ToDouble(datas, offset));
                }
                else if (type == RRQMReadonly.decimalType)
                {
                    obj = (BitConverter.ToDouble(datas, offset));
                }
                else if (type == RRQMReadonly.charType)
                {
                    obj = (BitConverter.ToChar(datas, offset));
                }
                else if (type == RRQMReadonly.dateTimeType)
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
                        obj = Enum.ToObject(type, BitConverter.ToInt16(datas, offset));
                    }
                    else if (enumType == typeof(int))
                    {
                        obj = Enum.ToObject(type, BitConverter.ToInt32(datas, offset));
                    }
                    else
                    {
                        obj = Enum.ToObject(type, BitConverter.ToInt64(datas, offset));
                    }
                }
                else if (type == RRQMReadonly.bytesType)
                {
                    byte[] data = new byte[len];
                    Buffer.BlockCopy(datas, offset, data, 0, len);
                    obj = data;
                }
                else if (type.IsClass || type.IsStruct())
                {
                    obj = DeserializeClass(type, datas, offset, len);
                }
                else
                {
                    throw new Exception("未定义的类型：" + type.ToString());
                }
            }
            else
            {
                obj = type.GetDefault();
            }
            offset += len;
            return obj;
        }

        private dynamic DeserializeClass(Type type, byte[] datas, int offset, int length)
        {
            InstanceObject instanceObject = GetOrAddInstance(type);

            object instance;
            switch (instanceObject.instanceType)
            {
                case InstanceType.Class:
                    {
                        instance = instanceObject.GetInstance();
                        if (reserveAttributeName)
                        {
                            int index = offset;
                            while (offset - index < length && (length >= 4))
                            {
                                int len = datas[offset];
                                string propertyName = Encoding.UTF8.GetString(datas, offset + 1, len);
                                offset += len + 1;
                                PropertyInfo propertyInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                                if (propertyInfo == null)
                                {
                                    int pLen = BitConverter.ToInt32(datas, offset);
                                    offset += 4;
                                    offset += pLen;
                                    continue;
                                }
                                object obj = Deserialize(propertyInfo.PropertyType, datas, ref offset);
                                propertyInfo.SetValue(instance, obj);
                            }
                        }
                        else
                        {
                            foreach (var item in instanceObject.Properties)
                            {
                                object obj = Deserialize(item.PropertyType, datas, ref offset);
                                item.SetValue(instance, obj);
                            }
                        }

                        break;
                    }
                case InstanceType.List:
                    {
                        instance = instanceObject.GetInstance();
                        if (length > 0)
                        {
                            uint paramLen = BitConverter.ToUInt32(datas, offset);
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
                            uint paramLen = BitConverter.ToUInt32(datas, offset);
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
                            uint paramLen = BitConverter.ToUInt32(datas, offset);
                            offset += 4;
                            for (uint i = 0; i < paramLen; i++)
                            {
                                offset += 4;
                                if (reserveAttributeName)
                                {
                                    offset += datas[offset] + 1;
                                }

                                object key = Deserialize(instanceObject.ArgTypes[0], datas, ref offset);

                                if (reserveAttributeName)
                                {
                                    offset += datas[offset] + 1;
                                }

                                object value = Deserialize(instanceObject.ArgTypes[1], datas, ref offset);
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
            return type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        private static readonly ConcurrentDictionary<string, InstanceObject> InstanceCache = new ConcurrentDictionary<string, InstanceObject>();

        private static InstanceObject GetOrAddInstance(Type type)
        {
            if (InstanceCache.TryGetValue(type.FullName, out InstanceObject instance))
            {
                return instance;
            }

            if (type.IsArray && !type.IsGenericType)//数组
            {
                InstanceObject typeInfo = InstanceCache.GetOrAdd(type.FullName, (v) =>
                {
                    InstanceObject instanceObject = new InstanceObject();
                    instanceObject.Type = type;
                    instanceObject.ArrayType = type.GetElementType();
                    instanceObject.instanceType = InstanceType.Array;
                    instanceObject.Properties = GetProperties(type);
                    instanceObject.ProTypes = instanceObject.Properties.Select(a => a.PropertyType).ToArray();
                    return instanceObject;
                });
                return typeInfo;
            }
            else if (type.IsClass || type.IsStruct())
            {
                InstanceObject typeInfo = InstanceCache.GetOrAdd(type.FullName, (v) =>
                {
                    InstanceObject instanceObject = new InstanceObject();
                    instanceObject.Type = type;
                    instanceObject.Properties = GetProperties(type);
                    instanceObject.ProTypes = instanceObject.Properties.Select(a => a.PropertyType).ToArray();

                    if (type.IsGenericType)
                    {
                        instanceObject.AddMethod = type.GetMethod("Add");
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
                    else if (RRQMReadonly.listType.IsAssignableFrom(type))
                    {
                        Type baseType = type.BaseType;
                        while (!baseType.IsGenericType)
                        {
                            baseType = baseType.BaseType;
                        }
                        instanceObject.ArgTypes = baseType.GetGenericArguments();

                        instanceObject.AddMethod = type.GetMethod("Add");
                        instanceObject.instanceType = InstanceType.List;
                    }
                    else if (RRQMReadonly.dicType.IsAssignableFrom(type))
                    {
                        Type baseType = type.BaseType;
                        while (!baseType.IsGenericType)
                        {
                            baseType = baseType.BaseType;
                        }
                        instanceObject.ArgTypes = baseType.GetGenericArguments();
                        instanceObject.AddMethod = type.GetMethod("Add");
                        instanceObject.instanceType = InstanceType.Dictionary;
                    }
                    else
                    {
                        instanceObject.instanceType = InstanceType.Class;
                    }
                    return instanceObject;
                });
                return typeInfo;
            }
            return null;
        }
    }
}