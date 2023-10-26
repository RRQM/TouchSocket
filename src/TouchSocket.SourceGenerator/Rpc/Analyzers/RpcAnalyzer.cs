using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace TouchSocket.SourceGenerator
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RpcAnalyzer : DiagnosticAnalyzer
    {
        #region DiagnosticDescriptors
        private static readonly DiagnosticDescriptor m_rule_Rpc0001 = new DiagnosticDescriptor(
            "Rpc0001", 
            "用于判断源生成接口函数是否使用了构造入参", 
            "{0}使用了构造函数设置，这在源生成时将无法生效，所以可能会导致调用失败，所以请考虑使用NamedArguments设置。", 
            "Rpc", DiagnosticSeverity.Warning, isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor m_rule_Rpc0002 = new DiagnosticDescriptor(
            "Rpc0002",
            "用于判断Rpc函数是否为静态函数",
            "{0}是静态函数，这在Rpc中是不允许的。",
            "Rpc", DiagnosticSeverity.Error, isEnabledByDefault: true);
        #endregion


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(
            m_rule_Rpc0001,
            m_rule_Rpc0002
            ); } }

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
           
            if (namedTypeSymbol.TypeKind == TypeKind.Interface&&RpcClientSyntaxReceiver.IsRpcApiInterface(namedTypeSymbol))
            {
                foreach (var item in namedTypeSymbol.GetMembers().OfType<IMethodSymbol>())
                {
                    var att = RpcClientCodeBuilder.GetRpcAttribute(item);
                    if (att!=null&&att.ConstructorArguments.Length>0)
                    {
                        
                        context.ReportDiagnostic(Diagnostic.Create(m_rule_Rpc0001, att.ApplicationSyntaxReference.GetSyntax().GetLocation(), att.AttributeClass.Name));
                    }
                }
            }
            
            if (RpcServerSyntaxReceiver.IsRpcServer(namedTypeSymbol))
            {
                foreach (var item in namedTypeSymbol.GetMembers().OfType<IMethodSymbol>())
                {
                    var att = RpcClientCodeBuilder.GetRpcAttribute(item);
                    if (att != null && item.IsStatic)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(m_rule_Rpc0002, item.Locations[0], item.Name));
                    }
                }
            }
        }
    }
}
