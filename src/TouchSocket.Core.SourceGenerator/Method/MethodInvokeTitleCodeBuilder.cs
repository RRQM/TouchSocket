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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TouchSocket;

internal class MethodInvokeTitleCodeBuilder : MethodCodeBuilder
{
    private readonly List<IMethodSymbol> m_methodSymbols;

    public MethodInvokeTitleCodeBuilder(INamedTypeSymbol type, List<IMethodSymbol> methodSymbols) : base(type)
    {
        this.m_methodSymbols = methodSymbols;
    }

    public override string Id => this.TypeSymbol.ToDisplayString();
    public override string GetFileName()
    {
        return this.GeneratorTypeNamespace + this.GetGeneratorTypeName() + "Title.Generator.g.cs";
    }

    protected override bool GeneratorCode(StringBuilder codeBuilder)
    {
        using (this.CreateNamespaceIfNotGlobalNamespace(codeBuilder, this.GeneratorTypeNamespace))
        {
            codeBuilder.AppendLine($"[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
            codeBuilder.AppendLine($"[global::System.Obsolete(\"此方法不允许直接调用\")]");
            codeBuilder.AppendLine(Utils.GetGeneratedCodeString());

            codeBuilder.AppendLine($"#if NET6_0_OR_GREATER");
            codeBuilder.AppendLine($"[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
            codeBuilder.AppendLine($"[global::System.Diagnostics.DebuggerNonUserCode]");
            codeBuilder.AppendLine($"#endif");
            var generatedTypeName = this.GetGeneratorTypeName();
            codeBuilder.AppendLine($"partial class {generatedTypeName}");
            using (this.CreateCodeSpace(codeBuilder))
            {
                codeBuilder.AppendLine($"#if NET6_0_OR_GREATER");
                codeBuilder.AppendLine("[System.Runtime.CompilerServices.ModuleInitializer]");

                // 保留生成的扩展类型的所有成员，防止AOT裁剪
                codeBuilder.AppendLine($"[global::System.Diagnostics.CodeAnalysis.DynamicDependency(global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.All, typeof({generatedTypeName}))]");

                // 保留所有被DynamicMethodAttribute标记的方法所在的原始类型，防止AOT裁剪
                var preservedTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
                foreach (var method in this.m_methodSymbols)
                {
                    if (method.ContainingType != null && preservedTypes.Add(method.ContainingType))
                    {
                        codeBuilder.AppendLine($"[global::System.Diagnostics.CodeAnalysis.DynamicDependency(global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.All, typeof({method.ContainingType.ToDisplayString()}))]");
                    }
                }

                codeBuilder.AppendLine("public static void TouchSocketModuleInitializer()");
                using (this.CreateCodeSpace(codeBuilder))
                {
                    // 引用生成的静态成员以阻止裁剪器移除生成的代码和原始方法
                    foreach (var method in this.m_methodSymbols)
                    {
                        var methodName = method.GetDeterminantName();
                        codeBuilder.AppendLine($"_ = {methodName}Func;");
                        codeBuilder.AppendLine($"_ = {methodName}ClassProperty;");
                    }
                }

                codeBuilder.AppendLine($"#endif");
            }
        }

        return true;
    }
}