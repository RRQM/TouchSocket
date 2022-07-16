using System;

namespace TouchSocket.Core.Dependency
{
    /// <summary>
    /// 依赖属性委托
    /// </summary>
    public sealed class DependencyAction
    {
        /// <summary>
        /// 依赖属性委托。
        /// </summary>
        /// <param name="func">当在没有注入值的情况下，获取值时，实际上是获取默认值，也就会调用此委托。</param>
        /// <param name="alwaysNew">当为True时，不管有没有注入值，总是会从此委托获取新值。</param>
        public DependencyAction(Func<object> func, bool alwaysNew = false)
        {
            this.Func = func ?? throw new ArgumentNullException(nameof(func));
            this.AlwaysNew = alwaysNew;
        }

        /// <summary>
        /// 委托
        /// </summary>
        public Func<object> Func { get; }

        /// <summary>
        /// 总是新值
        /// </summary>
        public bool AlwaysNew { get; }
    }
}