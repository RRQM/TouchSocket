//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace TouchSocket
{
    [Generator]
    public class RpcServerSourceGenerator : ISourceGenerator
    {
        private string m_generatorAttribute = @"
using System;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 标识将通过源生成器生成Rpc服务的调用委托。
    /// </summary>
    [AttributeUsage( AttributeTargets.Class)]
    internal class GeneratorRpcServerAttribute:Attribute
    {
    }
}
";

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(a =>
            {
                a.AddSource(nameof(this.m_generatorAttribute), this.m_generatorAttribute);
            });
            context.RegisterForSyntaxNotifications(() => new RpcServerSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var s = context.Compilation.GetMetadataReference(context.Compilation.Assembly);

            if (context.SyntaxReceiver is RpcServerSyntaxReceiver receiver)
            {
                var builders = receiver
                    .GetRpcServerTypes(context.Compilation)
                    .Select(i => new RpcServerCodeBuilder(i))
                    .Distinct();
                //Debugger.Launch();
                foreach (var builder in builders)
                {
                    var tree = CSharpSyntaxTree.ParseText(builder.ToSourceText());
                    var root = tree.GetRoot().NormalizeWhitespace();
                    var ret = root.ToFullString();
                    context.AddSource($"{builder.GetFileName()}.g.cs", ret);
                }
            }
        }
    }
}