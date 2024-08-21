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
using System.Buffers;
using System.Collections.Generic;
using System.IO;

namespace TouchSocket.Core
{
    /// <summary>
    /// 定义了一个可释放的字节块接口，用于支持将字节数据写入缓冲区。
    /// 该接口继承自<see cref="IDisposable"/>和<see cref="IBufferWriter{T}"/>，表明它既是一个可释放资源的对象，
    /// 也是一个能够将字节数据写入缓冲区的对象。
    /// </summary>
    public interface IByteBlock : IDisposable, IBufferWriter<byte>
    {
        #region 属性

        /// <summary>
        /// 获取是否可以读取。
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// 获取可以读取的长度。
        /// </summary>
        int CanReadLength { get; }

        /// <summary>
        /// 获取容量。
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// 获取空闲长度。
        /// </summary>
        int FreeLength { get; }

        /// <summary>
        /// 获取是否持有数据。
        /// </summary>
        bool Holding { get; }

        /// <summary>
        /// 获取是否为结构类型。
        /// </summary>
        bool IsStruct { get; }

        /// <summary>
        /// 获取长度。
        /// </summary>
        int Length { get; }

        /// <summary>
        /// 获取只读内存。
        /// </summary>
        ReadOnlyMemory<byte> Memory { get; }

        /// <summary>
        /// 获取或设置位置。
        /// </summary>
        int Position { get; set; }

        /// <summary>
        /// 获取只读字节跨度。
        /// </summary>
        ReadOnlySpan<byte> Span { get; }

        /// <summary>
        /// 获取总内存。
        /// </summary>
        Memory<byte> TotalMemory { get; }

        /// <summary>
        /// 获取是否正在使用。
        /// </summary>
        bool Using { get; }

        /// <summary>
        /// 获取字节池。
        /// </summary>
        BytePool BytePool { get; }

        /// <summary>
        /// 获取或设置指定索引处的字节。
        /// </summary>
        /// <param name="index">索引位置。</param>
        /// <value>指定索引处的字节。</value>
        byte this[int index] { get; set; }

        #endregion 属性

        /// <summary>
        /// 清空当前数据结构中的所有数据。
        /// </summary>
        void Clear();

        #region Boolean

        /// <summary>
        /// 读取一个布尔值。
        /// </summary>
        /// <returns>返回读取的布尔值。</returns>
        bool ReadBoolean();

        /// <summary>
        /// 读取一个布尔值数组。
        /// </summary>
        /// <returns>返回读取的布尔值数组。</returns>
        bool[] ReadBooleans();

        /// <summary>
        /// 从位数据中转换为布尔值集合。
        /// </summary>
        /// <returns>返回转换得到的布尔值集合。</returns>
        IEnumerable<bool> ToBoolensFromBit();

        /// <summary>
        /// 从字节数据中转换为布尔值集合。
        /// </summary>
        /// <returns>返回转换得到的布尔值集合。</returns>
        IEnumerable<bool> ToBoolensFromByte();

        /// <summary>
        /// 写入一个布尔值。
        /// </summary>
        /// <param name="value">要写入的布尔值。</param>
        void WriteBoolean(bool value);

        /// <summary>
        /// 写入一个布尔值数组。
        /// </summary>
        /// <param name="values">要写入的布尔值数组。</param>
        void WriteBooleans(bool[] values);

        #endregion Boolean

        #region Byte

        /// <summary>
        /// 读取一个字节。
        /// </summary>
        /// <returns>返回读取到的字节。</returns>
        byte ReadByte();

        /// <summary>
        /// 写入一个字节。
        /// </summary>
        /// <param name="value">要写入的字节值。</param>
        void WriteByte(byte value);

        #endregion Byte

        #region Write

        /// <summary>
        /// 将指定的字节序列写入到当前内存的<see cref="Position"/>处。
        /// </summary>
        /// <param name="span">一个只读的字节序列，表示要写入的数据。</param>
        void Write(ReadOnlySpan<byte> span);

        #endregion Write

        #region Read

        /// <summary>
        /// 从数据源读取字节并将其写入指定的字节范围。
        /// </summary>
        /// <param name="span">要写入读取字节的字节范围。</param>
        /// <returns>实际写入字节范围的字节数。</returns>
        int Read(Span<byte> span);

