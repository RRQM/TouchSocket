using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace TouchSocket
{

    internal sealed class ContainerSyntaxReceiver : ISyntaxReceiver
    {
        public const string GeneratorContainerAttributeTypeName = "TouchSocket.Core.GeneratorContainerAttribute";
        public const string AutoInjectForSingletonAttributeTypeName = "TouchSocket.Core.AutoInjectForSingletonAttribute";
        public const string AutoInjectForTransientAttributeTypeName = "TouchSocket.Core.AutoInjectForTransientAttribute";
        public const string ManualContainerTypeName = "TouchSocket.Core.ManualContainer";

        public static INamedTypeSymbol GeneratorContainerAttribute { get; private set; }
        public static INamedTypeSymbol AutoInjectForSingletonAttribute { get; private set; }
        public static INamedTypeSymbol AutoInjectForTransientAttribute { get; private set; }

        /// <summary>
        /// 接口列表
        /// </summary>
        private readonly List<TypeDeclarationSyntax> m_classSyntaxList = new List<TypeDeclarationSyntax>();

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
            else if (syntaxNode is InterfaceDeclarationSyntax @interface)
            {
                this.m_classSyntaxList.Add(@interface);
            }
        }

        /// <summary>
        /// 获取所有Container符号
        /// </summary>
        /// <param name="compilation"></param>
        /// <returns></returns>
        public IEnumerable<INamedTypeSymbol> GetContainerClassTypes(Compilation compilation)
        {
            // Debugger.Launch();
            GeneratorContainerAttribute = compilation.GetTypeByMetadataName(GeneratorContainerAttributeTypeName);
            if (GeneratorContainerAttribute == null)
            {
                yield break;
            }
            foreach (var classSyntax in this.m_classSyntaxList)
            {
                var @class = compilation.GetSemanticModel(classSyntax.SyntaxTree).GetDeclaredSymbol(classSyntax);
                if (@class != null && IsContainerClass(@class))
                {
                    yield return @class;
                }
            }
        }

        public IEnumerable<InjectDescription> GetAutoInjectForSingletonClassTypes(Compilation compilation)
        {
            // Debugger.Launch();
            AutoInjectForSingletonAttribute = compilation.GetTypeByMetadataName(AutoInjectForSingletonAttributeTypeName);
            if (AutoInjectForSingletonAttribute == null)
            {
                yield break;
            }
            foreach (var classSyntax in this.m_classSyntaxList)
            {
                var @class = compilation.GetSemanticModel(classSyntax.SyntaxTree).GetDeclaredSymbol(classSyntax);
                if (@class != null && IsAutoInjectForSingletonClass(@class, out var attributeData))
                {
                    yield return this.Create(@class, attributeData);
                }
            }
        }

        public IEnumerable<InjectDescription> GetAutoInjectForTransientClassTypes(Compilation compilation)
        {
            // Debugger.Launch();
            AutoInjectForTransientAttribute = compilation.GetTypeByMetadataName(AutoInjectForTransientAttributeTypeName);
            if (AutoInjectForTransientAttribute == null)
            {
                yield break;
            }
            foreach (var classSyntax in this.m_classSyntaxList)
            {
                var @class = compilation.GetSemanticModel(classSyntax.SyntaxTree).GetDeclaredSymbol(classSyntax);
                if (@class != null && IsAutoInjectForTransientClass(@class, out var attributeData))
                {
                    yield return this.Create(@class, attributeData);
                }
            }
        }

        private InjectDescription Create(INamedTypeSymbol typeSymbol, AttributeData attributeData)
        {
            var dic = attributeData.NamedArguments.ToImmutableDictionary();
            var description = new InjectDescription();
            if (dic.TryGetValue("FromType", out var typedConstant))
            {
                description.From = (INamedTypeSymbol)typedConstant.Value;
            }
            if (dic.TryGetValue("ToType", out typedConstant))
            {
                description.To = (INamedTypeSymbol)typedConstant.Value;
            }
            if (dic.TryGetValue("Key", out typedConstant))
            {
                description.Key = typedConstant.Value.ToString();
            }
            description.From ??= typeSymbol;
            description.To ??= typeSymbol;
            return description;
        }

        /// <summary>
        /// 是否为容器生成
        /// </summary>
        /// <param name="class"></param>
        /// <returns></returns>
        public static bool IsContainerClass(INamedTypeSymbol @class)
        {
            if (GeneratorContainerAttribute is null)
            {
                return false;
            }
            //Debugger.Launch();

            if (!@class.HasAttribute(GeneratorContainerAttributeTypeName, out _))
            {
                return false;
            }
            if (@class.IsInheritFrom(ManualContainerTypeName))
            {
                return true;
            }
            return false;
        }

        public static bool IsAutoInjectForSingletonClass(INamedTypeSymbol @class, out AttributeData attributeData)
        {
            if (AutoInjectForSingletonAttribute is null)
            {
                attributeData = null;
                return false;
            }
            //Debugger.Launch();

            if (@class.HasAttribute(AutoInjectForSingletonAttributeTypeName, out attributeData))
            {
                return true;
            }
            return false;
        }
        public static bool IsAutoInjectForTransientClass(INamedTypeSymbol @class, out AttributeData attributeData)
        {
            if (AutoInjectForTransientAttribute is null)
            {
                attributeData = null;
                return false;
            }
            //Debugger.Launch();

            if (@class.HasAttribute(AutoInjectForTransientAttributeTypeName, out attributeData))
            {
                return true;
            }
            return false;
        }

    }
}