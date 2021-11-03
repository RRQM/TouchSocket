using RRQMCore.ByteManager;
using RRQMCore.Serialization;

namespace RRQMCore.Helper
{
    /// <summary>
    /// 序列化辅助
    /// </summary>
    public static class SerializationHelper
    {
        /// <summary>
        /// 序列化成数据
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="serializationType"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this object obj, SerializationType serializationType = SerializationType.RRQMBinary)
        {
            ByteBlock byteBlock = new ByteBlock(1024 * 10);
            byteBlock.WriteObject(obj, serializationType);
            return byteBlock.ToArray();
        }
    }
}