        #endregion Read

        #region ByteBlock

        /// <summary>
        /// 读取一个字节块。
        /// </summary>
        /// <returns>返回读取到的字节块。</returns>
        ByteBlock ReadByteBlock();

        /// <summary>
        /// 写入一个字节块。
        /// </summary>
        /// <param name="byteBlock">要写入的字节块。</param>
        void WriteByteBlock(ByteBlock byteBlock);

        #endregion ByteBlock

        #region Char

        /// <summary>
        /// 从输入流中读取一个字符。
        /// </summary>
        /// <returns>返回读取的字符。</returns>
        char ReadChar();

        /// <summary>
        /// 根据指定的字节序从输入流中读取一个字符。
        /// </summary>
        /// <param name="endianType">指定的字节序类型。</param>
        /// <returns>返回读取的字符。</returns>
        char ReadChar(EndianType endianType);

        /// <summary>
        /// 从输入流中读取一系列字符。
        /// </summary>
        /// <returns>返回一个字符序列。</returns>
        IEnumerable<char> ToChars();

        /// <summary>
        /// 根据指定的字节序从输入流中读取一系列字符。
        /// </summary>
        /// <param name="endianType">指定的字节序类型。</param>
        /// <returns>返回一个字符序列。</returns>
        IEnumerable<char> ToChars(EndianType endianType);

        /// <summary>
        /// 向输出流写入一个字符。
        /// </summary>
        /// <param name="value">要写入的字符。</param>
        void WriteChar(char value);

        /// <summary>
        /// 根据指定的字节序向输出流写入一个字符。
        /// </summary>
        /// <param name="value">要写入的字符。</param>
        /// <param name="endianType">指定的字节序类型。</param>
        void WriteChar(char value, EndianType endianType);

        #endregion Char

        #region Decimal

        /// <summary>
        /// 读取一个字节序列并将其转换为十进制数。
        /// </summary>
        /// <returns>转换后的十进制数。</returns>
        decimal ReadDecimal();

        /// <summary>
        /// 根据指定的字节序读取一个字节序列并将其转换为十进制数。
        /// </summary>
        /// <param name="endianType">指定的字节序。</param>
        /// <returns>转换后的十进制数。</returns>
        decimal ReadDecimal(EndianType endianType);

        /// <summary>
        /// 将一系列字节序列转换为十进制数的集合。
        /// </summary>
        /// <returns>转换后的十进制数集合。</returns>
        IEnumerable<decimal> ToDecimals();

        /// <summary>
        /// 根据指定的字节序将一系列字节序列转换为十进制数的集合。
        /// </summary>
        /// <param name="endianType">指定的字节序。</param>
        /// <returns>转换后的十进制数集合。</returns>
        IEnumerable<decimal> ToDecimals(EndianType endianType);

        /// <summary>
        /// 将一个十进制数转换为字节序列并写入。
        /// </summary>
        /// <param name="value">要转换并写入的十进制数。</param>
        void WriteDecimal(decimal value);

        /// <summary>
        /// 根据指定的字节序将一个十进制数转换为字节序列并写入。
        /// </summary>
        /// <param name="value">要转换并写入的十进制数。</param>
        /// <param name="endianType">指定的字节序。</param>
        void WriteDecimal(decimal value, EndianType endianType);

        #endregion Decimal

        #region Float

        /// <summary>
        /// 读取一个浮点数。
        /// </summary>
        /// <returns>读取到的浮点数。</returns>
        float ReadFloat();

        /// <summary>
        /// 按指定的字节序读取一个浮点数。
        /// </summary>
        /// <param name="endianType">指定的字节序类型。</param>
        /// <returns>读取到的浮点数。</returns>
        float ReadFloat(EndianType endianType);

        /// <summary>
        /// 读取一系列浮点数。
        /// </summary>
        /// <returns>一个浮点数的集合。</returns>
        IEnumerable<float> ToFloats();

