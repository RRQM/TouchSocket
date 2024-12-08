using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 标识一个方法或类是否可重新进入。
    /// </summary>
    /// <remarks>
    /// 该属性主要用于并发控制和同步机制中，指示被标记的方法或类是否可以在尚未完成执行时再次被调用。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]

    public sealed class ReenterableAttribute : Attribute
    {
        /// <summary>
        /// 初始化 ReenterableAttribute 类的新实例。
        /// </summary>
        /// <param name="reenterable">指示是否可重新进入。true 表示可重新进入；false 表示不可重新进入。</param>
        public ReenterableAttribute(bool reenterable)
        {
            this.Reenterable = reenterable;
        }

        /// <summary>
        /// 获取一个值，指示是否可重新进入。
        /// </summary>
        public bool Reenterable { get; }
    }
}
