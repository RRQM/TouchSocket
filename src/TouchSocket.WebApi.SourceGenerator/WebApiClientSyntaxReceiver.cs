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
using TouchSocket.Rpc;

namespace TouchSocket;

internal sealed class WebApiClientSyntaxReceiver : ISyntaxReceiver
{

    private readonly List<InterfaceDeclarationSyntax> interfaceSyntaxList = new List<InterfaceDeclarationSyntax>();

    void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is InterfaceDeclarationSyntax syntax)
        {
            this.interfaceSyntaxList.Add(syntax);
        }
    }

    public IEnumerable<INamedTypeSymbol> GetRpcApiTypes(Compilation compilation)
    {
        //Debugger.Launch();
        var generatorRpcProxyAttribute = RpcUtils.GetGeneratorRpcProxyAttribute(compilation);
        if (generatorRpcProxyAttribute == null)
        {
            yield break;
        }
        foreach (var interfaceSyntax in this.interfaceSyntaxList)
        {
            var @interface = compilation.GetSemanticModel(interfaceSyntax.SyntaxTree).GetDeclaredSymbol(interfaceSyntax);
            if (@interface != null && RpcUtils.IsRpcApiInterface(@interface))
            {
                yield return @interface;
            }
        }
    }
}