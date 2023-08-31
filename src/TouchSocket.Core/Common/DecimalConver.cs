namespace TouchSocket.Core
{
    /// <summary>
    /// <see cref="decimal"/>与字节数组转换
    /// </summary>
    public static class DecimalConver
    {
        /// <summary>
        /// 将<see cref="decimal"/>对象转换为固定字节长度（16）数组。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytes(decimal value)
        {
            var bits = decimal.GetBits(value);
            var bytes = new byte[bits.Length * 4];
            for (var i = 0; i < bits.Length; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    bytes[i * 4 + j] = (byte)(bits[i] >> (j * 8));
                }
            }
            return bytes;
        }
        /// <summary>
        /// 将固定字节长度（16）数组转换为<see cref="decimal"/>对象。
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static decimal FromBytes(byte[] array)
        {
            var bits = new int[array.Length / 4];
            for (var i = 0; i < bits.Length; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    bits[i] |= array[i * 4 + j] << j * 8;
                }
            }
            return new decimal(bits);
        }
    }
}
