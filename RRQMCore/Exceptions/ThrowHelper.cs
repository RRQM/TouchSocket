namespace RRQMCore.Exceptions
{
    /// <summary>
    /// 异常助手
    /// </summary>
    public class ThrowHelper
    {
        static ThrowHelper()
        {
            throwHelper = new ThrowHelper();
        }

        private static ThrowHelper throwHelper;

        /// <summary>
        /// 默认实例
        /// </summary>
        public static ThrowHelper Default => throwHelper;

    }
}
