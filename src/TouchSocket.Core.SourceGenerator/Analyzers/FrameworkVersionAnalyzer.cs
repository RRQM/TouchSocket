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

//namespace ClassLibrary1
//{
//    [Generator]
//    public class MethodUsageAnalyzer : IIncrementalGenerator
//    {
//        private const string SpecialAttributeName = "MyNamespace.SpecialAttribute";

//        public void Initialize(IncrementalGeneratorInitializationContext context)
//        {
//            // 第一步：收集所有方法调用语法节点
//            var invocationNodes = context.SyntaxProvider
//                .CreateSyntaxProvider(
//                    predicate: static (s, _) => s is InvocationExpressionSyntax,
//                    transform: static (ctx, _) => (Invocation: (InvocationExpressionSyntax)ctx.Node, ctx.SemanticModel)
//                );

//            // 第二步：获取项目配置
//            var configOptions = context.AnalyzerConfigOptionsProvider;

//            // 第三步：组合输入数据
//            var combined = invocationNodes.Combine(configOptions);

//            // 第四步：注册分析操作
//            context.RegisterSourceOutput(combined, (spc, tuple) =>
//            {
//                var ((invocation, semanticModel), options) = tuple;

//                // 1. 检测目标框架
//                if (!options.GlobalOptions.TryGetValue("build_property.TargetFramework", out var targetFramework))
//                    return;

//                if (!IsTargetFrameworkValid(targetFramework))
//                    return;

//                // 2. 获取被调用方法符号
//                var symbolInfo = semanticModel.GetSymbolInfo(invocation);
//                if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
//                    return;

//                // 3. 检查特性标记
//                if (HasSpecialAttribute(methodSymbol))
//                {
//                    var location = invocation.GetLocation();
//                    var diagnostic = Diagnostic.Create(
//                        descriptor: Rules.RestrictedMethodWarning,
//                        location: location,
//                        methodSymbol.Name
//                    );
//                    spc.ReportDiagnostic(diagnostic);
//                }
//            });
//        }

//        private static bool IsTargetFrameworkValid(string targetFramework)
//        {
//            if (string.IsNullOrEmpty(targetFramework)) return false;

//            // 处理不同目标框架格式：
//            // - net6.0
//            // - net7.0-windows
//            // - net8.0.1
//            if (targetFramework.StartsWith("net", StringComparison.OrdinalIgnoreCase) &&
//                targetFramework.Length > 3 &&
//                char.IsDigit(targetFramework[3]))
//            {
//                var versionString = targetFramework.Substring(3);
//                var dashIndex = versionString.IndexOf('-');
//                if (dashIndex > 0)
//                {
//                    versionString = versionString.Substring(0, dashIndex);
//                }

//                // 处理带小数点的版本号
//                var decimalIndex = versionString.IndexOf('.');
//                if (decimalIndex > 0)
//                {
//                    versionString = versionString.Substring(0, decimalIndex + 1) +
//                                  versionString.Substring(decimalIndex + 1).Replace(".", "");
//                }

//                if (decimal.TryParse(versionString, NumberStyles.Any, CultureInfo.InvariantCulture, out var version))
//                {
//                    return version >= 6.0m;
//                }
//            }
//            return false;
//        }

//        private static bool HasSpecialAttribute(IMethodSymbol methodSymbol)
//        {
//            foreach (var attribute in methodSymbol.GetAttributes())
//            {
//                if (attribute.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ==
//                    "global::MyNamespace.SpecialAttribute")
//                {
//                    return true;
//                }
//            }

//            // 检查覆盖方法的情况
//            if (methodSymbol.OverriddenMethod != null)
//            {
//                return HasSpecialAttribute(methodSymbol.OverriddenMethod);
//            }

//            return false;
//        }
//    }

//    public static class Rules
//    {
//        public static readonly DiagnosticDescriptor RestrictedMethodWarning = new(
//            id: "SG2001",
//            title: "Restricted method usage",
//            messageFormat: "Method '{0}' is marked as restricted in .NET 6+ environment",
//            category: "Usage",
//            defaultSeverity: DiagnosticSeverity.Warning,
//            isEnabledByDefault: true,
//            description: "This method has special restrictions in modern .NET versions");
//    }
//}
