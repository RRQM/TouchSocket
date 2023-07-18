using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace TouchSocket.SourceGenerator.Rpc
{
    /// <summary>
    /// RpcApi语法接收器
    /// </summary>
    sealed class RpcSyntaxReceiver : ISyntaxReceiver
    {
        public const string GeneratorRpcProxyAttributeTypeName = "TouchSocket.Rpc.GeneratorRpcProxyAttribute";
        public const string RpcMethodAttributeTypeName = "TouchSocket.Rpc.GeneratorRpcMethodAttribute";

        /// <summary>
        /// 接口列表
        /// </summary>
        private readonly List<InterfaceDeclarationSyntax> interfaceSyntaxList = new List<InterfaceDeclarationSyntax>();

        /// <summary>
        /// 访问语法树 
        /// </summary>
        /// <param name="syntaxNode"></param>
        void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InterfaceDeclarationSyntax syntax)
            {
                this.interfaceSyntaxList.Add(syntax);
            }
        }

        public static INamedTypeSymbol GeneratorRpcProxyAttribute { get; private set; }

        /// <summary>
        /// 获取所有RpcApi符号
        /// </summary>
        /// <param name="compilation"></param>
        /// <returns></returns>
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


        /// <summary>
        /// 返回是否声明指定的特性
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static bool HasAttribute(ISymbol symbol, INamedTypeSymbol attribute)
        {
            foreach (var attr in symbol.GetAttributes())
            {
                var attrClass = attr.AttributeClass;
                if (attrClass != null && attrClass.AllInterfaces.Contains(attribute))
                {
                    return true;
                }
            }
            return false;
        }
    }
}