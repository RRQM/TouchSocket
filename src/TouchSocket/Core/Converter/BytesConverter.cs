
using System;
using System.Text;

namespace TouchSocket.Core
{
    /// <summary>
    /// 字节类转换器
    /// </summary>
    public class BytesConverter : TouchSocketConverter<byte[]>
    {
        /// <summary>
        /// 字节类转换器
        /// </summary>
        public BytesConverter()
        {
            Add(new JsonBytesToClassConverter());
        }
    }

    /// <summary>
    /// Json字节转到对应类
    /// </summary>
    public class JsonBytesToClassConverter : IConverter<byte[]>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool TryConvertFrom(byte[] source, Type targetType, out object target)
        {
            try
            {
                target = SerializeConvert.JsonDeserializeFromBytes(source, targetType);
                return true;
            }
            catch
            {
                target = default;
                return false;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool TryConvertTo(object target, out byte[] source)
        {
            try
            {
                source =SerializeConvert.JsonSerializeToBytes(target);
                return true;
            }
            catch (Exception)
            {
                source = null;
                return false;
            }
        }
    }
}