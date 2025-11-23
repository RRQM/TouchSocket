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
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using TouchSocket.SourceGenerator;

namespace TouchSocket;

[Generator]
public class PluginAddSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 筛选符合要求的接口语法节点
        var interfaceDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsInterfaceSyntax(s),
                transform: static (ctx, _) => GetInterfaceSymbol(ctx))
            .Where(static s => s is not null)
            .Collect();

        // 生成源代码
        context.RegisterSourceOutput(interfaceDeclarations, (spc, source) =>
            this.GenerateSource(spc, source));
    }

    private static INamedTypeSymbol GetInterfaceSymbol(GeneratorSyntaxContext context)
    {
        var interfaceSyntax = (InterfaceDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(interfaceSyntax);

        // 验证接口是否符合插件要求
        if (symbol is null || !CoreAnalyzer.IsPluginInterface(symbol))
            return null;

        return symbol.DeclaredAccessibility == Accessibility.Public ? symbol : null;
    }

    private static bool IsInterfaceSyntax(SyntaxNode node)
    {
        // 筛选接口声明且具有至少一个方法
        return node is InterfaceDeclarationSyntax { Members.Count: > 0 };
    }

    private void GenerateSource(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> interfaces)
    {
        foreach (var interfaceSymbol in interfaces.Distinct(SymbolEqualityComparer.Default).Cast<INamedTypeSymbol>())
        {
            var source = new PluginAddCodeBuilder(interfaceSymbol);
            if (source != null)
            {
                context.AddSource(source);
            }
        }
    }

    #region Class

    internal sealed class PluginAddCodeBuilder : CodeBuilder
    {
        private const string IPluginManagerString = "TouchSocket.Core.IPluginManager";
        private const string PluginBaseString = "TouchSocket.Core.PluginBase";
        private const string PluginEventArgsString = "TouchSocket.Core.PluginEventArgs";
        private readonly INamedTypeSymbol m_pluginClass;

        public PluginAddCodeBuilder(INamedTypeSymbol pluginClass)
        {
            this.m_pluginClass = pluginClass;
        }

        public override string Id => this.m_pluginClass.ToDisplayString();
        public string Prefix { get; set; }

        public override string GetFileName()
        {
            return this.m_pluginClass.ToDisplayString() + "ExtensionsGenerator.g.cs";
        }

        public bool TryToSourceText(out SourceText sourceText)
        {
            var code = this.ToString();
            if (string.IsNullOrEmpty(code))
            {
                sourceText = null;
                return false;
            }
            sourceText = SourceText.From(code, Encoding.UTF8);
            return true;
        }

        protected override bool GeneratorCode(StringBuilder codeBuilder)
        {
            var method = this.FindMethod();
            if (method is null)
            {
                return false;
            }
            if (method.Parameters.Length != 2)
            {
                return false;
            }

            var pluginClassName = this.GetPluginClassName();
            var pluginMethodName = method.Name;
            var firstType = method.Parameters[0].Type;
            var secondType = method.Parameters[1].Type;
            // var xml = ExtractSummary((method.GetDocumentationCommentXml() ?? this.m_pluginClass.GetDocumentationCommentXml()) ?? string.Empty);

            if (!this.m_pluginClass.ContainingNamespace.IsGlobalNamespace)
            {
                codeBuilder.AppendLine($"namespace {this.m_pluginClass.ContainingNamespace}");
                codeBuilder.AppendLine("{");
            }

            codeBuilder.AppendLine($"/// <inheritdoc cref=\"{this.m_pluginClass.ToDisplayString()}\"/>");
            codeBuilder.AppendLine($"public static class _{pluginClassName}Extensions");
            codeBuilder.AppendLine("{");
            //1
            codeBuilder.AppendLine($"/// <inheritdoc cref=\"{this.m_pluginClass.ToDisplayString()}.{method.Name}({firstType.ToDisplayString()},{secondType.ToDisplayString()})\"/>");
            codeBuilder.AppendLine($"public static void Add{pluginClassName}(this IPluginManager pluginManager, Func<{firstType.ToDisplayString()}, {secondType.ToDisplayString()}, Task> func)");

            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine($"pluginManager.Add(typeof({this.m_pluginClass.ToDisplayString()}), func);");
            codeBuilder.AppendLine("}");

            //2
            codeBuilder.AppendLine($"/// <inheritdoc cref=\"{this.m_pluginClass.ToDisplayString()}.{method.Name}({firstType.ToDisplayString()},{secondType.ToDisplayString()})\"/>");
            codeBuilder.AppendLine($"public static void Add{pluginClassName}(this IPluginManager pluginManager, Action<{firstType.ToDisplayString()}> action)");

            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine("Task newFunc(object sender, PluginEventArgs e)");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine($"action(({firstType.ToDisplayString()})sender);");
            codeBuilder.AppendLine("return e.InvokeNext();");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine($"pluginManager.Add(typeof({this.m_pluginClass.ToDisplayString()}), newFunc, action);");
            codeBuilder.AppendLine("}");

            //3
            codeBuilder.AppendLine($"/// <inheritdoc cref=\"{this.m_pluginClass.ToDisplayString()}.{method.Name}({firstType.ToDisplayString()},{secondType.ToDisplayString()})\"/>");
            codeBuilder.AppendLine($"public static void Add{pluginClassName}(this IPluginManager pluginManager, Action action)");

            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine($"pluginManager.Add(typeof({this.m_pluginClass.ToDisplayString()}), action);");
            codeBuilder.AppendLine("}");

            //4
            codeBuilder.AppendLine($"/// <inheritdoc cref=\"{this.m_pluginClass.ToDisplayString()}.{method.Name}({firstType.ToDisplayString()},{secondType.ToDisplayString()})\"/>");
            codeBuilder.AppendLine($"public static void Add{pluginClassName}(this IPluginManager pluginManager, Func<{secondType.ToDisplayString()},Task> func)");

            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine($"pluginManager.Add(typeof({this.m_pluginClass.ToDisplayString()}), func);");
            codeBuilder.AppendLine("}");

            //5
            codeBuilder.AppendLine($"/// <inheritdoc cref=\"{this.m_pluginClass.ToDisplayString()}.{method.Name}({firstType.ToDisplayString()},{secondType.ToDisplayString()})\"/>");
            codeBuilder.AppendLine($"public static void Add{pluginClassName}(this IPluginManager pluginManager, Func<{firstType.ToDisplayString()},Task> func)");

            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine("async Task newFunc(object sender, PluginEventArgs e)");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine($"await func(({firstType.ToDisplayString()})sender).ConfigureAwait(EasyTask.ContinueOnCapturedContext);");
            codeBuilder.AppendLine("await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine($"pluginManager.Add(typeof({this.m_pluginClass.ToDisplayString()}), newFunc, func);");
            codeBuilder.AppendLine("}");

            //6
            codeBuilder.AppendLine($"/// <inheritdoc cref=\"{this.m_pluginClass.ToDisplayString()}.{method.Name}({firstType.ToDisplayString()},{secondType.ToDisplayString()})\"/>");
            codeBuilder.AppendLine($"public static void Add{pluginClassName}(this IPluginManager pluginManager, Func<Task> func)");

            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine($"pluginManager.Add(typeof({this.m_pluginClass.ToDisplayString()}), func);");
            codeBuilder.AppendLine("}");

            //class end
            codeBuilder.AppendLine("}");
            if (!this.m_pluginClass.ContainingNamespace.IsGlobalNamespace)
            {
                codeBuilder.AppendLine("}");
            }
            return true;
        }

        private string ExtractSummary(string xmlDoc)
        {
            if (string.IsNullOrEmpty(xmlDoc))
            {
                return string.Empty;
            }
            try
            {
                var doc = XDocument.Parse(xmlDoc);
                var summaryElement = doc.Descendants("summary").FirstOrDefault();
                var summary = summaryElement?.Value.Trim();

                if (string.IsNullOrEmpty(summary))
                {
                    return string.Empty;
                }
                //去掉换行符
                return summary.Replace("\n", "").Replace("\r", "");
            }
            catch
            {
                return null;
            }
        }

        private IMethodSymbol FindMethod()
        {
            return this.m_pluginClass.GetMembers().OfType<IMethodSymbol>().FirstOrDefault();
        }

        private string GetPluginClassName()
        {
            var name = this.m_pluginClass.Name;
            if (name.StartsWith("I"))
            {
                return name.Substring(1);
            }
            return this.m_pluginClass.Name;
        }
    }

    #endregion Class
}