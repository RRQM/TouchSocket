using System.Threading;

namespace TouchSocket.Core
{
    /// <summary>
    /// 提供依赖属性(DependencyProperty)的基础实现。
    /// </summary>
    public abstract class DependencyPropertyBase
    {
        /// <summary>
        /// 用于生成依赖属性标识的唯一ID。
        /// </summary>
        private static int s_idCount = 0;

        /// <summary>
        /// 初始化依赖属性对象，为每个属性分配唯一的ID。
        /// </summary>
        public DependencyPropertyBase()
        {
            // 原子性增加s_idCount并赋值给当前实例的Id，保证Id的唯一性。
            this.Id = Interlocked.Increment(ref s_idCount);
        }

        /// <summary>
        /// 标识属性的唯一ID。
        /// </summary>
        public int Id { get; }
    }
}