        /// <summary>
        /// 按指定的字节序读取一系列浮点数。
        /// </summary>
        /// <param name="endianType">指定的字节序类型。</param>
        /// <returns>一个浮点数的集合。</returns>
        IEnumerable<float> ToFloats(EndianType endianType);

        /// <summary>
        /// 写入一个浮点数。
        /// </summary>
        /// <param name="value">要写入的浮点数值。</param>
        void WriteFloat(float value);

        /// <summary>
        /// 按指定的字节序写入一个浮点数。
        /// </summary>
        /// <param name="value">要写入的浮点数值。</param>
        /// <param name="endianType">指定的字节序类型。</param>
        void WriteFloat(float value, EndianType endianType);

        #endregion Float

        #region Int64

        /// <summary>
        /// 读取一个 Int64 类型的值。
        /// </summary>
        /// <returns>读取到的 Int64 类型的值。</returns>
        long ReadInt64();

        /// <summary>
        /// 按指定的字节序读取一个 Int64 类型的值。
        /// </summary>
        /// <param name="endianType">指定的字节序类型。</param>
        /// <returns>读取到的 Int64 类型的值。</returns>
        long ReadInt64(EndianType endianType);

        /// <summary>
        /// 将当前对象转换为一系列 Int64 类型的值。
        /// </summary>
        /// <returns>一系列 Int64 类型的值。</returns>
        IEnumerable<long> ToInt64s();

        /// <summary>
        /// 按指定的字节序将当前对象转换为一系列 Int64 类型的值。
        /// </summary>
        /// <param name="endianType">指定的字节序类型。</param>
        /// <returns>一系列 Int64 类型的值。</returns>
        IEnumerable<long> ToInt64s(EndianType endianType);

        /// <summary>
        /// 写入一个 Int64 类型的值。
        /// </summary>
        /// <param name="value">要写入的 Int64 类型的值。</param>
        void WriteInt64(long value);

        /// <summary>
        /// 按指定的字节序写入一个 Int64 类型的值。
        /// </summary>
        /// <param name="value">要写入的 Int64 类型的值。</param>
        /// <param name="endianType">指定的字节序类型。</param>
        void WriteInt64(long value, EndianType endianType);

        #endregion Int64

        #region Null

        /// <summary>
        /// 读取是否为null。
        /// </summary>
        /// <returns>如果读取内容为null，则返回true；否则返回false。</returns>
        bool ReadIsNull();

        /// <summary>
        /// 写入一个值，并标记该值是否为null。
        /// </summary>
        /// <param name="t">要写入的值。</param>
        /// <typeparam name="T">值的类型，必须是引用类型。</typeparam>
        void WriteIsNull<T>(T t) where T : class;

        /// <summary>
        /// 写入一个可能为null的值，并标记该值是否为null。
        /// </summary>
        /// <param name="t">要写入的值，可以为null。</param>
        /// <typeparam name="T">值的类型，必须是值类型。</typeparam>
        void WriteIsNull<T>(T? t) where T : struct;

        /// <summary>
        /// 写入一个非null的标记。
        /// </summary>
        void WriteNotNull();

        /// <summary>
        /// 写入一个null的标记。
        /// </summary>
        void WriteNull();

        #endregion Null

        #region Guid

        /// <summary>
        /// 读取一个全局唯一标识符（GUID）。
        /// </summary>
        /// <returns>返回读取到的GUID。</returns>
        Guid ReadGuid();

        /// <summary>
        /// 写入一个全局唯一标识符（GUID）。
        /// </summary>
        /// <param name="value">要写入的GUID，使用in修饰符确保参数只读且不复制。</param>
        void WriteGuid(in Guid value);
        #endregion Guid

        #region TimeSpan

        /// <summary>
        /// 读取一个时间间隔。
        /// </summary>
        /// <returns>返回读取的时间间隔。</returns>
        TimeSpan ReadTimeSpan();

        /// <summary>
        /// 将当前对象转换为一系列时间间隔。
        /// </summary>
        /// <returns>返回一个时间间隔的集合。</returns>
        IEnumerable<TimeSpan> ToTimeSpans();

        /// <summary>
        /// 写入一个时间间隔。
        /// </summary>
        /// <param name="value">要写入的时间间隔。</param>
        void WriteTimeSpan(TimeSpan value);

