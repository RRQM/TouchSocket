//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace TouchSocket
{
    internal sealed class RpcServerSyntaxReceiver : ISyntaxReceiver
    {
        //public const string GeneratorRpcServerAttributeTypeName = "TouchSocket.Rpc.GeneratorRpcServerAttribute";
        public const string IRpcServerTypeName = "TouchSocket.Rpc.IRpcServer";

        private readonly List<ClassDeclarationSyntax> m_classDeclarationSyntaxes = new List<ClassDeclarationSyntax>();

        void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax syntax)
            {
                this.m_classDeclarationSyntaxes.Add(syntax);
            }
        }

        //public static INamedTypeSymbol GeneratorRpcServerAttribute { get; private set; }

        public IEnumerable<INamedTypeSymbol> GetRpcServerTypes(Compilation compilation)
        {
            //Debugger.Launch();
            //GeneratorRpcServerAttribute = compilation.GetTypeByMetadataName(GeneratorRpcServerAttributeTypeName);
            //if (GeneratorRpcServerAttribute == null)
            //{
            //    yield break;
            //}
            foreach (var interfaceSyntax in this.m_classDeclarationSyntaxes)
            {
                var @class = compilation.GetSemanticModel(interfaceSyntax.SyntaxTree).GetDeclaredSymbol(interfaceSyntax);
                if (@class != null && IsRpcServer(@class))
                {
                    yield return @class;
                }
            }
        }

        public static bool IsRpcServer(INamedTypeSymbol @class)
        {
            //if (GeneratorRpcServerAttribute is null)
            //{
            //    return false;
            //}
            if (!@class.AllInterfaces.Any(a => a.ToDisplayString() == IRpcServerTypeName))
            {
                return false;
            }
            //Debugger.Launch();
            //return @class.HasAttribute(GeneratorRpcServerAttributeTypeName, out _);

            if (@class.IsAbstract )
            {
                return false;
            }
            return true;
        }
    }
}