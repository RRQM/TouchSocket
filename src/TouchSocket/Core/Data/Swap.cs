namespace TouchSocket.Core
{
    /// <summary>
    /// 交换类。功能类似：a=1，b=2，交换后a=2，b=1。
    /// </summary>
    public static class Swap
    {
        /// <summary>
        /// 执行交换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void Execute<T>(ref T x, ref T y)
        {
#if NET45_OR_GREATER
            T temp = x;
            x = y;
            y = temp;
#else
            (y, x) = (x, y);
#endif
        }
    }
}