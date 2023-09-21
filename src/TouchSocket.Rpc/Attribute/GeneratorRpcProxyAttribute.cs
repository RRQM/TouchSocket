using System;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 标识该接口将自动生成调用的扩展方法静态类
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class GeneratorRpcProxyAttribute : Attribute
    {
        /// <summary>
        /// 调用键的前缀，包括服务的命名空间，类名，不区分大小写。格式：命名空间.类名
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// 生成泛型方法的约束
        /// </summary>
        public Type[] GenericConstraintTypes { get; set; }

        /// <summary>
        /// 是否仅以函数名调用，当为True是，调用时仅需要传入方法名即可。
        /// </summary>
        public bool MethodInvoke { get; set; }

        /// <summary>
        /// 生成代码的命名空间
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// 生成的类名，不要包含“I”，生成接口时会自动家。
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// 生成代码
        /// </summary>
        public CodeGeneratorFlag GeneratorFlag { get; set; }

        /// <summary>
        /// 函数标识
        /// </summary>
        public MethodFlags MethodFlags { get; set; }

        /// <summary>
        /// 继承接口
        /// </summary>
        public bool InheritedInterface { get; set; }
    }
}