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
using System.Text;

namespace TouchSocket;

internal class MethodInvokeTitleCodeBuilder : MethodCodeBuilder
{
    public MethodInvokeTitleCodeBuilder(INamedTypeSymbol type) : base(type)
    {
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
            codeBuilder.AppendLine($"[global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembersAttribute(global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties)]");
            codeBuilder.AppendLine($"[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
            codeBuilder.AppendLine($"[global::System.Diagnostics.DebuggerNonUserCode]");
            codeBuilder.AppendLine($"#endif");
            codeBuilder.AppendLine($"partial class {this.GetGeneratorTypeName()}");
            using (this.CreateCodeSpace(codeBuilder))
            {
                codeBuilder.AppendLine($"#if NET6_0_OR_GREATER");
                codeBuilder.AppendLine("[System.Runtime.CompilerServices.ModuleInitializer]");
                codeBuilder.AppendLine($"[System.Diagnostics.CodeAnalysis.DynamicDependency( System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties,typeof({this.GetGeneratorTypeName()}))]");
                codeBuilder.AppendLine("public static void TouchSocketModuleInitializer()");

                using (this.CreateCodeSpace(codeBuilder))
                {
                    codeBuilder.AppendLine("");
                }

                codeBuilder.AppendLine($"#endif");
            }
        }

        return true;
    }
}