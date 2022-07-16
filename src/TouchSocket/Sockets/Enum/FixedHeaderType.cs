namespace TouchSocket.Sockets
{
    /// <summary>
    /// 固定包头类型
    /// </summary>
    public enum FixedHeaderType : byte
    {
        /// <summary>
        /// 以1Byte标识长度，最长接收255
        /// </summary>
        Byte = 1,

        /// <summary>
        /// 以2Byte标识长度，最长接收65535
        /// </summary>
        Ushort = 2,

        /// <summary>
        /// 以4Byte标识长度，最长接收2147483647
        /// </summary>
        Int = 4
    }
}
