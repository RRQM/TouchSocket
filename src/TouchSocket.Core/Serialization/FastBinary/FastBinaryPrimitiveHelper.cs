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

namespace TouchSocket.Core;
/// <summary>
/// 原生类型（含字符串、时间、decimal 等）读写辅助。保持与原始协议编码完全一致（尤其 sbyte 仍按短整型写入以保证兼容）。
/// </summary>
internal static class FastBinaryPrimitiveHelper
{
    /// <summary>
    /// 写入已知原生类型。成功返回 true。
    /// </summary>
    public static bool TryWritePrimitive<TWriter, T>(ref TWriter writer, T value)
        where TWriter : IBytesWriter
    {
        switch (value)
        {
            case byte v: WriterExtension.WriteValue<TWriter, byte>(ref writer, v); return true;
            case sbyte v: WriterExtension.WriteValue<TWriter, short>(ref writer, v); return true; // 协议历史：按短整型
            case bool v: WriterExtension.WriteValue<TWriter, bool>(ref writer, v); return true;
            case short v: WriterExtension.WriteValue<TWriter, short>(ref writer, v); return true;
            case ushort v: WriterExtension.WriteValue<TWriter, ushort>(ref writer, v); return true;
            case int v: WriterExtension.WriteValue<TWriter, int>(ref writer, v); return true;
            case uint v: WriterExtension.WriteValue<TWriter, uint>(ref writer, v); return true;
            case long v: WriterExtension.WriteValue<TWriter, long>(ref writer, v); return true;
            case ulong v: WriterExtension.WriteValue<TWriter, ulong>(ref writer, v); return true;
            case float v: WriterExtension.WriteValue<TWriter, float>(ref writer, v); return true;
            case double v: WriterExtension.WriteValue<TWriter, double>(ref writer, v); return true;
            case char v: WriterExtension.WriteValue<TWriter, char>(ref writer, v); return true;
            case decimal v: WriterExtension.WriteValue<TWriter, decimal>(ref writer, v); return true;
            case DateTime v: WriterExtension.WriteValue<TWriter, DateTime>(ref writer, v); return true;
            case TimeSpan v: WriterExtension.WriteValue<TWriter, TimeSpan>(ref writer, v); return true;
            case string s: WriterExtension.WriteString(ref writer, s); return true;
        }
        return false;
    }

    /// <summary>
    /// 读取基础类型。若处理则返回 true。
    /// </summary>
    public static bool TryReadPrimitive<TReader>(ref TReader reader, Type type, out object value)
        where TReader : IBytesReader
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean: value = ReaderExtension.ReadValue<TReader, bool>(ref reader); return true;
            case TypeCode.Char: value = ReaderExtension.ReadValue<TReader, char>(ref reader); return true;
            case TypeCode.SByte: value = (sbyte)ReaderExtension.ReadValue<TReader, short>(ref reader); return true; // 按短整型读取
            case TypeCode.Byte: value = ReaderExtension.ReadValue<TReader, byte>(ref reader); return true;
            case TypeCode.Int16: value = ReaderExtension.ReadValue<TReader, short>(ref reader); return true;
            case TypeCode.UInt16: value = ReaderExtension.ReadValue<TReader, ushort>(ref reader); return true;
            case TypeCode.Int32: value = ReaderExtension.ReadValue<TReader, int>(ref reader); return true;
            case TypeCode.UInt32: value = ReaderExtension.ReadValue<TReader, uint>(ref reader); return true;
            case TypeCode.Int64: value = ReaderExtension.ReadValue<TReader, long>(ref reader); return true;
            case TypeCode.UInt64: value = ReaderExtension.ReadValue<TReader, ulong>(ref reader); return true;
            case TypeCode.Single: value = ReaderExtension.ReadValue<TReader, float>(ref reader); return true;
            case TypeCode.Double: value = ReaderExtension.ReadValue<TReader, double>(ref reader); return true;
            case TypeCode.Decimal: value = ReaderExtension.ReadValue<TReader, decimal>(ref reader); return true;
            case TypeCode.DateTime: value = ReaderExtension.ReadValue<TReader, DateTime>(ref reader); return true;
            case TypeCode.String: value = ReaderExtension.ReadString(ref reader); return true;
            default:
                value = null;
                return false;
        }
    }
}