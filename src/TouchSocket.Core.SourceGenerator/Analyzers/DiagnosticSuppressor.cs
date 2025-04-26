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

namespace TouchSocket;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class DiagnosticSuppressor : Microsoft.CodeAnalysis.Diagnostics.DiagnosticSuppressor
{
    private static readonly SuppressionDescriptor m_rule_SAppMessage001 = new(
        "SAppMessage001",
        "CA1822",
        "AppMessage方法，所以抑制");
    public const string AppMessageAttribute = "TouchSocket.Core.AppMessageAttribute";

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            var syntaxNode = diagnostic.Location.SourceTree?.GetRoot(context.CancellationToken)
                .FindNode(diagnostic.Location.SourceSpan);

            if (syntaxNode is not null)
            {
                var semanticModel = context.GetSemanticModel(syntaxNode.SyntaxTree);

                var declaredSymbol = semanticModel.GetDeclaredSymbol(syntaxNode, context.CancellationToken);

                if (declaredSymbol is IMethodSymbol methodSymbol)
                {
                    var att = methodSymbol.HasAttribute(AppMessageAttribute);
                    if (att)
                    {
                        context.ReportSuppression(Suppression.Create(m_rule_SAppMessage001, diagnostic));
                    }
                }
            }
        }
    }

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(m_rule_SAppMessage001);
}