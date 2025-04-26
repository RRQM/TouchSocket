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
using System.Text;

namespace TouchSocket;

internal class MethodInvokeTitleCodeBuilder : CodeBuilder
{
    private readonly INamedTypeSymbol m_namedTypeSymbol;

    public MethodInvokeTitleCodeBuilder(INamedTypeSymbol type)
    {
        this.m_namedTypeSymbol = type;
    }

    public override string Id => this.m_namedTypeSymbol.ToDisplayString();
    public INamedTypeSymbol NamedTypeSymbol => this.m_namedTypeSymbol;

    public virtual IEnumerable<string> Usings
    {
        get
        {
            yield return "using System;";
            yield return "using System.Diagnostics;";
            yield return "using TouchSocket.Core;";
            yield return "using System.Threading.Tasks;";
        }
    }

    protected virtual string GeneratorTypeNamespace => "TouchSocket.Core.__Internals";

    public override string GetFileName()
    {
        return this.GeneratorTypeNamespace + this.GetGeneratorTypeName() + "Title.Generator";
    }


    public override string ToString()
    {
        var codeString = new StringBuilder();
        codeString.AppendLine("/*");
        codeString.AppendLine("此代码由Rpc工具直接生成，非必要请不要修改此处代码");
        codeString.AppendLine("*/");
        codeString.AppendLine("#pragma warning disable");

        foreach (var item in this.Usings)
        {
            codeString.AppendLine(item);
        }
        if (!this.NamedTypeSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            codeString.AppendLine($"namespace {this.GeneratorTypeNamespace}");
            codeString.AppendLine("{");
        }

        codeString.AppendLine($"[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
        codeString.AppendLine($"[global::System.Obsolete(\"此方法不允许直接调用\")]");
        codeString.AppendLine(Utils.GetGeneratedCodeString());

        codeString.AppendLine($"#if NET6_0_OR_GREATER");
        codeString.AppendLine($"[global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembersAttribute(global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties)]");
        codeString.AppendLine($"[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
        codeString.AppendLine($"[global::System.Diagnostics.DebuggerNonUserCode]");
        codeString.AppendLine($"#endif");
        codeString.AppendLine($"partial class {this.GetGeneratorTypeName()}");
        codeString.AppendLine("{");
        codeString.AppendLine($"#if NET6_0_OR_GREATER");
        codeString.AppendLine("[System.Runtime.CompilerServices.ModuleInitializer]");
        codeString.AppendLine($"[System.Diagnostics.CodeAnalysis.DynamicDependency( System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties,typeof({this.GetGeneratorTypeName()}))]");
        codeString.AppendLine("public static void TouchSocketModuleInitializer()");
        codeString.AppendLine("{");
        codeString.AppendLine("");
        codeString.AppendLine("}");
        codeString.AppendLine($"#endif");

        codeString.AppendLine("}");

        if (!this.NamedTypeSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            codeString.AppendLine("}");
        }

        // System.Diagnostics.Debugger.Launch();
        return codeString.ToString();
    }

    private string GetGeneratorTypeName()
    {
        var typeName = $"__{Utils.MakeIdentifier(this.m_namedTypeSymbol.ToDisplayString())}MethodExtension";

        return typeName;
    }
}