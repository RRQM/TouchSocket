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
using System.Text;
using TouchSocket.Core.Serialization;
using TouchSocket.Core.XREF.Newtonsoft.Json;

namespace TouchSocket.Core.ByteManager
{
    /// <summary>
    /// ByteBlock扩展
    /// </summary>
    public static class ByteBlockExtensions
    {
        /// <summary>
        /// 读取数据，然后递增Pos
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static int Read<T>(this T byteBlock, byte[] buffer) where T : IByteBlock
        {
            return byteBlock.Read(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 设置游标到末位
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns></returns>
        public static T SeekToEnd<T>(this T byteBlock) where T : IByteBlock
        {
            byteBlock.Position = byteBlock.Length;
            return byteBlock;
        }

        /// <summary>
        /// 移动游标
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="byteBlock"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static T Seek<T>(this T byteBlock, int position) where T : IByteBlock
        {
            byteBlock.Position = position;
            return byteBlock;
        }

        /// <summary>
        /// 设置游标到首位
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns></returns>
        public static T SeekToStart<T>(this T byteBlock) where T : IByteBlock
        {
            byteBlock.Position = 0;
            return byteBlock;
        }

        /// <summary>
        /// 读取数据，然后递增Pos
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int Read<T>(this T byteBlock, out byte[] buffer, int length) where T : IByteBlock
        {
            buffer = new byte[length];
            return byteBlock.Read(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 转换为有效内存
        /// </summary>
        /// <returns></returns>
        public static byte[] ToArray<T>(this T byteBlock) where T : IByteBlock
        {
            return byteBlock.ToArray(0, byteBlock.Len);
        }

        /// <summary>
        /// 从指定位置转化到有效内存
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public static byte[] ToArray<T>(this T byteBlock, int offset) where T : IByteBlock
        {
            return byteBlock.ToArray(offset, byteBlock.Len - offset);
        }

        #region BytesPackage

        /// <summary>
        /// 从当前流位置读取一个独立的<see cref="byte"/>数组包
        /// </summary>
        public static byte[] ReadBytesPackage<T>(this T byteBlock) where T : IByteBlock
        {
            byte status = (byte)byteBlock.ReadByte();
            if (status == 0)
            {
                return null;
            }
            int length = byteBlock.ReadInt32();
            byte[] data = new byte[length];
            Array.Copy(byteBlock.Buffer, byteBlock.Pos, data, 0, length);
            byteBlock.Pos += length;
            return data;
        }

        /// <summary>
        /// 尝试获取数据包信息，方便从Buffer操作数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="pos"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static bool TryReadBytesPackageInfo<T>(this T byteBlock, out int pos, out int len) where T : IByteBlock
        {
            byte status = (byte)byteBlock.ReadByte();
            if (status == 0)
            {
                pos = 0;
                len = 0;
                return false;
            }
            len = byteBlock.ReadInt32();
            pos = byteBlock.Pos;
            return true;
        }

        /// <summary>
        /// 写入一个独立的<see cref="byte"/>数组包
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public static TByteBlock WriteBytesPackage<TByteBlock>(this TByteBlock byteBlock, byte[] value, int offset, int length) where TByteBlock : IByteBlock
        {
            if (value == null)
            {
                byteBlock.Write((byte)0);
            }
            else
            {
                byteBlock.Write((byte)1);
                byteBlock.Write(length);
                byteBlock.Write(value, offset, length);
            }
            return byteBlock;
        }

        /// <summary>
        /// 写入一个独立的<see cref="byte"/>数组包
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static TByteBlock WriteBytesPackage<TByteBlock>(this TByteBlock byteBlock, byte[] value) where TByteBlock : IByteBlock
        {
            if (value == null)
            {
                return byteBlock.WriteBytesPackage(value, 0, 0);
            }
            return byteBlock.WriteBytesPackage(value, 0, value.Length);
        }

        #endregion BytesPackage

        #region Int32

        /// <summary>
        /// 从当前流位置读取一个<see cref="int"/>值
        /// </summary>
        public static int ReadInt32(this IByteBlock byteBlock)
        {
            int value = TouchSocketBitConverter.Default.ToInt32(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 4;
            return value;
        }

        /// <summary>
        /// 写入<see cref="int"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static TByteBlock Write<TByteBlock>(this TByteBlock byteBlock, int value) where TByteBlock : IByteBlock
        {
            byteBlock.Write(TouchSocketBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion Int32

        #region Int16

        /// <summary>
        /// 从当前流位置读取一个<see cref="short"/>值
        /// </summary>
        public static short ReadInt16(this IByteBlock byteBlock)
        {
            short value = TouchSocketBitConverter.Default.ToInt16(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 2;
            return value;
        }

        /// <summary>
        /// 写入<see cref="short"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static TByteBlock Write<TByteBlock>(this TByteBlock byteBlock, short value) where TByteBlock : IByteBlock
        {
            byteBlock.Write(TouchSocketBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion Int16

        #region Int64

        /// <summary>
        /// 从当前流位置读取一个<see cref="long"/>值
        /// </summary>
        public static long ReadInt64(this IByteBlock byteBlock)
        {
            long value = TouchSocketBitConverter.Default.ToInt64(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 8;
            return value;
        }

        /// <summary>
        /// 写入<see cref="long"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static TByteBlock Write<TByteBlock>(this TByteBlock byteBlock, long value) where TByteBlock : IByteBlock
        {
            byteBlock.Write(TouchSocketBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion Int64

        #region Boolean

        /// <summary>
        /// 从当前流位置读取一个<see cref="bool"/>值
        /// </summary>
        public static bool ReadBoolean(this IByteBlock byteBlock)
        {
            bool value = TouchSocketBitConverter.Default.ToBoolean(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 1;
            return value;
        }

        /// <summary>
        /// 写入<see cref="bool"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static TByteBlock Write<TByteBlock>(this TByteBlock byteBlock, bool value) where TByteBlock : IByteBlock
        {
            byteBlock.Write(TouchSocketBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion Boolean

        #region Byte

        /// <summary>
        /// 写入<see cref="byte"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TByteBlock Write<TByteBlock>(this TByteBlock byteBlock, byte value) where TByteBlock : IByteBlock
        {
            byteBlock.Write(new byte[] { value }, 0, 1);
            return byteBlock;
        }

        #endregion Byte

        #region String

        /// <summary>
        /// 从当前流位置读取一个<see cref="string"/>值
        /// </summary>
        public static string ReadString(this IByteBlock byteBlock)
        {
            byte value = (byte)byteBlock.ReadByte();
            if (value == 0)
            {
                return null;
            }
            else if (value == 1)
            {
                return string.Empty;
            }
            else
            {
                ushort len = byteBlock.ReadUInt16();
                string str = Encoding.UTF8.GetString(byteBlock.Buffer, byteBlock.Pos, len);
                byteBlock.Pos += len;
                return str;
            }
        }

        /// <summary>
        /// 写入<see cref="string"/>值。
        /// <para>读取时必须使用ReadString</para>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static TByteBlock Write<TByteBlock>(this TByteBlock byteBlock, string value) where TByteBlock : IByteBlock
        {
            if (value == null)
            {
                byteBlock.Write((byte)0);
            }
            else if (value == string.Empty)
            {
                byteBlock.Write((byte)1);
            }
            else
            {
                byteBlock.Write((byte)2);
                byte[] buffer = Encoding.UTF8.GetBytes(value);
                if (buffer.Length > ushort.MaxValue)
                {
                    throw new Exception("传输长度超长");
                }
                byteBlock.Write((ushort)buffer.Length);
                byteBlock.Write(buffer);
            }
            return byteBlock;
        }

        #endregion String

        #region Char

        /// <summary>
        /// 从当前流位置读取一个<see cref="char"/>值
        /// </summary>
        public static char ReadChar(this IByteBlock byteBlock)
        {
            char value = TouchSocketBitConverter.Default.ToChar(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 2;
            return value;
        }

        /// <summary>
        /// 写入<see cref="char"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static TByteBlock Write<TByteBlock>(this TByteBlock byteBlock, char value) where TByteBlock : IByteBlock
        {
            byteBlock.Write(TouchSocketBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion Char

        #region Double

        /// <summary>
        /// 从当前流位置读取一个<see cref="double"/>值
        /// </summary>
        public static double ReadDouble(this IByteBlock byteBlock)
        {
            double value = TouchSocketBitConverter.Default.ToDouble(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 8;
            return value;
        }

        /// <summary>
        /// 写入<see cref="double"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static TByteBlock Write<TByteBlock>(this TByteBlock byteBlock, double value) where TByteBlock : IByteBlock
        {
            byteBlock.Write(TouchSocketBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion Double

        #region Float

        /// <summary>
        /// 从当前流位置读取一个<see cref="float"/>值
        /// </summary>
        public static float ReadFloat(this IByteBlock byteBlock)
        {
            float value = TouchSocketBitConverter.Default.ToSingle(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 4;
            return value;
        }

        /// <summary>
        /// 写入<see cref="float"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static TByteBlock Write<TByteBlock>(this TByteBlock byteBlock, float value) where TByteBlock : IByteBlock
        {
            byteBlock.Write(TouchSocketBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion Float

        #region UInt16

        /// <summary>
        /// 从当前流位置读取一个<see cref="ushort"/>值
        /// </summary>
        public static ushort ReadUInt16(this IByteBlock byteBlock)
        {
            ushort value = TouchSocketBitConverter.Default.ToUInt16(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 2;
            return value;
        }

        /// <summary>
        /// 写入<see cref="ushort"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static TByteBlock Write<TByteBlock>(this TByteBlock byteBlock, ushort value) where TByteBlock : IByteBlock
        {
            byteBlock.Write(TouchSocketBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion UInt16

        #region UInt32

        /// <summary>
        /// 从当前流位置读取一个<see cref="uint"/>值
        /// </summary>
        public static uint ReadUInt32(this IByteBlock byteBlock)
        {
            uint value = TouchSocketBitConverter.Default.ToUInt32(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 4;
            return value;
        }

        /// <summary>
        /// 写入<see cref="uint"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static TByteBlock Write<TByteBlock>(this TByteBlock byteBlock, uint value) where TByteBlock : IByteBlock
        {
            byteBlock.Write(TouchSocketBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion UInt32

        #region UInt64

        /// <summary>
        /// 从当前流位置读取一个<see cref="ulong"/>值
        /// </summary>
        public static ulong ReadUInt64(this IByteBlock byteBlock)
        {
            ulong value = TouchSocketBitConverter.Default.ToUInt64(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 8;
            return value;
        }

        /// <summary>
        /// 写入<see cref="ulong"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static TByteBlock Write<TByteBlock>(this TByteBlock byteBlock, ulong value) where TByteBlock : IByteBlock
        {
            byteBlock.Write(TouchSocketBitConverter.Default.GetBytes(value));
            return byteBlock;
        }

        #endregion UInt64

        #region DateTime

        /// <summary>
        /// 从当前流位置读取一个<see cref="DateTime"/>值
        /// </summary>
        public static DateTime ReadDateTime(this IByteBlock byteBlock)
        {
            long value = TouchSocketBitConverter.Default.ToInt64(byteBlock.Buffer, byteBlock.Pos);
            byteBlock.Pos += 8;
            return DateTime.FromBinary(value);
        }

        /// <summary>
        /// 写入<see cref="DateTime"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        public static TByteBlock Write<TByteBlock>(this TByteBlock byteBlock, DateTime value) where TByteBlock : IByteBlock
        {
            byteBlock.Write(TouchSocketBitConverter.Default.GetBytes(value.ToBinary()));
            return byteBlock;
        }

        #endregion DateTime

        #region Object

        /// <summary>
        ///  从当前流位置读取一个泛型值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="byteBlock"></param>
        /// <param name="serializationType"></param>
        /// <returns></returns>
        public static T ReadObject<T>(this IByteBlock byteBlock, SerializationType serializationType = SerializationType.FastBinary)
        {
            int length = byteBlock.ReadInt32();

            if (length == 0)
            {
                return default;
            }

            T obj;

            switch (serializationType)
            {
                case SerializationType.FastBinary:
                    {
                        obj = SerializeConvert.FastBinaryDeserialize<T>(byteBlock.Buffer, byteBlock.Pos);
                    }
                    break;

                case SerializationType.Json:
                    {
                        string jsonString = Encoding.UTF8.GetString(byteBlock.Buffer, byteBlock.Pos, length);
                        obj = JsonConvert.DeserializeObject<T>(jsonString);
                    }
                    break;

                default:
                    throw new Exception("未定义的序列化类型");
            }

            byteBlock.Pos += length;
            return obj;
        }

        /// <summary>
        /// 写入<see cref="object"/>值
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="value"></param>
        /// <param name="serializationType"></param>
        public static TByteBlock WriteObject<TByteBlock>(this TByteBlock byteBlock, object value, SerializationType serializationType = SerializationType.FastBinary)
            where TByteBlock : IByteBlock
        {
            if (value == null)
            {
                byteBlock.Write(0);
                return byteBlock;
            }
            byte[] data;
            switch (serializationType)
            {
                case SerializationType.FastBinary:
                    {
                        data = SerializeConvert.FastBinarySerialize(value);
                    }
                    break;

                case SerializationType.Json:
                    {
                        data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
                    }
                    break;

                default:
                    throw new Exception("未定义的序列化类型");
            }

            byteBlock.Write(data.Length);
            byteBlock.Write(data);
            return byteBlock;
        }

        #endregion Object
    }
}