using TouchSocket.Core.ByteManager;

namespace TouchSocket.Core.Serialization
{
    /// <summary>
    /// FastBinary转换器
    /// </summary>
    public interface IFastBinaryConverter
    {
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public object Read(byte[] buffer, int offset, int len);

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        public int Write(ByteBlock byteBlock, object obj);
    }

    /// <summary>
    /// FastBinary转换器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class FastBinaryConverter<T> : IFastBinaryConverter
    {
        int IFastBinaryConverter.Write(ByteBlock byteBlock, object obj)
        {
            return this.Write(byteBlock, (T)obj);
        }

        object IFastBinaryConverter.Read(byte[] buffer, int offset, int len)
        {
            return this.Read(buffer, offset, len);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected abstract int Write(ByteBlock byteBlock, T obj);

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        protected abstract T Read(byte[] buffer, int offset, int len);
    }
}