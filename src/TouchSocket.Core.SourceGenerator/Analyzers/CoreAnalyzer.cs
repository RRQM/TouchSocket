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
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
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
                DiagnosticDescriptors.m_rule_FastSerialize0001
                );
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        //context.RegisterOperationAction(this.AnalyzeSymbol, OperationKind.PropertyReference);
        context.RegisterSymbolAction(this.AnalyzePluginSymbol, SymbolKind.NamedType);
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

        if (!namedTypeSymbol.HasAttribute(MethodInvokeSyntaxReceiver.DynamicMethod))
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