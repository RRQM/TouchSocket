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
using System.Linq;

namespace TouchSocket;

internal sealed class RpcServerSyntaxReceiver : ISyntaxReceiver
{
    public const string IRpcServerTypeName = "TouchSocket.Rpc.IRpcServer";

    private readonly List<TypeDeclarationSyntax> m_classDeclarationSyntaxes = new List<TypeDeclarationSyntax>();

    public static bool IsRegisterRpcServer(INamedTypeSymbol @class)
    {
        if (!@class.AllInterfaces.Any(a => a.ToDisplayString() == IRpcServerTypeName))
        {
            return false;
        }

        if (@class.IsAbstract)
        {
            return false;
        }

        return true;
    }

    public IEnumerable<INamedTypeSymbol> GetRpcServerTypes(Compilation compilation)
    {
        //Debugger.Launch();
        foreach (var interfaceSyntax in this.m_classDeclarationSyntaxes)
        {
            var @class = compilation.GetSemanticModel(interfaceSyntax.SyntaxTree).GetDeclaredSymbol(interfaceSyntax);
            if (@class != null)
            {
                yield return @class;
            }
        }
    }

    void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax syntax)
        {
            this.m_classDeclarationSyntaxes.Add(syntax);
        }

        if (syntaxNode is InterfaceDeclarationSyntax interfaceDeclarationSyntax)
        {
            this.m_classDeclarationSyntaxes.Add(interfaceDeclarationSyntax);
        }
    }
}