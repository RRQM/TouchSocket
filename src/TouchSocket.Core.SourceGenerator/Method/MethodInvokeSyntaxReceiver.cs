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
using System;
using System.Collections.Generic;
using System.Linq;

namespace TouchSocket;

internal sealed class MethodInvokeSyntaxReceiver : ISyntaxReceiver
{
    public const string DynamicMethod = "TouchSocket.Core.DynamicMethodAttribute";
    private readonly List<TypeDeclarationSyntax> m_syntaxList = new List<TypeDeclarationSyntax>();

    public static INamedTypeSymbol GeneratorPluginAttributeAttribute { get; private set; }

    public Dictionary<INamedTypeSymbol, List<IMethodSymbol>> GetInvokePairs(Compilation compilation)
    {
        var pairs = new Dictionary<INamedTypeSymbol, List<IMethodSymbol>>();

        foreach (var syntax in this.m_syntaxList)
        {
            var namedTypeSymbol = compilation.GetSemanticModel(syntax.SyntaxTree).GetDeclaredSymbol(syntax);

            switch (namedTypeSymbol.DeclaredAccessibility)
            {
                case Accessibility.ProtectedAndInternal:
                case Accessibility.Internal:
                case Accessibility.ProtectedOrInternal:
                case Accessibility.Public:
                    break;
                default:
                    continue;
            }
            //Debugger.Launch();

            var methodSymbols = this.GetMethodSymbols(namedTypeSymbol);

            if (methodSymbols.Any())
            {
                if (pairs.TryGetValue(namedTypeSymbol, out var methods))
                {
                    foreach (var item in methodSymbols)
                    {
                        if (!methods.Contains(item))
                        {
                            methods.Add(item);
                        }
                    }
                }
                else
                {
                    methods = new List<IMethodSymbol>(methodSymbols);
                    pairs.Add(namedTypeSymbol, methods);
                }
            }
        }

        return pairs;
    }

    /// <summary>
    /// 访问语法树
    /// </summary>
    /// <param name="syntaxNode"></param>
    void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is TypeDeclarationSyntax syntax)
        {
            this.m_syntaxList.Add(syntax);
        }
    }

    private IEnumerable<IMethodSymbol> GetMethodSymbols(INamedTypeSymbol namedTypeSymbol)
    {
        if (namedTypeSymbol.IsGenericType)
        {
            return Array.Empty<IMethodSymbol>();
        }
        //Debugger.Launch();
        if (this.IsDynamicMethod(namedTypeSymbol))
        {
            return namedTypeSymbol.GetMembers().OfType<IMethodSymbol>().Where(a =>
            {
                if (a.MethodKind != MethodKind.Ordinary)
                {
                    return false;
                }

                if (a.IsGenericMethod)
                {
                    return false;
                }

                switch (a.DeclaredAccessibility)
                {
                    case Accessibility.ProtectedAndInternal:
                    case Accessibility.Internal:
                    case Accessibility.ProtectedOrInternal:
                    case Accessibility.Public:
                        return true;

                    default:
                        return false;
                }
            });
        }
        else
        {
            return namedTypeSymbol.GetMembers().OfType<IMethodSymbol>().Where(a =>
            {
                if (a.MethodKind != MethodKind.Ordinary)
                {
                    return false;
                }
                if (a.IsGenericMethod)
                {
                    return false;
                }
                if (!this.IsDynamicMethod(a))
                {
                    return false;
                }
                switch (a.DeclaredAccessibility)
                {
                    case Accessibility.ProtectedAndInternal:
                    case Accessibility.Protected:
                    case Accessibility.Internal:
                    case Accessibility.ProtectedOrInternal:
                    case Accessibility.Public:
                        return true;

                    default:
                        return false;
                }
            });
        }
    }

    private bool IsDynamicMethod(ISymbol symbol)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == DynamicMethod)
            {
                return true;
            }

            if (attribute.AttributeClass.HasAttribute(DynamicMethod))
            {
                return true;
            }
        }
        return false;
    }
}