namespace TouchSocket.Resources
{
    /// <summary>
    /// StatusExtension
    /// </summary>
    internal static class TouchSocketStatusExtension
    {
        /// <summary>
        /// 转为状态字
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketStatus ToStatus(this byte value)
        {
            return (TouchSocketStatus)value;
        }

        /// <summary>
        /// 转为数值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte ToValue(this TouchSocketStatus value)
        {
            return (byte)value;
        }
    }
}
