using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    public interface IByteBlock : IDisposable
    {
        bool CanRead { get; }
        int CanReadLength { get; }
        int Capacity { get; }
        int FreeLength { get; }
        bool Holding { get; }
        int Length { get; }
        int Position { get; set; }
        ReadOnlySpan<byte> Span { get; }
        Memory<byte> TotalMemory { get; }
        ReadOnlyMemory<byte> Memory { get; }
        bool Using { get; }
        byte this[int index] { get; set; }

        ArraySegment<byte> AsSegment();

        ArraySegment<byte> AsSegment(int offset);

        ArraySegment<byte> AsSegment(int offset, int length);

        ArraySegment<byte> AsSegmentTake();

        ArraySegment<byte> AsSegmentTake(int length);

        void Clear();

        IEnumerator<byte> GetEnumerator();

        bool ReadBoolean();

        bool[] ReadBooleans();

        byte ReadByte();

        ByteBlock ReadByteBlock();

        byte[] ReadBytesPackage();

        Memory<byte>? ReadBytesPackageMemory();

        char ReadChar();

        char ReadChar(EndianType endianType);

        DateTime ReadDateTime();

        decimal ReadDecimal();

        decimal ReadDecimal(EndianType endianType);

        double ReadDouble();

        double ReadDouble(EndianType endianType);

        float ReadFloat();

        float ReadFloat(EndianType endianType);

        short ReadInt16();

        short ReadInt16(EndianType endianType);

        int ReadInt32();

        int ReadInt32(EndianType endianType);

        long ReadInt64();

        long ReadInt64(EndianType endianType);

        bool ReadIsNull();

        //T ReadObject<T>(SerializationType serializationType = SerializationType.FastBinary);

        TPackage ReadPackage<TPackage>() where TPackage : class, IPackage, new();

        string ReadString(FixedHeaderType headerType = FixedHeaderType.Int);

        TimeSpan ReadTimeSpan();

        ushort ReadUInt16();

        ushort ReadUInt16(EndianType endianType);

        uint ReadUInt32();

        uint ReadUInt32(EndianType endianType);

        ulong ReadUInt64();

        ulong ReadUInt64(EndianType endianType);

        void Reset();

        void Seek(int position);

        int Seek(int offset, SeekOrigin origin);

        void SeekToEnd();

        void SeekToStart();

        void SetCapacity(int size, bool retainedData = false);

        void SetHolding(bool holding);

        void SetLength(int value);

        byte[] ToArray();

        byte[] ToArray(int offset);

        byte[] ToArray(int offset, int length);

        byte[] ToArrayTake();

        byte[] ToArrayTake(int length);

        IEnumerable<bool> ToBoolensFromBit();

        IEnumerable<bool> ToBoolensFromByte();

        IEnumerable<char> ToChars();

        IEnumerable<char> ToChars(EndianType endianType);

        IEnumerable<DateTime> ToDateTimes();

        IEnumerable<decimal> ToDecimals();

        IEnumerable<decimal> ToDecimals(EndianType endianType);

        IEnumerable<double> ToDoubles();

        IEnumerable<double> ToDoubles(EndianType endianType);

        IEnumerable<float> ToFloats();

        IEnumerable<float> ToFloats(EndianType endianType);

        IEnumerable<short> ToInt16s();

        IEnumerable<short> ToInt16s(EndianType endianType);

        IEnumerable<int> ToInt32s();

        IEnumerable<int> ToInt32s(EndianType endianType);

        IEnumerable<long> ToInt64s();

        IEnumerable<long> ToInt64s(EndianType endianType);

        Memory<byte> ToMemory();

        Memory<byte> ToMemory(int offset);

        Memory<byte> ToMemory(int offset, int length);

        Memory<byte> ToMemoryTake();

        Memory<byte> ToMemoryTake(int length);

        string ToString();

        string ToString(int offset);

        string ToString(int offset, int length);

        IEnumerable<TimeSpan> ToTimeSpans();

        IEnumerable<ushort> ToUInt16s();

        IEnumerable<ushort> ToUInt16s(EndianType endianType);

        IEnumerable<uint> ToUInt32s();

        IEnumerable<uint> ToUInt32s(EndianType endianType);

        IEnumerable<ulong> ToUInt64s();

        IEnumerable<ulong> ToUInt64s(EndianType endianType);

        void Write(bool value);

        void Write(bool[] values);

        void Write(byte value);

        void Write(byte[] buffer, int offset, int count);

        void Write(char value);

        void Write(char value, EndianType endianType);

        void Write(DateTime value);

        void Write(decimal value);

        void Write(decimal value, EndianType endianType);

        void Write(double value);

        void Write(double value, EndianType endianType);

        void Write(float value);

        void Write(float value, EndianType endianType);

        void Write(int value);

        void Write(int value, EndianType endianType);

        void Write(long value);

        void Write(long value, EndianType endianType);

        void Write(short value);

        void Write(short value, EndianType endianType);

        void Write(ReadOnlySpan<byte> span);

        void Write(string value, FixedHeaderType headerType = FixedHeaderType.Int);

        void Write(TimeSpan value);

        void Write(uint value);

        void Write(uint value, EndianType endianType);

        void Write(ulong value);

        void Write(ulong value, EndianType endianType);

        void Write(ushort value);

        void Write(ushort value, EndianType endianType);

        void WriteByteBlock(ByteBlock byteBlock);

        void WriteBytesPackage(byte[] value);

        void WriteBytesPackage(byte[] value, int offset, int length);

        void WriteIsNull<T>(T t) where T : class;

        void WriteIsNull<T>(T? t) where T : struct;

        void WriteNotNull();

        void WriteNull();

        //void WriteObject(object value, SerializationType serializationType = SerializationType.FastBinary);

        void WritePackage<TPackage>(TPackage package) where TPackage : class, IPackage;

        void WriteString(string value, Encoding encoding = null);
        int Read(byte[] buffer, int offset, int length);
        int Read(byte[] buffer);
        int Read(out byte[] buffer, int length);
        byte[] ReadToArray(int length);
        int WriteVarUInt32(uint value);
        void Write(in Guid value);
        Guid ReadGuid();
    }
}