        #endregion TimeSpan

        #region UInt32

        /// <summary>
        /// 读取一个无符号32位整数，使用默认的大端字节序。
        /// </summary>
        /// <returns>读取到的无符号32位整数。</returns>
        uint ReadUInt32();

        /// <summary>
        /// 按指定的字节序读取一个无符号32位整数。
        /// </summary>
        /// <param name="endianType">指定的字节序类型。</param>
        /// <returns>读取到的无符号32位整数。</returns>
        uint ReadUInt32(EndianType endianType);

        /// <summary>
        /// 将输入的数据流转换为一系列无符号32位整数，使用默认的大端字节序。
        /// </summary>
        /// <returns>转换得到的一系列无符号32位整数。</returns>
        IEnumerable<uint> ToUInt32s();

        /// <summary>
        /// 按指定的字节序将输入的数据流转换为一系列无符号32位整数。
        /// </summary>
        /// <param name="endianType">指定的字节序类型。</param>
        /// <returns>转换得到的一系列无符号32位整数。</returns>
        IEnumerable<uint> ToUInt32s(EndianType endianType);

        /// <summary>
        /// 将无符号32位整数写入输出流，使用默认的大端字节序。
        /// </summary>
        /// <param name="value">要写入的无符号32位整数。</param>
        void WriteUInt32(uint value);

        /// <summary>
        /// 按指定的字节序将无符号32位整数写入输出流。
        /// </summary>
        /// <param name="value">要写入的无符号32位整数。</param>
        /// <param name="endianType">指定的字节序类型。</param>
        void WriteUInt32(uint value, EndianType endianType);

        #endregion UInt32

        #region UInt64

        /// <summary>
        /// 读取一个无符号64位整数。
        /// </summary>
        /// <returns>读取到的无符号64位整数。</returns>
        ulong ReadUInt64();

        /// <summary>
        /// 按指定字节序读取一个无符号64位整数。
        /// </summary>
        /// <param name="endianType">指定的字节序。</param>
        /// <returns>读取到的无符号64位整数。</returns>
        ulong ReadUInt64(EndianType endianType);

        /// <summary>
        /// 将输入流中的数据全部转换为无符号64位整数的集合。
        /// </summary>
        /// <returns>转换得到的无符号64位整数集合。</returns>
        IEnumerable<ulong> ToUInt64s();

        /// <summary>
        /// 按指定字节序将输入流中的数据全部转换为无符号64位整数的集合。
        /// </summary>
        /// <param name="endianType">指定的字节序。</param>
        /// <returns>转换得到的无符号64位整数集合。</returns>
        IEnumerable<ulong> ToUInt64s(EndianType endianType);

        /// <summary>
        /// 写入一个无符号64位整数。
        /// </summary>
        /// <param name="value">要写入的无符号64位整数值。</param>
        void WriteUInt64(ulong value);

        /// <summary>
        /// 按指定字节序写入一个无符号64位整数。
        /// </summary>
        /// <param name="value">要写入的无符号64位整数值。</param>
        /// <param name="endianType">指定的字节序。</param>
        void WriteUInt64(ulong value, EndianType endianType);
        #endregion UInt64

        #region Seek

        /// <summary>
        /// 将流定位到指定位置。
        /// </summary>
        /// <param name="position">要定位到的位置。</param>
        void Seek(int position);

        /// <summary>
        /// 将流定位到相对于指定起始点的偏移位置。
        /// </summary>
        /// <param name="offset">从起始点开始的偏移量。</param>
        /// <param name="origin">偏移量的起始点。</param>
        /// <returns>新的位置。</returns>
        int Seek(int offset, SeekOrigin origin);

        /// <summary>
        /// 将流定位到末尾位置。
        /// </summary>
        void SeekToEnd();

        /// <summary>
        /// 将流定位到起始位置。
        /// </summary>
        void SeekToStart();
        #endregion Seek

        /// <summary>
        /// 将指定长度的数据读取到只读字节跨度中。
        /// </summary>
        /// <param name="length">要读取的数据长度。</param>
        /// <returns>包含读取数据的只读字节跨度。</returns>
        ReadOnlySpan<byte> ReadToSpan(int length);

