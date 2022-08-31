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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using TouchSocket.Core.ByteManager;

namespace TouchSocket.Core.Serialization
{
    /// <summary>
    /// 高性能序列化器
    /// </summary>
    public static class SerializeConvert
    {
#if NET45_OR_GREATER

        #region 普通二进制序列化

        /// <summary>
        /// 普通二进制序列化对象
        /// </summary>
        /// <param name="obj">数据对象</param>
        /// <returns></returns>
        public static byte[] BinarySerialize(object obj)
        {
            using (MemoryStream serializeStream = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(serializeStream, obj);
                return serializeStream.ToArray();
            }
        }

        /// <summary>
        /// 二进制序列化对象至文件
        /// </summary>
        /// <param name="obj">数据对象</param>
        /// <param name="path">路径</param>
        public static void BinarySerializeToFile(object obj, string path)
        {
            using (FileStream serializeStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(serializeStream, obj);
                serializeStream.Close();
            }
        }

        /// <summary>
        /// 二进制序列化对象
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        public static void BinarySerialize(Stream stream, object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
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
        public static T BinaryDeserialize<T>(byte[] data, int offset, int length, SerializationBinder binder = null)
        {
            using (MemoryStream DeserializeStream = new MemoryStream(data, offset, length))
            {
                DeserializeStream.Position = 0;
                BinaryFormatter bf = new BinaryFormatter();
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
        public static object BinaryDeserialize(byte[] data, int offset, int length, SerializationBinder binder = null)
        {
            using (MemoryStream DeserializeStream = new MemoryStream(data, offset, length))
            {
                DeserializeStream.Position = 0;
                BinaryFormatter bf = new BinaryFormatter();
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
        public static T BinaryDeserialize<T>(Stream stream, SerializationBinder binder = null)
        {
            BinaryFormatter bf = new BinaryFormatter();
            if (binder != null)
            {
                bf.Binder = binder;
            }
            return (T)bf.Deserialize(stream);
        }

        /// <summary>
        /// 将二进制文件数据反序列化为指定类型对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T BinaryDeserializeFromFile<T>(string path)
        {
            using (FileStream serializeStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                BinaryFormatter bf = new BinaryFormatter();
                return (T)bf.Deserialize(serializeStream);
            }
        }

        /// <summary>
        /// 将二进制数据反序列化为指定类型对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
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
        public static T BinaryDeserialize<T>(byte[] data, SerializationBinder binder = null)
        {
            return BinaryDeserialize<T>(data, 0, data.Length, binder);
        }

        #endregion 普通二进制反序列化

#endif

        #region Fast二进制序列化

        /// <summary>
        /// Fast二进制序列化对象
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static void FastBinarySerialize(ByteBlock stream, object obj)
        {
            FastBinaryFormatter bf = new FastBinaryFormatter();
            bf.Serialize(stream, obj);
        }

        /// <summary>
        /// Fast二进制序列化对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] FastBinarySerialize(object obj)
        {
            using (ByteBlock byteBlock = new ByteBlock())
            {
                FastBinarySerialize(byteBlock, obj);
                return byteBlock.ToArray();
            }
        }

        #endregion Fast二进制序列化

        #region Fast二进制反序列化

        /// <summary>
        /// Fast反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static T FastBinaryDeserialize<T>(byte[] data, int offset)
        {
            FastBinaryFormatter bf = new FastBinaryFormatter();
            return (T)bf.Deserialize(data, offset, typeof(T));
        }

        /// <summary>
        /// Fast反序列化
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object FastBinaryDeserialize(byte[] data, int offset, Type type)
        {
            FastBinaryFormatter bf = new FastBinaryFormatter();
            return bf.Deserialize(data, offset, type);
        }

        /// <summary>
        /// 从Byte[]中反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T FastBinaryDeserialize<T>(byte[] data)
        {
            return FastBinaryDeserialize<T>(data, 0);
        }

        #endregion Fast二进制反序列化

        #region Xml序列化和反序列化

        /// <summary>
        /// Xml序列化数据对象
        /// </summary>
        /// <param name="obj">数据对象</param>
        /// <param name="encoding">编码格式</param>
        /// <returns></returns>
        public static string XmlSerializeToString(object obj, Encoding encoding)
        {
            return encoding.GetString(XmlSerializeToBytes(obj));
        }

        /// <summary>
        /// Xml序列化数据对象
        /// </summary>
        /// <param name="obj">数据对象</param>
        /// <returns></returns>
        public static string XmlSerializeToString(object obj)
        {
            return XmlSerializeToString(obj, Encoding.UTF8);
        }

        /// <summary>
        /// Xml序列化数据对象
        /// </summary>
        /// <param name="obj">数据对象</param>
        /// <returns></returns>
        public static byte[] XmlSerializeToBytes(object obj)
        {
            using (MemoryStream fileStream = new MemoryStream())
            {
                XmlSerializer xml = new XmlSerializer(obj.GetType());
                xml.Serialize(fileStream, obj);
                return fileStream.ToArray();
            }
        }

        /// <summary>
        /// Xml序列化至文件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="path"></param>
        public static void XmlSerializeToFile(object obj, string path)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                XmlSerializer xml = new XmlSerializer(obj.GetType());
                xml.Serialize(fileStream, obj);
                fileStream.Close();
            }
        }

        /// <summary>
        /// Xml反序列化
        /// </summary>
        /// <typeparam name="T">反序列化类型</typeparam>
        /// <param name="datas">数据</param>
        /// <returns></returns>
        public static T XmlDeserializeFromBytes<T>(byte[] datas)
        {
            XmlSerializer xmlserializer = new XmlSerializer(typeof(T));
            using (Stream xmlstream = new MemoryStream(datas))
            {
                return (T)xmlserializer.Deserialize(xmlstream);
            }
        }

        /// <summary>
        /// Xml反序列化
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object XmlDeserializeFromBytes(byte[] datas, Type type)
        {
            XmlSerializer xmlserializer = new XmlSerializer(type);
            using (Stream xmlstream = new MemoryStream(datas))
            {
                return xmlserializer.Deserialize(xmlstream);
            }
        }

        /// <summary>
        /// Xml反序列化
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="xmlString">xml字符串</param>
        /// <param name="encoding">编码格式</param>
        /// <returns></returns>
        public static T XmlDeserializeFromString<T>(string xmlString, Encoding encoding)
        {
            XmlSerializer xmlserializer = new XmlSerializer(typeof(T));
            using (Stream xmlstream = new MemoryStream(encoding.GetBytes(xmlString)))
            {
                return (T)xmlserializer.Deserialize(xmlstream);
            }
        }

        /// <summary>
        /// Xml反序列化
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="xmlString">xml字符串</param>
        /// <returns></returns>
        public static T XmlDeserializeFromString<T>(string xmlString)
        {
            return XmlDeserializeFromString<T>(xmlString, Encoding.UTF8);
        }

        /// <summary>
        /// Xml反序列化
        /// </summary>
        /// <typeparam name="T">反序列化类型</typeparam>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static T XmlDeserializeFromFile<T>(string path)
        {
            using (Stream xmlstream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer xmlserializer = new XmlSerializer(typeof(T));
                return (T)xmlserializer.Deserialize(xmlstream);
            }
        }

        #endregion Xml序列化和反序列化
    }
}