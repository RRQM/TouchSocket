using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace TouchSocket
{
    /// <summary>
    /// RpcApi语法接收器
    /// </summary>
    internal sealed class PluginSyntaxReceiver : ISyntaxReceiver
    {
        public const string GeneratorPluginAttributeTypeName = "TouchSocket.Core.GeneratorPluginAttribute";

        /// <summary>
        /// 接口列表
        /// </summary>
        private readonly List<ClassDeclarationSyntax> m_classSyntaxList = new List<ClassDeclarationSyntax>();

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
        }

        public static INamedTypeSymbol GeneratorPluginAttributeAttribute { get; private set; }

        /// <summary>
        /// 获取所有插件符号
        /// </summary>
        /// <param name="compilation"></param>
        /// <returns></returns>
        public IEnumerable<INamedTypeSymbol> GetPluginClassTypes(Compilation compilation)
        {
            // Debugger.Launch();
            GeneratorPluginAttributeAttribute = compilation.GetTypeByMetadataName(GeneratorPluginAttributeTypeName);
            if (GeneratorPluginAttributeAttribute == null)
            {
                yield break;
            }
            foreach (var classSyntax in this.m_classSyntaxList)
            {
                var @class = compilation.GetSemanticModel(classSyntax.SyntaxTree).GetDeclaredSymbol(classSyntax);
                if (@class != null && IsPluginClass(@class))
                {
                    yield return @class;
                }
            }
        }

        /// <summary>
        /// 是否为插件
        /// </summary>
        /// <param name="class"></param>
        /// <returns></returns>
        public static bool IsPluginClass(INamedTypeSymbol @class)
        {
            if (GeneratorPluginAttributeAttribute is null)
            {
                return false;
            }
            //Debugger.Launch();

            if (@class.IsAbstract)
            {
                return false;
            }
            return @class.AllInterfaces.Any(a =>
            {
                if (a.ToDisplayString() == "TouchSocket.Core.IPlugin")
                {
                    return true;
                }
                return false;
            });
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