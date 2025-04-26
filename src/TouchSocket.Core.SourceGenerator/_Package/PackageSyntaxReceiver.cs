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
using System.Collections.Generic;

namespace TouchSocket;

internal sealed class PackageSyntaxReceiver : ISyntaxReceiver
{
    public const string GeneratorPackageAttributeTypeName = "TouchSocket.Core.GeneratorPackageAttribute";
    public const string IPackageTypeName = "TouchSocket.Core.IPackage";

    public static INamedTypeSymbol GeneratorPackageAttribute { get; private set; }

    /// <summary>
    /// 接口列表
    /// </summary>
    private readonly List<TypeDeclarationSyntax> m_classSyntaxList = new List<TypeDeclarationSyntax>();

    /// <summary>
    /// 访问语法树
    /// </summary>
    /// <param name="syntaxNode"></param>
    void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax syntax)
        {
            this.m_classSyntaxList.Add(syntax);
        }
        else if (syntaxNode is StructDeclarationSyntax @struct)
        {
            this.m_classSyntaxList.Add(@struct);
        }
    }

    /// <summary>
    /// 获取所有Package符号
    /// </summary>
    /// <param name="compilation"></param>
    /// <returns></returns>
    public IEnumerable<INamedTypeSymbol> GetPackageClassTypes(Compilation compilation)
    {
        // Debugger.Launch();
        GeneratorPackageAttribute = compilation.GetTypeByMetadataName(GeneratorPackageAttributeTypeName);
        if (GeneratorPackageAttribute == null)
        {
            yield break;
        }
        foreach (var classSyntax in this.m_classSyntaxList)
        {
            var @class = compilation.GetSemanticModel(classSyntax.SyntaxTree).GetDeclaredSymbol(classSyntax);
            if (@class != null && IsPackageClass(@class))
            {
                yield return @class;
            }
        }
    }

    /// <summary>
    /// 是否为容器生成
    /// </summary>
    /// <param name="class"></param>
    /// <returns></returns>
    public static bool IsPackageClass(INamedTypeSymbol @class)
    {
        if (GeneratorPackageAttribute is null)
        {
            return false;
        }
        //Debugger.Launch();

        if (!@class.HasAttribute(GeneratorPackageAttributeTypeName, out _))
        {
            return false;
        }
        if (@class.IsInheritFrom(IPackageTypeName))
        {
            return true;
        }
        return false;
    }
}