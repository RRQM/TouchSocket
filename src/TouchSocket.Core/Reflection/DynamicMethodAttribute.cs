using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 定义一个动态方法的特性，可以指导源生代码生成器如何生成动态方法。便于在运行时动态调用。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface)]
    public sealed class DynamicMethodAttribute : Attribute

    {
    }
}
