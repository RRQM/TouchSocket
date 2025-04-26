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
using TouchSocket.SourceGenerator;

namespace TouchSocket;

/// <summary>
/// RpcApi语法接收器
/// </summary>
internal sealed class PluginSyntaxReceiver : ISyntaxReceiver
{
    //public const string GeneratorPluginAttributeTypeName = "TouchSocket.Core.GeneratorPluginAttribute";

    /// <summary>
    /// 接口列表
    /// </summary>
    private readonly List<InterfaceDeclarationSyntax> m_classSyntaxList = new();

    //public static INamedTypeSymbol GeneratorPluginAttributeAttribute { get; private set; }



    /// <summary>
    /// 获取所有插件符号
    /// </summary>
    /// <param name="compilation"></param>
    /// <returns></returns>
    public IEnumerable<INamedTypeSymbol> GetPluginPluginInterfaceTypes(Compilation compilation)
    {
        // Debugger.Launch();
        //GeneratorPluginAttributeAttribute = compilation.GetTypeByMetadataName(GeneratorPluginAttributeTypeName);
        //if (GeneratorPluginAttributeAttribute == null)
        //{
        //    yield break;
        //}
        foreach (var classSyntax in this.m_classSyntaxList)
        {
            var @class = compilation.GetSemanticModel(classSyntax.SyntaxTree).GetDeclaredSymbol(classSyntax);
            if (@class != null && CoreAnalyzer.IsPluginInterface(@class) && @class.DeclaredAccessibility == Accessibility.Public)
            {
                yield return @class;
            }
        }
    }

    /// <summary>
    /// 访问语法树
    /// </summary>
    /// <param name="syntaxNode"></param>
    void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is InterfaceDeclarationSyntax syntax)
        {

            this.m_classSyntaxList.Add(syntax);
        }
    }
}