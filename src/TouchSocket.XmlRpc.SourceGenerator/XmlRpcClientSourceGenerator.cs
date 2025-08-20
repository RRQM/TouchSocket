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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using TouchSocket.Rpc;

namespace TouchSocket;
[Generator]
public class XmlRpcClientSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 第一步：收集所有接口声明语法节点
        var interfaceDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is InterfaceDeclarationSyntax,
                transform: static (ctx, _) => (InterfaceDeclarationSyntax)ctx.Node);

        // 第二步：将语法节点转换为符号并过滤有效接口
        var interfacesWithSymbol = context.CompilationProvider.Combine(interfaceDeclarations.Collect())
            .SelectMany(static (tuple, _) =>
            {
                var (compilation, interfaces) = tuple;
                var results = new List<INamedTypeSymbol>();
                var attributeSymbol = RpcUtils.GetGeneratorRpcProxyAttribute(compilation);

                foreach (var interfaceSyntax in interfaces)
                {
                    var model = compilation.GetSemanticModel(interfaceSyntax.SyntaxTree);
                    var interfaceSymbol = model.GetDeclaredSymbol(interfaceSyntax);

                    if (interfaceSymbol != null &&
                        attributeSymbol != null &&
                        RpcUtils.IsRpcApiInterface(interfaceSymbol))
                    {
                        results.Add(interfaceSymbol);
                    }
                }
                return results.Distinct(SymbolEqualityComparer.Default);
            });

        // 第三步：生成源代码
        context.RegisterSourceOutput(interfacesWithSymbol,
            static (productionContext, interfaceSymbol) =>
            {
                var builder = new XmlRpcClientCodeBuilder((INamedTypeSymbol)interfaceSymbol);
                productionContext.AddSource(builder);
            });
    }
}

//[Generator]
//public class XmlRpcClientSourceGenerator : ISourceGenerator
//{
//    public void Initialize(GeneratorInitializationContext context)
//    {
//        context.RegisterForSyntaxNotifications(() => new XmlRpcClientSyntaxReceiver());
//    }

//    public void Execute(GeneratorExecutionContext context)
//    {
//        var s = context.Compilation.GetMetadataReference(context.Compilation.Assembly);

//        if (context.SyntaxReceiver is XmlRpcClientSyntaxReceiver receiver)
//        {
//            var builders = receiver
//                .GetRpcApiTypes(context.Compilation)
//                .Select(i => new XmlRpcClientCodeBuilder(i))
//                .Distinct(CodeBuilderEqualityComparer<XmlRpcClientCodeBuilder>.Default);
//            //Debugger.Launch();
//            foreach (var builder in builders)
//            {
//                var tree = CSharpSyntaxTree.ParseText(builder.ToSourceText());
//                var root = tree.GetRoot().NormalizeWhitespace();
//                var ret = root.ToFullString();
//                context.AddSource($"{builder.GetFileName()}.g.cs", ret);
//            }
//        }
//    }
//}