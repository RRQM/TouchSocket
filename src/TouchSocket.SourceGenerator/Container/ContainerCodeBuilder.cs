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
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TouchSocket
{
    internal sealed class ContainerCodeBuilder
    {
        private const string AddSingletonInjectAttributeString = "TouchSocket.Core.AddSingletonInjectAttribute";
        private const string AddTransientInjectAttributeString = "TouchSocket.Core.AddTransientInjectAttribute";
        private const string DependencyInjectAttributeString = "TouchSocket.Core.DependencyInjectAttribute";
        private const string DependencyTypeAttributeString = "TouchSocket.Core.DependencyTypeAttribute";
        private readonly IEnumerable<InjectDescription> m_autoInjectForSingletonClassTypes;
        private readonly IEnumerable<InjectDescription> m_autoInjectForTransientClassTypes;
        private readonly INamedTypeSymbol m_containerClass;

        public ContainerCodeBuilder(INamedTypeSymbol containerClass, IEnumerable<InjectDescription> autoInjectForSingletonClassTypes, IEnumerable<InjectDescription> autoInjectForTransientClassTypes)
        {
            this.m_containerClass = containerClass;
            this.m_autoInjectForSingletonClassTypes = autoInjectForSingletonClassTypes;
            this.m_autoInjectForTransientClassTypes = autoInjectForTransientClassTypes;
        }

        /// <summary>
        /// 依赖注入类型。
        /// </summary>
        [Flags]
        private enum DependencyType
        {
            /// <summary>
            /// 构造函数
            /// </summary>

            Constructor = 0,

            /// <summary>
            /// 属性
            /// </summary>
            Property = 1,

            /// <summary>
            /// 方法
            /// </summary>
            Method = 2
        }

        public string Prefix { get; set; }

        public IEnumerable<string> Usings
        {
            get
            {
                yield return "using System;";
                yield return "using System.Diagnostics;";
                yield return "using TouchSocket.Core;";
                yield return "using System.Threading.Tasks;";
            }
        }

        public string GetFileName()
        {
            return this.m_containerClass.ToDisplayString() + "Generator";
        }

        public override string ToString()
        {
            var codeString = new StringBuilder();
            codeString.AppendLine("/*");
            codeString.AppendLine("此代码由SourceGenerator工具直接生成，非必要请不要修改此处代码");
            codeString.AppendLine("*/");
            codeString.AppendLine("#pragma warning disable");

            foreach (var item in this.Usings)
            {
                codeString.AppendLine(item);
            }

            //Debugger.Launch();

            codeString.AppendLine($"namespace {this.m_containerClass.ContainingNamespace}");
            codeString.AppendLine("{");
            codeString.AppendLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"TouchSocket.SourceGenerator\",\"{Assembly.GetExecutingAssembly().GetName().Version.ToString()}\")]");
            codeString.AppendLine($"partial class {this.m_containerClass.Name}");
            codeString.AppendLine("{");
            var singletonInjectDescriptions = this.FindSingletonInjects().ToList();
            singletonInjectDescriptions.AddRange(this.m_autoInjectForSingletonClassTypes);

            var transientInjectDescriptions = this.FindTransientInjects().ToList();
            transientInjectDescriptions.AddRange(this.m_autoInjectForTransientClassTypes);

            this.BuildConstructor(codeString);
            this.BuildContainerInit(codeString, singletonInjectDescriptions);
            foreach (var item in singletonInjectDescriptions)
            {
                this.BuildSingletonInjectField(codeString, item);
                this.BuildInject(codeString, item);
            }

            foreach (var item in transientInjectDescriptions)
            {
                this.BuildInject(codeString, item);
            }

            this.BuildPrivateTryResolve(codeString, singletonInjectDescriptions, transientInjectDescriptions);
            this.TryBuildInvokeTryResolve(codeString);
            codeString.AppendLine("}");
            codeString.AppendLine("}");

            // System.Diagnostics.Debugger.Launch();
            return codeString.ToString();
        }

        public bool TryToSourceText(out SourceText sourceText)
        {
            var code = this.ToString();
            if (string.IsNullOrEmpty(code))
            {
                sourceText = null;
                return false;
            }
            sourceText = SourceText.From(code, Encoding.UTF8);
            return true;
        }

        #region TryResolve

        private void BuildPrivateTryResolve(StringBuilder codeString, List<InjectDescription> singletonDescriptions, List<InjectDescription> transientInjectDescriptions)
        {
            codeString.AppendLine($"private bool PrivateTryResolve(Type fromType, out object instance, string key = \"\")");
            codeString.AppendLine("{");
            codeString.AppendLine("string typeKey;");
            codeString.AppendLine("if(key.IsNullOrEmpty())");
            codeString.AppendLine("{");
            codeString.AppendLine("typeKey = fromType.FullName;");
            codeString.AppendLine("}");
            codeString.AppendLine("else");
            codeString.AppendLine("{");
            codeString.AppendLine("typeKey = $\"{fromType.FullName}{key}\";");
            codeString.AppendLine("}");
            codeString.AppendLine("switch (typeKey)");
            codeString.AppendLine("{");
            foreach (var item in singletonDescriptions)
            {
                codeString.AppendLine($"case \"{item.From.ToDisplayString()}{item.Key}\":");
                codeString.AppendLine("{");
                codeString.AppendLine($"instance = this.{this.GetFieldName(item)}.Value;");
                codeString.AppendLine("return true;");
                codeString.AppendLine("}");
            }

            foreach (var item in transientInjectDescriptions)
            {
                codeString.AppendLine($"case \"{item.From.ToDisplayString()}{item.Key}\":");
                codeString.AppendLine("{");
                codeString.AppendLine($"instance = this.{this.GetMethodName(item)}();");
                codeString.AppendLine("return true;");
                codeString.AppendLine("}");
            }

            codeString.AppendLine("default:");
            codeString.AppendLine("{");
            codeString.AppendLine("instance = default;");
            codeString.AppendLine("return false;");
            codeString.AppendLine("}");
            codeString.AppendLine("}");
            codeString.AppendLine("}");
        }

        #endregion TryResolve

        private void BuildConstructor(StringBuilder codeString)
        {
            codeString.AppendLine($"public {this.m_containerClass.Name}()");
            codeString.AppendLine("{");
            codeString.AppendLine("this.ContainerInit();");
            codeString.AppendLine("}");
        }

        private void BuildContainerInit(StringBuilder codeString, List<InjectDescription> descriptions)
        {
            if (descriptions.Count == 0)
            {
                return;
            }
            codeString.AppendLine($"private void ContainerInit()");
            codeString.AppendLine("{");
            foreach (var item in descriptions)
            {
                codeString.AppendLine($"this.{this.GetFieldName(item)} = new Lazy<{item.From.ToDisplayString()}>(this.{this.GetMethodName(item)});");
            }

            codeString.AppendLine("}");
        }

        #region SingletonInject

        private void BuildSingletonInjectField(StringBuilder codeString, InjectDescription description)
        {
            codeString.AppendLine($"private Lazy<{description.From.ToDisplayString()}> {this.GetFieldName(description)};");
        }

        private IMethodSymbol GetConstructor(INamedTypeSymbol namedTypeSymbol)
        {
            var methodSymbol = namedTypeSymbol.Constructors.FirstOrDefault();
            foreach (var item in namedTypeSymbol.Constructors)
            {
                if (item.HasAttribute(DependencyInjectAttributeString, out _))
                {
                    return item;
                }

                if (item.Parameters.Length > methodSymbol.Parameters.Length)
                {
                    methodSymbol = item;
                }
            }

            return methodSymbol;
        }

        private string GetFieldName(InjectDescription description)
        {
            if (string.IsNullOrEmpty(description.Key))
            {
                return $"m_{description.From.Name.RenameCamelCase()}";
            }
            return $"m_{description.From.Name.RenameCamelCase()}_{description.Key}";
        }

        private string GetMethodName(InjectDescription description)
        {
            if (string.IsNullOrEmpty(description.Key))
            {
                return $"Create{description.From.Name}";
            }
            return $"Create{description.From.Name}_{description.Key}";
        }

        #endregion SingletonInject

        private void BuildInject(StringBuilder codeString, InjectDescription description)
        {
            codeString.AppendLine($"private {description.From.ToDisplayString()} {this.GetMethodName(description)}()");
            codeString.Append('{');

            var constructor = this.GetConstructor(description.To);
            if (constructor == default || constructor.Parameters.Length == 0)
            {
                codeString.Append($"var obj= new {description.To.ToDisplayString()}();");
            }
            else
            {
                codeString.Append($"var obj=  new {description.To.ToDisplayString()}(");
                var ps = new List<string>();
                foreach (var item in constructor.Parameters)
                {
                    ps.Add($"({item.Type.ToDisplayString()})this.Resolve(typeof({item.Type.ToDisplayString()}))");
                }
                codeString.Append(string.Join(",", ps));
                codeString.Append($");");
            }
            var dependencyType = this.GetDependencyType(description.To);
            if (dependencyType.HasFlag(DependencyType.Property))
            {
                var properties = this.GetInjectProperties(description.To);
                foreach (var item in properties)
                {
                    if (string.IsNullOrEmpty(item.Key))
                    {
                        codeString.Append($"obj.{item.Name}=({item.Type.ToDisplayString()})this.Resolve(typeof({item.Type.ToDisplayString()}));");
                    }
                    else
                    {
                        codeString.Append($"obj.{item.Name}=({item.Type.ToDisplayString()})this.Resolve(typeof({item.Type.ToDisplayString()}),key:{item.Key});");
                    }
                }
            }

            if (dependencyType.HasFlag(DependencyType.Method))
            {
                foreach (var item in this.GetInjectMethods(description.To))
                {
                    codeString.Append($"obj.{item.Name}(");
                    var parameters = new List<string>();
                    foreach (var item2 in item.Types)
                    {
                        if (string.IsNullOrEmpty(item2.Key))
                        {
                            parameters.Add($"({item2.Type.ToDisplayString()})this.Resolve(typeof({item2.Type.ToDisplayString()}))");
                        }
                        else
                        {
                            parameters.Add($"({item2.Type.ToDisplayString()})this.Resolve(typeof({item2.Type.ToDisplayString()}),key:{item2.Key})");
                        }
                    }
                    codeString.Append(string.Join(",", parameters));
                    codeString.Append($");");
                }
            }

            codeString.Append("return obj;");
            codeString.Append('}');
        }

        private IEnumerable<InjectDescription> FindSingletonInjects()
        {
            return this.m_containerClass.GetAttributes()
                 .Where(a => a.AttributeClass?.ToDisplayString() == AddSingletonInjectAttributeString)
                 .Select(a =>
                 {
                     var list = a.ConstructorArguments;

                     INamedTypeSymbol fromTypedConstant = null;
                     INamedTypeSymbol toTypedConstant = null;
                     var key = string.Empty;
                     if (list.Length == 1)
                     {
                         fromTypedConstant = (INamedTypeSymbol)list[0].Value;
                         toTypedConstant = (INamedTypeSymbol)list[0].Value;
                     }
                     else if (list.Length == 2)
                     {
                         fromTypedConstant = (INamedTypeSymbol)list[0].Value;
                         toTypedConstant = (INamedTypeSymbol)list[1].Value;
                     }
                     else if (list.Length == 3)
                     {
                         fromTypedConstant = (INamedTypeSymbol)list[0].Value;
                         toTypedConstant = (INamedTypeSymbol)list[1].Value;
                         key = list[2].Value.ToString();
                     }

                     return new InjectDescription()
                     {
                         From = fromTypedConstant,
                         To = toTypedConstant,
                         Key = key
                     };
                 });
        }

        private IEnumerable<InjectDescription> FindTransientInjects()
        {
            return this.m_containerClass.GetAttributes()
                 .Where(a => a.AttributeClass?.ToDisplayString() == AddTransientInjectAttributeString)
                 .Select(a =>
                 {
                     var list = a.ConstructorArguments;

                     INamedTypeSymbol fromTypedConstant = null;
                     INamedTypeSymbol toTypedConstant = null;
                     var key = string.Empty;
                     if (list.Length == 1)
                     {
                         fromTypedConstant = (INamedTypeSymbol)list[0].Value;
                         toTypedConstant = (INamedTypeSymbol)list[0].Value;
                     }
                     else if (list.Length == 2)
                     {
                         fromTypedConstant = (INamedTypeSymbol)list[0].Value;
                         toTypedConstant = (INamedTypeSymbol)list[1].Value;
                     }
                     else if (list.Length == 3)
                     {
                         fromTypedConstant = (INamedTypeSymbol)list[0].Value;
                         toTypedConstant = (INamedTypeSymbol)list[1].Value;
                         key = list[2].Value.ToString();
                     }

                     return new InjectDescription()
                     {
                         From = fromTypedConstant,
                         To = toTypedConstant,
                         Key = key
                     };
                 });
        }

        private DependencyType GetDependencyType(INamedTypeSymbol namedType)
        {
            if (namedType.HasAttribute(DependencyTypeAttributeString, out var attributeData))
            {
                return (DependencyType)attributeData.ConstructorArguments[0].Value;
            }
            return DependencyType.Constructor | DependencyType.Property | DependencyType.Method;
        }

        private IEnumerable<InjectMethodDescription> GetInjectMethods(INamedTypeSymbol namedType)
        {
            //Debugger.Launch();
            var members = namedType.GetMembers();

            var descriptions = new List<InjectMethodDescription>();
            foreach (var item in members)
            {
                if (item is not IMethodSymbol method)
                {
                    continue;
                }

                if (method.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }
                if (method.MethodKind != MethodKind.Ordinary)
                {
                    continue;
                }
                if (method.HasAttribute(DependencyInjectAttributeString, out var attributeData))
                {
                    var description = new InjectMethodDescription()
                    {
                        Name = method.Name
                    };

                    description.Types = method.Parameters
                        .Select(a =>
                        {
                            var des = new InjectPropertyDescription();

                            if (a.HasAttribute(DependencyInjectAttributeString, out var attributeData))
                            {
                                if (attributeData.ConstructorArguments.Length == 0)
                                {
                                    des.Type = a.Type;
                                }
                                else if (attributeData.ConstructorArguments.Length == 1 && attributeData.ConstructorArguments[0].Kind == TypedConstantKind.Primitive)
                                {
                                    des.Type = a.Type;
                                    des.Key = attributeData.ConstructorArguments[0].Value.ToString();
                                }
                                else if (attributeData.ConstructorArguments.Length == 1 && attributeData.ConstructorArguments[0].Kind == TypedConstantKind.Type)
                                {
                                    des.Type = (ITypeSymbol)attributeData.ConstructorArguments[0].Value;
                                }
                                else if (attributeData.ConstructorArguments.Length == 2)
                                {
                                    des.Type = (ITypeSymbol)attributeData.ConstructorArguments[0].Value;
                                    des.Key = attributeData.ConstructorArguments[1].Value.ToString();
                                }
                            }
                            else
                            {
                                des.Type = a.Type;
                            }
                            return des;
                        });
                    descriptions.Add(description);
                }
            }
            return descriptions;
        }

        private IEnumerable<InjectPropertyDescription> GetInjectProperties(INamedTypeSymbol namedType)
        {
            // Debugger.Launch();
            var members = namedType.GetMembers();

            var descriptions = new List<InjectPropertyDescription>();
            foreach (var item in members)
            {
                if (item is not IPropertySymbol property)
                {
                    continue;
                }

                if (property.IsWriteOnly)
                {
                    continue;
                }

                if (property.HasAttribute(DependencyInjectAttributeString, out var attributeData))
                {
                    var description = new InjectPropertyDescription()
                    {
                        Name = property.Name
                    };

                    if (attributeData.ConstructorArguments.Length == 0)
                    {
                        description.Type = property.Type;
                    }
                    else if (attributeData.ConstructorArguments.Length == 1 && attributeData.ConstructorArguments[0].Kind == TypedConstantKind.Primitive)
                    {
                        description.Type = property.Type;
                        description.Key = attributeData.ConstructorArguments[0].Value.ToString();
                    }
                    else if (attributeData.ConstructorArguments.Length == 1 && attributeData.ConstructorArguments[0].Kind == TypedConstantKind.Type)
                    {
                        description.Type = (ITypeSymbol)attributeData.ConstructorArguments[0].Value;
                    }
                    else if (attributeData.ConstructorArguments.Length == 2)
                    {
                        description.Type = (ITypeSymbol)attributeData.ConstructorArguments[0].Value;
                        description.Key = attributeData.ConstructorArguments[1].Value.ToString();
                    }
                    descriptions.Add(description);
                }
            }
            return descriptions;
        }

        private bool HasOverrideMethod()
        {
            return this.m_containerClass
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Any(m =>
                {
                    if (m.Name == "TryResolve" && m.IsOverride && m.Parameters.Length == 3)
                    {
                        return true;
                    }
                    return false;
                });
        }

        private void TryBuildInvokeTryResolve(StringBuilder stringBuilder)
        {
            if (this.HasOverrideMethod())
            {
                return;
            }

            stringBuilder.AppendLine("protected override bool TryResolve(Type fromType, out object instance, string key = \"\")");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("if (base.TryResolve(fromType, out instance, key))");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("return true;");
            stringBuilder.AppendLine("}");
            stringBuilder.AppendLine("return this.PrivateTryResolve(fromType, out instance, key);");
            stringBuilder.AppendLine("}");
        }
    }
}