        /// <summary>
        /// 重置当前对象的状态到初始状态。
        /// </summary>
        void Reset();

        /// <summary>
        /// 设置当前对象的容量。
        /// </summary>
        /// <param name="size">新的容量大小。</param>
        /// <param name="retainedData">是否保留原有数据，默认为false。</param>
        void SetCapacity(int size, bool retainedData = false);

        /// <summary>
        /// 设置当前对象是否处于持有状态。
        /// </summary>
        /// <param name="holding">true表示处于持有状态，false表示非持有状态。</param>
        void SetHolding(bool holding);

        /// <summary>
        /// 设置当前对象的长度。
        /// </summary>
        /// <param name="value">新的长度值。</param>
        void SetLength(int value);

        #region ToArray

        /// <summary>
        /// 将当前对象转换为一个新的字节数组。
        /// </summary>
        /// <returns>一个新的字节数组，表示当前对象。</returns>
        byte[] ToArray();

        /// <summary>
        /// 将当前对象转换为一个新的字节数组，并在新数组的开头留出指定的偏移量。
        /// </summary>
        /// <param name="offset">新数组开头的偏移量。</param>
        /// <returns>一个新的字节数组，表示当前对象，开头有指定的偏移量。</returns>
        byte[] ToArray(int offset);

        /// <summary>
        /// 将当前对象转换为一个新的字节数组，指定新数组的长度。
        /// </summary>
        /// <param name="offset">新数组的起始偏移量。</param>
        /// <param name="length">新数组的长度。</param>
        /// <returns>一个新的字节数组，表示当前对象的指定长度。</returns>
        byte[] ToArray(int offset, int length);

        /// <summary>
        /// 将当前对象转换为一个新的字节数组，该数组包含当前对象的所有元素。
        /// </summary>
        /// <returns>一个新的字节数组，包含当前对象的所有元素。</returns>
        byte[] ToArrayTake();

        /// <summary>
        /// 将当前对象转换为一个新的字节数组，该数组的长度由指定的长度参数决定。
        /// </summary>
        /// <param name="length">新数组的长度。</param>
        /// <returns>一个新的字节数组，其长度由指定的长度参数决定。</returns>
        byte[] ToArrayTake(int length);

        #endregion ToArray

        #region ToString

        /// <summary>
        /// 将当前实例转换为字符串表示形式。
        /// </summary>
        /// <returns>当前实例的字符串表示。</returns>
        string ToString();

        /// <summary>
        /// 将当前实例从指定位置转换为字符串表示形式。
        /// </summary>
        /// <param name="offset">开始转换的索引位置。</param>
        /// <returns>从指定位置开始的当前实例的字符串表示。</returns>
        string ToString(int offset);

        /// <summary>
        /// 将当前实例从指定位置开始，并在指定长度内转换为字符串表示形式。
        /// </summary>
        /// <param name="offset">开始转换的索引位置。</param>
        /// <param name="length">转换的字符数量。</param>
        /// <returns>从指定位置开始，并在指定长度内的当前实例的字符串表示。</returns>
        string ToString(int offset, int length);
        #endregion ToString

        #region DateTime

        /// <summary>
        /// 读取一个DateTime值。
        /// </summary>
        /// <returns>返回读取到的DateTime值。</returns>
        DateTime ReadDateTime();

        /// <summary>
        /// 将当前实例转换为一系列的DateTime值。
        /// </summary>
        /// <returns>返回一个IEnumerable&lt;DateTime&gt;，包含转换后的DateTime值。</returns>
        IEnumerable<DateTime> ToDateTimes();

        /// <summary>
        /// 写入一个DateTime值。
        /// </summary>
        /// <param name="value">要写入的DateTime值。</param>
        void WriteDateTime(DateTime value);

        #endregion DateTime

        #region Double

        /// <summary>
        /// 读取一个双精度浮点数。
        /// </summary>
        /// <returns>读取到的双精度浮点数。</returns>
        double ReadDouble();

        /// <summary>
        /// 按指定字节序读取一个双精度浮点数。
        /// </summary>
        /// <param name="endianType">指定的字节序。</param>
        /// <returns>读取到的双精度浮点数。</returns>
        double ReadDouble(EndianType endianType);

