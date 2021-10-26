using System.Collections.Concurrent;

namespace RRQMCore.Helper
{
    /// <summary>
    /// 集合助手
    /// </summary>
    public static class CollectionsHelper
    {
        /// <summary>
        /// 清除所有成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue"></param>
        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {
#if NETCOREAPP3_1_OR_GREATER
            queue.Clear();
#else
            while (queue.TryDequeue(out _))
            {
            }
#endif
        }
    }
}