//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using System.Reflection;

namespace TouchSocket;

[Generator]
public class RpcClientSourceGenerator : IIncrementalGenerator
{
    private readonly string m_generatorRpcProxyAttribute = @"

/*
此代码由SourceGenerator工具直接生成，非必要请不要修改此处代码
*/

#pragma warning disable

using System;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 标识该接口将使用源生成自动生成调用的代理类
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    /*GeneratedCode*/
    internal sealed class GeneratorRpcProxyAttribute : Attribute
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
        /// 生成的类名，不要包含“I”，生成接口时会自动添加。
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// 生成代码
        /// </summary>
        public GeneratorFlag GeneratorFlag { get; set; }

        /// <summary>
        /// 继承接口
        /// </summary>
        public bool InheritedInterface { get; set; }
    }

    /// <summary>
    /// 代码生成标识
    /// </summary>
    [Flags]
    /*GeneratedCode*/
    internal enum GeneratorFlag
    {
        /// <summary>
        /// 生成扩展同步代码
        /// </summary>
        ExtensionSync = 1,

        /// <summary>
        /// 生成扩展异步代码
        /// </summary>
        ExtensionAsync = 2,

        /// <summary>
        /// 生成实例类同步代码（源代码生成无效）
        /// </summary>
        InstanceSync = 4,

        /// <summary>
        /// 生成实例类异步代码（源代码生成无效）
        /// </summary>
        InstanceAsync = 8,

        /// <summary>
        /// 生成接口同步代码
        /// </summary>
        InterfaceSync = 16,

        /// <summary>
        /// 生成接口异步代码
        /// </summary>
        InterfaceAsync = 32
    }
}
";


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(a =>
        {
            var sourceCode = this.m_generatorRpcProxyAttribute.Replace("/*GeneratedCode*/", $"[global::System.CodeDom.Compiler.GeneratedCode(\"TouchSocket.SourceGenerator\",\"{Assembly.GetExecutingAssembly().GetName().Version.ToString()}\")]");
            a.AddSource(nameof(this.m_generatorRpcProxyAttribute) + ".g.cs", sourceCode);
        });
    }
}