        /// <summary>
        /// 读取一系列双精度浮点数。
        /// </summary>
        /// <returns>一个包含读取到的双精度浮点数的序列。</returns>
        IEnumerable<double> ToDoubles();

        /// <summary>
        /// 按指定字节序读取一系列双精度浮点数。
        /// </summary>
        /// <param name="endianType">指定的字节序。</param>
        /// <returns>一个包含读取到的双精度浮点数的序列。</returns>
        IEnumerable<double> ToDoubles(EndianType endianType);

        /// <summary>
        /// 写入一个双精度浮点数。
        /// </summary>
        /// <param name="value">要写入的双精度浮点数。</param>
        void WriteDouble(double value);

        /// <summary>
        /// 按指定字节序写入一个双精度浮点数。
        /// </summary>
        /// <param name="value">要写入的双精度浮点数。</param>
        /// <param name="endianType">指定的字节序。</param>
        void WriteDouble(double value, EndianType endianType);
        #endregion Double

        #region Int32

        /// <summary>
        /// 读取一个32位整数。
        /// </summary>
        /// <returns>读取到的32位整数值。</returns>
        int ReadInt32();

        /// <summary>
        /// 按指定的字节序读取一个32位整数。
        /// </summary>
        /// <param name="endianType">指定的字节序类型。</param>
        /// <returns>读取到的32位整数值。</returns>
        int ReadInt32(EndianType endianType);

        /// <summary>
        /// 读取一系列32位整数。
        /// </summary>
        /// <returns>一系列32位整数值的集合。</returns>
        IEnumerable<int> ToInt32s();

        /// <summary>
        /// 按指定的字节序读取一系列32位整数。
        /// </summary>
        /// <param name="endianType">指定的字节序类型。</param>
        /// <returns>一系列32位整数值的集合。</returns>
        IEnumerable<int> ToInt32s(EndianType endianType);

        /// <summary>
        /// 写入一个32位整数。
        /// </summary>
        /// <param name="value">要写入的32位整数值。</param>
        void WriteInt32(int value);

        /// <summary>
        /// 按指定的字节序写入一个32位整数。
        /// </summary>
        /// <param name="value">要写入的32位整数值。</param>
        /// <param name="endianType">指定的字节序类型。</param>
        void WriteInt32(int value, EndianType endianType);

        #endregion Int32

        #region Int16

        /// <summary>
        /// 读取一个Int16类型的数据，使用默认的字节序。
        /// </summary>
        /// <returns>读取到的Int16数据。</returns>
        short ReadInt16();

        /// <summary>
        /// 按指定字节序读取一个Int16类型的数据。
        /// </summary>
        /// <param name="endianType">指定的字节序类型。</param>
        /// <returns>读取到的Int16数据。</returns>
        short ReadInt16(EndianType endianType);

        /// <summary>
        /// 读取一系列Int16类型的数据，使用默认的字节序。
        /// </summary>
        /// <returns>一系列读取到的Int16数据。</returns>
        IEnumerable<short> ToInt16s();

        /// <summary>
        /// 按指定字节序读取一系列Int16类型的数据。
        /// </summary>
        /// <param name="endianType">指定的字节序类型。</param>
        /// <returns>一系列读取到的Int16数据。</returns>
        IEnumerable<short> ToInt16s(EndianType endianType);

        /// <summary>
        /// 写入一个Int16类型的数据，使用默认的字节序。
        /// </summary>
        /// <param name="value">要写入的Int16数据。</param>
        void WriteInt16(short value);

        /// <summary>
        /// 按指定字节序写入一个Int16类型的数据。
        /// </summary>
        /// <param name="value">要写入的Int16数据。</param>
        /// <param name="endianType">指定的字节序类型。</param>
        void WriteInt16(short value, EndianType endianType);

        #endregion Int16

        #region String

        /// <summary>
        /// 读取一个字符串。
        /// </summary>
        /// <param name="headerType">可选的固定头部类型，默认为Int类型。</param>
        /// <returns>读取到的字符串。</returns>
        string ReadString(FixedHeaderType headerType = FixedHeaderType.Int);

