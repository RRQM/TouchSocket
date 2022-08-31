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
#if HAVE_BINARY_SERIALIZATION && !HAVE_BINARY_FORMATTER

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Serialization
{
    internal class FormatterConverter : IFormatterConverter
    {
        public object Convert(object value, Type type)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }

        public object Convert(object value, TypeCode typeCode)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ChangeType(value, typeCode, CultureInfo.InvariantCulture);
        }

        public bool ToBoolean(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        }

        public byte ToByte(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToByte(value, CultureInfo.InvariantCulture);
        }

        public char ToChar(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToChar(value, CultureInfo.InvariantCulture);
        }

        public DateTime ToDateTime(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToDateTime(value, CultureInfo.InvariantCulture);
        }

        public decimal ToDecimal(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        public double ToDouble(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        public short ToInt16(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToInt16(value, CultureInfo.InvariantCulture);
        }

        public int ToInt32(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        public long ToInt64(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToInt64(value, CultureInfo.InvariantCulture);
        }

        public sbyte ToSByte(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToSByte(value, CultureInfo.InvariantCulture);
        }

        public float ToSingle(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }

        public string ToString(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public ushort ToUInt16(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToUInt16(value, CultureInfo.InvariantCulture);
        }

        public uint ToUInt32(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToUInt32(value, CultureInfo.InvariantCulture);
        }

        public ulong ToUInt64(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToUInt64(value, CultureInfo.InvariantCulture);
        }
    }
}

#endif