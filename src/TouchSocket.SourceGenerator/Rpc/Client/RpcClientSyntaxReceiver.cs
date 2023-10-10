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
        public const string RpcMethodAttributeTypeName = "TouchSocket.Rpc.GeneratorRpcMethodAttribute";

       
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