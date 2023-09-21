using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// 动态构建类型
    /// </summary>
    public enum DynamicBuilderType
    {
        /// <summary>
        /// 使用IL构建
        /// </summary>
        IL,

        /// <summary>
        /// 使用表达式树
        /// </summary>
        Expression,

        /// <summary>
        /// 使用反射
        /// </summary>
        Reflect
    }
}