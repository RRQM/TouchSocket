using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket
{
    internal sealed class PluginCodeBuilder
    {
        private readonly INamedTypeSymbol m_pluginClass;

        private const string PluginEventArgsString = "TouchSocket.Core.PluginEventArgs";
        private const string PluginBaseString = "TouchSocket.Core.PluginBase";
        private const string IPluginsManagerString = "TouchSocket.Core.IPluginsManager";

        public PluginCodeBuilder(INamedTypeSymbol pluginClass)
        {
            this.m_pluginClass = pluginClass;
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
            return this.m_pluginClass.ToDisplayString() + "Generator";
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

        public override string ToString()
        {
            var methods = this.FindMethods().ToList();
            if (methods.Count == 0)
            {
                return null;
            }
            var codeString = new StringBuilder();
            codeString.AppendLine("/*");
            codeString.AppendLine("此代码由Plugin工具直接生成，非必要请不要修改此处代码");
            codeString.AppendLine("*/");
            codeString.AppendLine("#pragma warning disable");

            foreach (var item in this.Usings)
            {
                codeString.AppendLine(item);
            }

            //Debugger.Launch();

            codeString.AppendLine($"namespace {this.m_pluginClass.ContainingNamespace}");
            codeString.AppendLine("{");
            codeString.AppendLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"TouchSocket.SourceGenerator\",\"{Assembly.GetExecutingAssembly().GetName().Version.ToString()}\")]");
            codeString.AppendLine($"partial class {this.m_pluginClass.Name}");
            codeString.AppendLine("{");
            codeString.AppendLine("private int RegisterPlugins(IPluginsManager pluginsManager)");
            codeString.AppendLine("{");
            foreach (var item in methods)
            {
                this.BuildMethod(codeString, item);
            }
            codeString.AppendLine($"return {methods.Count};");
            codeString.AppendLine("}");
            this.TryBuildInvokeRedister(codeString);
            codeString.AppendLine("}");
            codeString.AppendLine("}");

            // System.Diagnostics.Debugger.Launch();
            return codeString.ToString();
        }

        private void TryBuildInvokeRedister(StringBuilder stringBuilder)
        {
            if (!this.m_pluginClass.IsInheritFrom(PluginBaseString))
            {
                return;
            }

            if (this.HasOverrideMethod())
            {
                return;
            }

            stringBuilder.AppendLine("protected override void Loaded(IPluginsManager pluginsManager)");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("base.Loaded(pluginsManager);");
            stringBuilder.AppendLine("this.RegisterPlugins(pluginsManager);");
            stringBuilder.AppendLine("}");
        }

        private void BuildMethod(StringBuilder stringBuilder, IMethodSymbol methodSymbol)
        {
            // Debugger.Launch();
            var attributeData = methodSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass.ToDisplayString() == PluginSyntaxReceiver.GeneratorPluginAttributeTypeName);
            stringBuilder.AppendLine();
            stringBuilder.Append("pluginsManager.Add<");
            stringBuilder.Append($"{methodSymbol.Parameters[0].Type.ToDisplayString()},");
            stringBuilder.Append($"{methodSymbol.Parameters[1].Type.ToDisplayString()}>(");
            stringBuilder.Append($"\"{attributeData.ConstructorArguments[0].Value}\",this.");
            stringBuilder.Append($"{methodSymbol.Name});");
            stringBuilder.AppendLine();
        }

        private bool HasOverrideMethod()
        {
            return this.m_pluginClass
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Any(m =>
                {
                    if (m.Name == "Loaded" && m.IsOverride && m.Parameters.Length == 1 && m.Parameters[0].Type.ToDisplayString() == IPluginsManagerString)
                    {
                        return true;
                    }
                    return false;
                });
        }

        private IEnumerable<IMethodSymbol> FindMethods()
        {
            return this.m_pluginClass
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m =>
                {
                    if (m.Parameters.Length != 2)
                    {
                        return false;
                    }
                    if (m.ReturnType.ToDisplayString() != typeof(Task).FullName)
                    {
                        return false;
                    }
                    if (!m.Parameters[1].Type.IsInheritFrom(PluginEventArgsString))
                    {
                        return false;
                    }
                    return m.GetAttributes().Any(a => a.AttributeClass.ToDisplayString() == PluginSyntaxReceiver.GeneratorPluginAttributeTypeName);
                });
        }
    }
}