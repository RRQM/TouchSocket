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
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Reflection;

namespace TouchSocket;

[Generator]
public class ContainerSourceGenerator : ISourceGenerator
{
    private readonly string m_generatorContainerAttribute = @"
/*
此代码由SourceGenerator工具直接生成，非必要请不要修改此处代码
*/

#pragma warning disable

using System;
using System.Collections.Generic;
using System.Text;

namespace TouchSocket.Core
{
    /// <summary>
    /// 源生成容器特性
    /// </summary>
    /*GeneratedCode*/
    internal class GeneratorContainerAttribute : Attribute
    {
    }

    /*GeneratedCode*/
    internal class BaseInjectAttribute : Attribute
    {
        /// <summary>
        /// 注册类型
        /// </summary>
        public Type FromType { get; set; }

        /// <summary>
        /// 实例类型
        /// </summary>
        public Type ToType { get; set; }

        /// <summary>
        /// 注册额外键
        /// </summary>
        public string Key { get; set; }
    }

    /// <summary>
    /// 自动注入为单例。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    /*GeneratedCode*/
    internal class AutoInjectForSingletonAttribute : BaseInjectAttribute
    {
    }

    /// <summary>
    /// 自动注入为瞬时。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    /*GeneratedCode*/
    internal class AutoInjectForTransientAttribute : BaseInjectAttribute
    {
    }

    /// <summary>
    /// 添加单例注入。
    /// <para>
    /// 该标签仅添加在继承<see cref=""ManualContainer""/>的容器上时有用。
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    /*GeneratedCode*/
    internal class AddSingletonInjectAttribute : BaseInjectAttribute
    {
        /// <summary>
        /// 添加单例注入。
        /// <para>
        /// 该标签仅添加在继承<see cref=""ManualContainer""/>的容器上时有用。
        /// </para>
        /// </summary>
        /// <param name=""fromType"">注册类型</param>
        /// <param name=""toType"">实例类型</param>
        /// <param name=""key"">注册额外键</param>
        public AddSingletonInjectAttribute(Type fromType, Type toType, string key)
        {
            this.FromType = fromType;
            this.ToType = toType;
            this.Key = key;
        }

        /// <summary>
        /// 添加单例注入。
        /// <para>
        /// 该标签仅添加在继承<see cref=""ManualContainer""/>的容器上时有用。
        /// </para>
        /// </summary>
        /// <param name=""type"">注册类型与实例类型一致</param>
        public AddSingletonInjectAttribute(Type type) : this(type, type, null)
        {
        }

        /// <summary>
        /// 添加单例注入。
        /// <para>
        /// 该标签仅添加在继承<see cref=""ManualContainer""/>的容器上时有用。
        /// </para>
        /// </summary>
        /// <param name=""fromType"">注册类型</param>
        /// <param name=""toType"">实例类型</param>
        public AddSingletonInjectAttribute(Type fromType, Type toType) : this(fromType, toType, null)
        {
        }
    }

    /// <summary>
    /// 添加瞬态注入。
    /// <para>
    /// 该标签仅添加在继承<see cref=""ManualContainer""/>的容器上时有用。
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    /*GeneratedCode*/
    internal class AddTransientInjectAttribute : BaseInjectAttribute
    {
        /// <summary>
        /// 添加瞬态注入。
        /// <para>
        /// 该标签仅添加在继承<see cref=""ManualContainer""/>的容器上时有用。
        /// </para>
        /// </summary>
        /// <param name=""fromType"">注册类型</param>
        /// <param name=""toType"">实例类型</param>
        /// <param name=""key"">注册额外键</param>
        public AddTransientInjectAttribute(Type fromType, Type toType, string key)
        {
            this.FromType = fromType;
            this.ToType = toType;
            this.Key = key;
        }

        /// <summary>
        /// 添加瞬态注入。
        /// <para>
        /// 该标签仅添加在继承<see cref=""ManualContainer""/>的容器上时有用。
        /// </para>
        /// </summary>
        /// <param name=""type"">注册类型与实例类型一致</param>
        public AddTransientInjectAttribute(Type type) : this(type, type, null)
        {
        }

        /// <summary>
        /// 添加瞬态注入。
        /// <para>
        /// 该标签仅添加在继承<see cref=""ManualContainer""/>的容器上时有用。
        /// </para>
        /// </summary>
        /// <param name=""fromType"">注册类型</param>
        /// <param name=""toType"">实例类型</param>
        public AddTransientInjectAttribute(Type fromType, Type toType) : this(fromType, toType, null)
        {
        }
    }
}
";

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(a =>
        {
            var sourceCode = this.m_generatorContainerAttribute.Replace("/*GeneratedCode*/", $"[global::System.CodeDom.Compiler.GeneratedCode(\"TouchSocket.SourceGenerator\",\"{Assembly.GetExecutingAssembly().GetName().Version.ToString()}\")]");

            a.AddSource(nameof(this.m_generatorContainerAttribute), sourceCode);
        });
        context.RegisterForSyntaxNotifications(() => new ContainerSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var s = context.Compilation.GetMetadataReference(context.Compilation.Assembly);

        if (context.SyntaxReceiver is ContainerSyntaxReceiver receiver)
        {
            var types1 = receiver.GetAutoInjectForSingletonClassTypes(context.Compilation);
            var types2 = receiver.GetAutoInjectForTransientClassTypes(context.Compilation);

            var builders = receiver
                .GetContainerClassTypes(context.Compilation)
                .Select(i => new ContainerCodeBuilder(i, types1, types2))
                .Distinct(CodeBuilderEqualityComparer<ContainerCodeBuilder>.Default);
            //Debugger.Launch();

            foreach (var builder in builders)
            {
                if (builder.TryToSourceText(out var sourceText))
                {
                    var tree = CSharpSyntaxTree.ParseText(sourceText);
                    var root = tree.GetRoot().NormalizeWhitespace();
                    var ret = root.ToFullString();
                    context.AddSource($"{builder.GetFileName()}.g.cs", ret);
                }
            }
        }
    }
}