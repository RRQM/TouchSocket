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
        private const string IPluginManagerString = "TouchSocket.Core.IPluginManager";

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
            codeString.AppendLine("private int RegisterPlugins(IPluginManager pluginManager)");
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

            stringBuilder.AppendLine("protected override void Loaded(IPluginManager pluginManager)");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("base.Loaded(pluginManager);");
            stringBuilder.AppendLine("this.RegisterPlugins(pluginManager);");
            stringBuilder.AppendLine("}");
        }

        private void BuildMethod(StringBuilder stringBuilder, IMethodSymbol methodSymbol)
        {
            // Debugger.Launch();
            var attributeData = methodSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass.ToDisplayString() == PluginSyntaxReceiver.GeneratorPluginAttributeTypeName);
            stringBuilder.AppendLine();
            stringBuilder.Append("pluginManager.Add<");
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
                    if (m.Name == "Loaded" && m.IsOverride && m.Parameters.Length == 1 && m.Parameters[0].Type.ToDisplayString() == IPluginManagerString)
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