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
using System.Linq;

namespace TouchSocket;

[Generator]
public class FastSerializeGenerator : ISourceGenerator
{
    public const string FastSerializableAttributeString = "TouchSocket.Core.FastSerializableAttribute";
    public void Execute(GeneratorExecutionContext context)
    {
        var compilation = context.Compilation;
        var s = compilation.GetMetadataReference(context.Compilation.Assembly);

        var fastSerializableAttribute = compilation.GetTypeByMetadataName(FastSerializableAttributeString);

        if (context.SyntaxReceiver is FastSerializeSyntaxReceiver receiver)
        {
            var pairs = receiver.GetFastSerializeContexts(context);

            var builders = pairs.Select(a => new FastSerializeCodeBuilder(a.Key, a.Value));
            foreach (var builder in builders)
            {
                context.AddSource($"{builder.GetFileName()}.g.cs", builder.ToSourceText());
            }

            //foreach (var builder in pairs.Keys.Select(a => new MethodInvokeTitleCodeBuilder(a)))
            //{
            //    context.AddSource($"{builder.GetFileName()}.g.cs", builder.ToSourceText());
            //}
        }
    }

    private readonly string fastSerializableAttribute = @"

/*
此代码由SourceGenerator工具直接生成，非必要请不要修改此处代码
*/

#pragma warning disable

using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// 标识源生成Fast序列化相关的实现。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class,AllowMultiple =true)]
    /*GeneratedCode*/
    internal class FastSerializableAttribute : Attribute
    {
        public FastSerializableAttribute(Type type, TypeMode typeMode)
        {
            this.Type = type;
            this.TypeMode = typeMode;
        }

        public Type Type { get; }

        public FastSerializableAttribute(Type type) : this(type, TypeMode.Self)
        {

        }

        public TypeMode TypeMode { get; }
    }

    [Flags]
    /*GeneratedCode*/
    internal enum TypeMode
    {
        All = -1,
        Self = 0,
        Field = 1,
        Property = 2,
        MethodReturn = 4,
        MethodParameter = 8
    }
}


";

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(a =>
        {
            a.AddSource($"{nameof(this.fastSerializableAttribute)}.g.cs", this.fastSerializableAttribute);
        });
        context.RegisterForSyntaxNotifications(() => new FastSerializeSyntaxReceiver());
    }
}