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

internal class FastSerializeCodeBuilder : TypeCodeBuilder<INamedTypeSymbol>
{
    private readonly List<INamedTypeSymbol> m_namedTypeSymbols;

    public FastSerializeCodeBuilder(INamedTypeSymbol type, List<INamedTypeSymbol> namedTypeSymbols) : base(type)
    {
        this.m_namedTypeSymbols = namedTypeSymbols;
    }

    public override string GetFileName()
    {
        return this.GetGeneratorTypeName() + "Generator";
    }

    protected override bool GeneratorCode(StringBuilder codeBuilder)
    {
        using (this.CreateNamespace(codeBuilder))
        {
            codeBuilder.AppendLine($"partial class {this.GetGeneratorTypeName()}");
            codeBuilder.AppendLine("{");

            codeBuilder.AppendLine($"public {this.GetGeneratorTypeName()} ()");
            codeBuilder.AppendLine("{");
            foreach (var item in this.m_namedTypeSymbols)
            {
                this.BuildItems(codeBuilder, item);
            }
            codeBuilder.AppendLine("}");

            codeBuilder.AppendLine("}");
        }

        return true;
    }

    private void BuildItems(StringBuilder codeBuilder, INamedTypeSymbol namedTypeSymbol)
    {
        var typeName = namedTypeSymbol.ToDisplayString();
        codeBuilder.AppendLine($"this.AddConverter(typeof({typeName}), new PackageFastBinaryConverter<{typeName}>());");
    }

    private string GetGeneratorTypeName()
    {
        var typeName = this.TypeSymbol.Name;

        return typeName;
    }
}