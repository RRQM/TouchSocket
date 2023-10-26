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