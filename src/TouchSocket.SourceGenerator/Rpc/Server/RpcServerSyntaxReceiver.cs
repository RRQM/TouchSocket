using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace TouchSocket
{

    internal sealed class RpcServerSyntaxReceiver : ISyntaxReceiver
    {
        public const string GeneratorRpcServerAttributeTypeName = "TouchSocket.Rpc.GeneratorRpcServerAttribute";
        public const string IRpcServerTypeName = "TouchSocket.Rpc.IRpcServer";


        private readonly List<ClassDeclarationSyntax> interfaceSyntaxList = new List<ClassDeclarationSyntax>();


        void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax syntax)
            {
                this.interfaceSyntaxList.Add(syntax);
            }
        }

        public static INamedTypeSymbol GeneratorRpcServerAttribute { get; private set; }


        public IEnumerable<INamedTypeSymbol> GetRpcServerTypes(Compilation compilation)
        {
            //Debugger.Launch();
            GeneratorRpcServerAttribute = compilation.GetTypeByMetadataName(GeneratorRpcServerAttributeTypeName);
            if (GeneratorRpcServerAttribute == null)
            {
                yield break;
            }
            foreach (var interfaceSyntax in this.interfaceSyntaxList)
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
            if (GeneratorRpcServerAttribute is null)
            {
                return false;
            }
            if (!@class.AllInterfaces.Any(a => a.ToDisplayString() == IRpcServerTypeName))
            {
                return false;
            }
            //Debugger.Launch();
            return @class.HasAttribute(GeneratorRpcServerAttributeTypeName, out _);
        }
    }
}