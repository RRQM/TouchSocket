using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// 依赖注入类型。
    /// </summary>
    [Flags]
    public enum DependencyType
    {
        /// <summary>
        /// 构造函数
        /// </summary>

        Constructor = 0,

        /// <summary>
        /// 属性
        /// </summary>
        Property = 1,

        /// <summary>
        /// 方法
        /// </summary>
        Method = 2
    }
}