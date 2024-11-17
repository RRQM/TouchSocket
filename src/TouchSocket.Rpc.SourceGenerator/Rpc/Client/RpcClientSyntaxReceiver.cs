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
    internal sealed class RpcClientSyntaxReceiver : ISyntaxReceiver
    {
        public const string GeneratorRpcProxyAttributeTypeName = "TouchSocket.Rpc.GeneratorRpcProxyAttribute";

        private readonly List<InterfaceDeclarationSyntax> interfaceSyntaxList = new List<InterfaceDeclarationSyntax>();

        void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InterfaceDeclarationSyntax syntax)
            {
                this.interfaceSyntaxList.Add(syntax);
            }
        }

        public static INamedTypeSymbol GeneratorRpcProxyAttribute { get; private set; }

        public IEnumerable<INamedTypeSymbol> GetRpcApiTypes(Compilation compilation)
        {
            //Debugger.Launch();
            GeneratorRpcProxyAttribute = compilation.GetTypeByMetadataName(GeneratorRpcProxyAttributeTypeName);
            if (GeneratorRpcProxyAttribute == null)
            {
                yield break;
            }
            foreach (var interfaceSyntax in this.interfaceSyntaxList)
            {
                var @interface = compilation.GetSemanticModel(interfaceSyntax.SyntaxTree).GetDeclaredSymbol(interfaceSyntax);
                if (@interface != null && IsRpcApiInterface(@interface))
                {
                    yield return @interface;
                }
            }
        }

        /// <summary>
        /// 是否为Rpc接口
        /// </summary>
        /// <param name="interface"></param>
        /// <returns></returns>
        public static bool IsRpcApiInterface(INamedTypeSymbol @interface)
        {
            if (GeneratorRpcProxyAttribute is null)
            {
                return false;
            }
            //Debugger.Launch();
            return @interface.GetAttributes().FirstOrDefault(a =>
            {
                if (a.AttributeClass.ToDisplayString() != GeneratorRpcProxyAttribute.ToDisplayString())
                {
                    return false;
                }
                var s = GeneratorRpcProxyAttribute.ContainingAssembly.Name;
                //if (s != "TouchSocketPro")
                //{
                //    return false;
                //}
                //string key =  BitConverter.ToString(generatorRpcProxyAttribute.ContainingAssembly.Identity.PublicKeyToken.ToArray()).Replace("-", "")
                //.ToLower();
                //if (key != "efaad12a6cf1b696")
                //{
                //    return false;
                //}

                return true;
            }) is not null;
        }
    }
}