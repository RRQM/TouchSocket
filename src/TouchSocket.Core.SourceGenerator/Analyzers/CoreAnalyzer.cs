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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace TouchSocket.SourceGenerator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class CoreAnalyzer : DiagnosticAnalyzer
{
    public const string IPlugin = "TouchSocket.Core.IPlugin";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(
                DiagnosticDescriptors.m_rule_Package0001,
                DiagnosticDescriptors.m_rule_Package0002,
                DiagnosticDescriptors.m_rule_Plugin0001,
                DiagnosticDescriptors.m_rule_Plugin0002,
                DiagnosticDescriptors.m_rule_Plugin0003,
                DiagnosticDescriptors.m_rule_Plugin0004,
                DiagnosticDescriptors.m_rule_Plugin0005,
                DiagnosticDescriptors.m_rule_FastSerialize0001,
                DiagnosticDescriptors.m_rule_CodeAnalysis0001,
                DiagnosticDescriptors.m_rule_CodeAnalysis0002
                );
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        //context.RegisterOperationAction(this.AnalyzeSymbol, OperationKind.PropertyReference);
        context.RegisterSymbolAction(this.AnalyzePluginSymbol, SymbolKind.NamedType);
        context.RegisterOperationAction(this.AnalyzeAsyncToSyncMethodSymbol, OperationKind.Invocation);

        context.RegisterSyntaxNodeAction(this.AnalyzeMember, SyntaxKind.FieldDeclaration);
        context.RegisterSyntaxNodeAction(this.AnalyzeMember, SyntaxKind.PropertyDeclaration);
    }

    #region AnalyzeMember
    private void AnalyzeMember(SyntaxNodeAnalysisContext context)
    {
        switch (context.Node)
        {
            case FieldDeclarationSyntax field:
                this.AnalyzeField(field, context);
                break;
            case PropertyDeclarationSyntax property:
                this.AnalyzeProperty(property, context);
                break;
        }
    }

    private void AnalyzeField(FieldDeclarationSyntax field, SyntaxNodeAnalysisContext context)
    {
        foreach (var variable in field.Declaration.Variables)
        {
            var fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
            if (!this.IsDependencyProperty(fieldSymbol?.Type))
            {
                return;
            }

            // 检查static修饰符
            if (field.Modifiers.Any(SyntaxKind.StaticKeyword) && field.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))
            {
                continue;
            }

            this.ReportDiagnostic(field.Declaration.Type.GetLocation(), fieldSymbol.Name, context);
        }
    }

    private void AnalyzeProperty(PropertyDeclarationSyntax property, SyntaxNodeAnalysisContext context)
    {
        var propertySymbol = context.SemanticModel.GetDeclaredSymbol(property);
        if (!this.IsDependencyProperty(propertySymbol?.Type))
        {
            return;
        }

        // 检查static修饰符
        if (property.Modifiers.Any(SyntaxKind.StaticKeyword) && propertySymbol.IsReadOnly)
        {
            return;
        }
        this.ReportDiagnostic(property.Type.GetLocation(), propertySymbol.Name, context);
    }

    private bool IsDependencyProperty(ITypeSymbol typeSymbol)
    {
        return typeSymbol.IsInheritFrom(Utils.DependencyPropertyBase);
    }

    private void ReportDiagnostic(Location location, string memberName, SyntaxNodeAnalysisContext context)
    {
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.m_rule_CodeAnalysis0002,
            location,
            memberName);

        context.ReportDiagnostic(diagnostic);
    }
    #endregion

    private void AnalyzeAsyncToSyncMethodSymbol(OperationAnalysisContext context)
    {
        //Debugger.Launch();
        if (context.Operation is not IInvocationOperation invocationOperation)
        {
            return;
        }
        var methodSymbol = invocationOperation.TargetMethod;

        // 1. 检测目标框架
        if (!context.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.TargetFramework", out var targetFramework))
        {
            return;
        }

        if (!IsTargetFrameworkValid(targetFramework))
        {
            return;
        }

        if (methodSymbol.HasAttribute(AsyncToSyncWarningAttribute))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_CodeAnalysis0001, invocationOperation.Syntax.GetLocation(), methodSymbol.Name));
        }
    }

    public const string AsyncToSyncWarningAttribute = "TouchSocket.Core.AsyncToSyncWarningAttribute";

    private static bool IsTargetFrameworkValid(string targetFramework)
    {
        if (string.IsNullOrEmpty(targetFramework)) return false;

        // 处理不同目标框架格式：
        // - net6.0
        // - net7.0-windows
        // - net8.0.1
        if (targetFramework.StartsWith("net", StringComparison.OrdinalIgnoreCase) &&
            targetFramework.Length > 3 &&
            char.IsDigit(targetFramework[3]))
        {
            var versionString = targetFramework.Substring(3);
            var dashIndex = versionString.IndexOf('-');
            if (dashIndex > 0)
            {
                versionString = versionString.Substring(0, dashIndex);
            }

            // 处理带小数点的版本号
            var decimalIndex = versionString.IndexOf('.');
            if (decimalIndex > 0)
            {
                versionString = versionString.Substring(0, decimalIndex + 1) +
                              versionString.Substring(decimalIndex + 1).Replace(".", "");
            }

            if (decimal.TryParse(versionString, NumberStyles.Any, CultureInfo.InvariantCulture, out var version))
            {
                return version >= 6.0m;
            }
        }
        return false;
    }

    public static bool IsPluginInterface(INamedTypeSymbol namedTypeSymbol)
    {
        if (namedTypeSymbol.TypeKind != TypeKind.Interface)
        {
            return false;
        }
        if (namedTypeSymbol.ToDisplayString() == IPlugin)
        {
            return false;
        }
        if (!namedTypeSymbol.IsInheritFrom(IPlugin))
        {
            return false;
        }
        return true;
    }

    private void AnalyzePluginSymbol(SymbolAnalysisContext context)
    {
        //Debugger.Launch();
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        if (!IsPluginInterface(namedTypeSymbol))
        {
            return;
        }
        //if (namedTypeSymbol.ToDisplayString() == IPlugin)
        //{
        //    return;
        //}
        //if (!namedTypeSymbol.IsInheritFrom(IPlugin))
        //{
        //    return;
        //}

        if (namedTypeSymbol.IsGenericType)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_Plugin0002, namedTypeSymbol.Locations[0]));
            return;
        }

        var methods = namedTypeSymbol.GetMembers().OfType<IMethodSymbol>();
        var count = methods.Count();
        if (count != 1)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_Plugin0003, namedTypeSymbol.Locations[0]));
            return;
        }

        if (!namedTypeSymbol.HasAttribute(MethodInvokeSourceGenerator.DynamicMethod))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_Plugin0004, namedTypeSymbol.Locations[0]));
            return;
        }
        foreach (var methodSymbol in methods)
        {
            if (methodSymbol.ReturnType.ToDisplayString() != typeof(Task).ToString())
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_Plugin0001, methodSymbol.Locations[0]));
                return;
            }

            if (methodSymbol.Parameters.Length != 2)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_Plugin0001, methodSymbol.Locations[0]));
                return;
            }

            if (!methodSymbol.Parameters[1].Type.IsInheritFrom("TouchSocket.Core.PluginEventArgs"))
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_Plugin0001, methodSymbol.Locations[0]));
                return;
            }
        }
    }

    private void AnalyzeSymbol(OperationAnalysisContext context)
    {
        //if (context.Operation is IPropertyReferenceOperation property)
        //{
        //    if (property.Property.Name == "Buffer" && (property.Property.ContainingType.Name == "ByteBlock" || property.Property.ContainingType.Name == "ValueByteBlock"))
        //    {
        //        if (property.Parent is IPropertyReferenceOperation parent)
        //        {
        //            //Debugger.Launch();
        //            if ((parent.Property.Name == "Length" || parent.Property.Name == "LongLength") && parent.Property.ContainingType.Name == "Array")
        //            {
        //                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.m_rule_ByteBlock0001, parent.Syntax.GetLocation());
        //                context.ReportDiagnostic(diagnostic);
        //            }
        //        }
        //    }
        //}
    }
}