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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TouchSocket.Rpc;

namespace TouchSocket.SourceGenerator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RpcAnalyzer : DiagnosticAnalyzer
{
    #region DiagnosticDescriptors

    private static readonly DiagnosticDescriptor m_rule_Rpc0001 = new DiagnosticDescriptor(
        "Rpc0001",
        "用于判断源生成接口函数是否使用了构造入参",
        "{0}使用了构造函数设置，这在源生成时将无法生效，所以可能会导致调用失败，所以请考虑使用NamedArguments设置。例如：“MethodInvoke =true”",
        "Rpc", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor m_rule_Rpc0002 = new DiagnosticDescriptor(
        "Rpc0002",
        "用于判断Rpc函数是否为静态函数",
        "{0}是静态函数，这在Rpc中是不允许的。",
        "Rpc", DiagnosticSeverity.Error, isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor m_rule_Rpc0003 = new DiagnosticDescriptor(
       "Rpc0003",
       "用于判断Rpc函数是否为异步void",
       "{0}是异步Rpc，但是返回值为void，这在Rpc中是不允许的，请将返回值改为Task。",
       "Rpc", DiagnosticSeverity.Error, isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor m_rule_Rpc0004 = new DiagnosticDescriptor(
       "Rpc0004",
       "用于判断Rpc函数是否有重载函数",
       "Rpc中{0}函数出现重载，这是不允许的。",
       "Rpc", DiagnosticSeverity.Error, isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor m_rule_Rpc0005 = new DiagnosticDescriptor(
       "Rpc0005",
       "用于判断Rpc函数是否有ref、out、in参数",
       "Rpc中{0}函数出现ref、out、in参数，这是不允许的。",
       "Rpc", DiagnosticSeverity.Error, isEnabledByDefault: true);

    #endregion DiagnosticDescriptors

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(
        m_rule_Rpc0001,
        m_rule_Rpc0002,
        m_rule_Rpc0003,
        m_rule_Rpc0004,
        m_rule_Rpc0005
        );
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        if (namedTypeSymbol.TypeKind == TypeKind.Interface && RpcUtils.IsRpcApiInterface(namedTypeSymbol))
        {
            foreach (var methodSymbol in namedTypeSymbol.GetMembers().OfType<IMethodSymbol>())
            {
                var att = RpcUtils.GetRpcAttribute(methodSymbol);
                if (att != null && att.ConstructorArguments.Length > 0)
                {
                    context.ReportDiagnostic(Diagnostic.Create(m_rule_Rpc0001, att.ApplicationSyntaxReference.GetSyntax().GetLocation(), att.AttributeClass.Name));
                }
            }
        }
        //Debugger.Launch();
        if (namedTypeSymbol.AllInterfaces.Any(a => a.ToDisplayString() == RpcServerSyntaxReceiver.IRpcServerTypeName))
        {
            var names = new List<string>();
            foreach (var methodSymbol in namedTypeSymbol.GetMembers().OfType<IMethodSymbol>())
            {
                var att = RpcUtils.GetRpcAttribute(methodSymbol);
                if (att != null)//具有rpc特性
                {
                    if (methodSymbol.IsStatic)//静态
                    {
                        context.ReportDiagnostic(Diagnostic.Create(m_rule_Rpc0002, methodSymbol.Locations[0], methodSymbol.Name));
                    }

                    foreach (var item in methodSymbol.Parameters)
                    {
                        if (item.RefKind != RefKind.None)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(m_rule_Rpc0005, methodSymbol.Locations[0], methodSymbol.Name));
                        }
                    }

                    if (names.Contains(methodSymbol.Name))//有重载
                    {
                        context.ReportDiagnostic(Diagnostic.Create(m_rule_Rpc0004, methodSymbol.Locations[0], methodSymbol.Name));
                    }
                    else
                    {
                        names.Add(methodSymbol.Name);
                    }
                }

                if (methodSymbol.ReturnsVoid && methodSymbol.IsAsync)
                {
                    context.ReportDiagnostic(Diagnostic.Create(m_rule_Rpc0003, methodSymbol.Locations[0], methodSymbol.Name));
                }
            }
        }
    }
}