        /// <summary>
        /// 写入一个字符串。
        /// </summary>
        /// <param name="value">要写入的字符串值。</param>
        /// <param name="headerType">可选的固定头部类型，默认为Int类型。</param>
        void WriteString(string value, FixedHeaderType headerType = FixedHeaderType.Int);

        #endregion String

        #region UInt16

        /// <summary>
        /// 读取一个无符号16位整数。
        /// </summary>
        /// <returns>读取到的无符号16位整数。</returns>
        ushort ReadUInt16();

        /// <summary>
        /// 根据指定的字节顺序读取一个无符号16位整数。
        /// </summary>
        /// <param name="endianType">指定的字节顺序。</param>
        /// <returns>读取到的无符号16位整数。</returns>
        ushort ReadUInt16(EndianType endianType);

        /// <summary>
        /// 将输入流中的数据全部转换为无符号16位整数的集合。
        /// </summary>
        /// <returns>转换得到的无符号16位整数集合。</returns>
        IEnumerable<ushort> ToUInt16s();

        /// <summary>
        /// 根据指定的字节顺序，将输入流中的数据全部转换为无符号16位整数的集合。
        /// </summary>
        /// <param name="endianType">指定的字节顺序。</param>
        /// <returns>转换得到的无符号16位整数集合。</returns>
        IEnumerable<ushort> ToUInt16s(EndianType endianType);

        /// <summary>
        /// 写入一个无符号16位整数。
        /// </summary>
        /// <param name="value">要写入的无符号16位整数值。</param>
        void WriteUInt16(ushort value);

        /// <summary>
        /// 根据指定的字节顺序写入一个无符号16位整数。
        /// </summary>
        /// <param name="value">要写入的无符号16位整数值。</param>
        /// <param name="endianType">指定的字节顺序。</param>
        void WriteUInt16(ushort value, EndianType endianType);

        #endregion UInt16

        #region BytesPackage

        /// <summary>
        /// 读取字节流数据包。
        /// </summary>
        /// <returns>字节数组</returns>
        byte[] ReadBytesPackage();

        /// <summary>
        /// 读取字节流数据包为只读内存。
        /// </summary>
        /// <returns>只读内存字节块，可能为null</returns>
        ReadOnlyMemory<byte>? ReadBytesPackageMemory();

        /// <summary>
        /// 写入字节流数据包。
        /// </summary>
        /// <param name="value">要写入的字节数组</param>
        void WriteBytesPackage(byte[] value);

        /// <summary>
        /// 从指定偏移量开始写入指定长度的字节流数据包。
        /// </summary>
        /// <param name="value">要写入的字节数组</param>
        /// <param name="offset">从数组中开始写入的索引位置</param>
        /// <param name="length">要写入的字节数</param>
        void WriteBytesPackage(byte[] value, int offset, int length);

        #endregion BytesPackage

        #region Package

        /// <summary>
        /// 读取指定类型的包装对象。
        /// </summary>
        /// <typeparam name="TPackage">包装对象的类型，必须是IPackage的实现。</typeparam>
        /// <returns>返回读取到的包装对象。</returns>
        TPackage ReadPackage<TPackage>() where TPackage : class, IPackage, new();

        /// <summary>
        /// 写入指定类型的包装对象。
        /// </summary>
        /// <typeparam name="TPackage">包装对象的类型，必须是IPackage的实现。</typeparam>
        /// <param name="package">要写入的包装对象。</param>
        void WritePackage<TPackage>(TPackage package) where TPackage : class, IPackage;

        #endregion Package

        #region VarUInt32

        /// <summary>
        /// 读取一个变量长度的无符号32位整数。
        /// </summary>
        /// <returns>返回读取到的无符号32位整数值。</returns>
        uint ReadVarUInt32();

        /// <summary>
        /// 写入一个变量长度的无符号32位整数。
        /// </summary>
        /// <param name="value">要写入的无符号32位整数值。</param>
        /// <returns>返回写入操作的结果，成功返回0，否则返回-1。</returns>
        int WriteVarUInt32(uint value);
        #endregion VarUInt32
    }
}