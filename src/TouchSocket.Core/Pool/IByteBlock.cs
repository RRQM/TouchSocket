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
    public interface IByteBlock : IDisposable, IBufferWriter<byte>
    {
        #region 属性

        bool CanRead { get; }
        int CanReadLength { get; }
        int Capacity { get; }
        int FreeLength { get; }
        bool Holding { get; }
        bool IsStruct { get; }
        int Length { get; }
        ReadOnlyMemory<byte> Memory { get; }
        int Position { get; set; }
        ReadOnlySpan<byte> Span { get; }
        Memory<byte> TotalMemory { get; }
        bool Using { get; }
        BytePool BytePool { get; }

        byte this[int index] { get; set; }

        #endregion 属性

        #region AsSegment

        ArraySegment<byte> AsSegment();

        ArraySegment<byte> AsSegment(int offset);

        ArraySegment<byte> AsSegment(int offset, int length);

        ArraySegment<byte> AsSegmentTake();

        ArraySegment<byte> AsSegmentTake(int length);

        #endregion AsSegment

        void Clear();

        #region Boolean

        bool ReadBoolean();

        bool[] ReadBooleans();

        IEnumerable<bool> ToBoolensFromBit();

        IEnumerable<bool> ToBoolensFromByte();

        void WriteBoolean(bool value);

        void WriteBooleans(bool[] values);

        #endregion Boolean

        #region Byte

        byte ReadByte();

        void WriteByte(byte value);

        #endregion Byte

        #region Write

        void Write(ReadOnlySpan<byte> span);

        #endregion Write

        #region Read

        int Read(Span<byte> span);

        #endregion Read

        #region ByteBlock

        ByteBlock ReadByteBlock();

        void WriteByteBlock(ByteBlock byteBlock);

        #endregion ByteBlock

        #region Char

        char ReadChar();

        char ReadChar(EndianType endianType);

        IEnumerable<char> ToChars();

        IEnumerable<char> ToChars(EndianType endianType);

        void WriteChar(char value);

        void WriteChar(char value, EndianType endianType);

        #endregion Char

        #region Decimal

        decimal ReadDecimal();

        decimal ReadDecimal(EndianType endianType);

        IEnumerable<decimal> ToDecimals();

        IEnumerable<decimal> ToDecimals(EndianType endianType);

        void WriteDecimal(decimal value);

        void WriteDecimal(decimal value, EndianType endianType);

        #endregion Decimal

        #region Float

        float ReadFloat();

        float ReadFloat(EndianType endianType);

        IEnumerable<float> ToFloats();

        IEnumerable<float> ToFloats(EndianType endianType);

        void WriteFloat(float value);

        void WriteFloat(float value, EndianType endianType);

        #endregion Float

        #region Int64

        long ReadInt64();

        long ReadInt64(EndianType endianType);

        IEnumerable<long> ToInt64s();

        IEnumerable<long> ToInt64s(EndianType endianType);

        void WriteInt64(long value);

        void WriteInt64(long value, EndianType endianType);

        #endregion Int64

        #region Null

        bool ReadIsNull();

        void WriteIsNull<T>(T t) where T : class;

        void WriteIsNull<T>(T? t) where T : struct;

        void WriteNotNull();

        void WriteNull();

        #endregion Null

        #region Guid

        Guid ReadGuid();

        void WriteGuid(in Guid value);

        #endregion Guid

        #region TimeSpan

        TimeSpan ReadTimeSpan();

        IEnumerable<TimeSpan> ToTimeSpans();

        void WriteTimeSpan(TimeSpan value);

        #endregion TimeSpan

        #region UInt32

        uint ReadUInt32();

        uint ReadUInt32(EndianType endianType);

        IEnumerable<uint> ToUInt32s();

        IEnumerable<uint> ToUInt32s(EndianType endianType);

        void WriteUInt32(uint value);

        void WriteUInt32(uint value, EndianType endianType);

        #endregion UInt32

        #region UInt64

        ulong ReadUInt64();

        ulong ReadUInt64(EndianType endianType);

        IEnumerable<ulong> ToUInt64s();

        IEnumerable<ulong> ToUInt64s(EndianType endianType);

        void WriteUInt64(ulong value);

        void WriteUInt64(ulong value, EndianType endianType);

        #endregion UInt64

        #region Seek

        void Seek(int position);

        int Seek(int offset, SeekOrigin origin);

        void SeekToEnd();

        void SeekToStart();

        #endregion Seek

        ReadOnlySpan<byte> ReadToSpan(int length);

        void Reset();

        void SetCapacity(int size, bool retainedData = false);

        void SetHolding(bool holding);

        void SetLength(int value);

        #region ToArray

        byte[] ToArray();

        byte[] ToArray(int offset);

        byte[] ToArray(int offset, int length);

        byte[] ToArrayTake();

        byte[] ToArrayTake(int length);

        #endregion ToArray

        #region ToMemory

        Memory<byte> ToMemory();

        Memory<byte> ToMemory(int offset);

        Memory<byte> ToMemory(int offset, int length);

        Memory<byte> ToMemoryTake();

        Memory<byte> ToMemoryTake(int length);

        #endregion ToMemory

        #region ToString

        string ToString();

        string ToString(int offset);

        string ToString(int offset, int length);

        #endregion ToString

        #region DateTime

        DateTime ReadDateTime();

        IEnumerable<DateTime> ToDateTimes();

        void WriteDateTime(DateTime value);

        #endregion DateTime

        #region Double

        double ReadDouble();

        double ReadDouble(EndianType endianType);

        IEnumerable<double> ToDoubles();

        IEnumerable<double> ToDoubles(EndianType endianType);

        void WriteDouble(double value);

        void WriteDouble(double value, EndianType endianType);

        #endregion Double

        #region Int32

        int ReadInt32();

        int ReadInt32(EndianType endianType);

        IEnumerable<int> ToInt32s();

        IEnumerable<int> ToInt32s(EndianType endianType);

        void WriteInt32(int value);

        void WriteInt32(int value, EndianType endianType);

        #endregion Int32

        #region Int16

        short ReadInt16();

        short ReadInt16(EndianType endianType);

        IEnumerable<short> ToInt16s();

        IEnumerable<short> ToInt16s(EndianType endianType);

        void WriteInt16(short value);

        void WriteInt16(short value, EndianType endianType);

        #endregion Int16

        #region String

        string ReadString(FixedHeaderType headerType = FixedHeaderType.Int);

        void WriteString(string value, FixedHeaderType headerType = FixedHeaderType.Int);

        #endregion String

        #region UInt16

        ushort ReadUInt16();

        ushort ReadUInt16(EndianType endianType);

        IEnumerable<ushort> ToUInt16s();

        IEnumerable<ushort> ToUInt16s(EndianType endianType);

        void WriteUInt16(ushort value);

        void WriteUInt16(ushort value, EndianType endianType);

        #endregion UInt16

        #region BytesPackage

        byte[] ReadBytesPackage();

        Memory<byte>? ReadBytesPackageMemory();

        void WriteBytesPackage(byte[] value);

        void WriteBytesPackage(byte[] value, int offset, int length);

        #endregion BytesPackage

        #region Package

        TPackage ReadPackage<TPackage>() where TPackage : class, IPackage, new();

        void WritePackage<TPackage>(TPackage package) where TPackage : class, IPackage;

        #endregion Package

        #region VarUInt32

        uint ReadVarUInt32();

        int WriteVarUInt32(uint value);

        #endregion VarUInt32
    }
}