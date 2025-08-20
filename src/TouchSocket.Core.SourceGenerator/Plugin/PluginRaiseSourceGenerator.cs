// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace TouchSocket;

[Generator]
internal class PluginRaiseSourceGenerator : IIncrementalGenerator
{
    private const string IPluginString = "TouchSocket.Core.IPlugin";

    private const string PluginRaiseAttributeNameString = "TouchSocket.Core.PluginRaiseAttribute";

    private readonly string PluginRaiseAttributeString = @"
using System;

namespace TouchSocket.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    /*GeneratedCode*/
    internal sealed class PluginRaiseAttribute : Attribute
    {
        public Type PluginType { get; }

        public PluginRaiseAttribute(Type pluginType)
        {
            PluginType = pluginType;
        }
    }
}
";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 注册预生成的属性代码
        context.RegisterPostInitializationOutput(ctx =>
        {
            var sourceCode = CodeBuilder.ReplaceGeneratedCode(this.PluginRaiseAttributeString);

            ctx.AddSource("PluginRaiseAttribute.g.cs", sourceCode);
        });

        // 1. 筛选所有包含[PluginRaise]特性的类
        var pluginClassDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax cds
                    && cds.AttributeLists.Count > 0,
                transform: static (ctx, _) => GetClassWithAttribute(ctx))
            .Where(static c => c is not null).Collect();

        // 3. 生成最终代码
        context.RegisterSourceOutput(pluginClassDeclarations,
            (spc, source) => this.Execute(source, spc));
    }

    private static INamedTypeSymbol GetClassWithAttribute(GeneratorSyntaxContext context)
    {
        //Debugger.Launch();
        var model = context.SemanticModel;
        var classSymbol = model.GetDeclaredSymbol(context.Node) as INamedTypeSymbol;

        // 检查是否包含GeneratorPackageAttribute
        var hasAttribute = classSymbol.HasAttributes(PluginRaiseAttributeNameString, out var attributes);

        return hasAttribute == true ? classSymbol : null;
    }

    private void Execute(ImmutableArray<INamedTypeSymbol> classes, SourceProductionContext context)
    {
        var pluginTypes = new List<string>();
        foreach (var classSymbol in classes)
        {
            if (classSymbol.HasAttributes(PluginRaiseAttributeNameString, out var attributes))
            {
                foreach (var attribute in attributes)
                {
                    var pluginTypeArg = (INamedTypeSymbol)attribute.ConstructorArguments[0].Value!;

                    if (!pluginTypeArg.IsInheritFrom(IPluginString))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptors.m_rule_Plugin0005,
                            attribute.ApplicationSyntaxReference.GetSyntax().GetLocation(),
                            pluginTypeArg.Name));
                        return;
                    }
                }

                var codeBuilder = new MyCodeBuilder(classSymbol, attributes.Select(a => (INamedTypeSymbol)a.ConstructorArguments.First().Value));

                context.AddSource(codeBuilder);
            }
        }
    }

    #region Class

    private class MyCodeBuilder : TypeCodeBuilder<INamedTypeSymbol>
    {
        private readonly IEnumerable<INamedTypeSymbol> m_pluginInterfaces;

        public MyCodeBuilder(INamedTypeSymbol typeSymbol, IEnumerable<INamedTypeSymbol> pluginInterfaces) : base(typeSymbol)
        {
            this.m_pluginInterfaces = pluginInterfaces;
        }

        protected override bool GeneratorCode(StringBuilder codeBuilder)
        {
            using (this.CreateNamespace(codeBuilder))
            {
                codeBuilder.AppendLine($"partial class {this.TypeSymbol.Name}");
                using (this.CreateCodeSpace(codeBuilder))
                {
                    foreach (var item in this.m_pluginInterfaces)
                    {
                        this.AppendMethod(codeBuilder, item);
                    }
                }
            }
            return true;
        }

        private void AppendMethod(StringBuilder codeBuilder, INamedTypeSymbol namedTypeSymbol)
        {
            var method = namedTypeSymbol.GetMembers().OfType<IMethodSymbol>().FirstOrDefault();
            if (method is null)
            {
                return;
            }

            var firstParameter = method.Parameters[0];
            var secondParameter = method.Parameters[1];

            codeBuilder.AppendLine($"public static ValueTask<bool> Raise{namedTypeSymbol.Name}Async(this IPluginManager pluginManager, IResolver resolver, {firstParameter.Type.ToDisplayString()} sender, {secondParameter.Type.ToDisplayString()} e)");

            using (this.CreateCodeSpace(codeBuilder))
            {
                codeBuilder.AppendLine($"return pluginManager.RaiseAsync(typeof({namedTypeSymbol.ToDisplayString()}), resolver, sender, e);");
            }
        }
    }

    #endregion Class
}