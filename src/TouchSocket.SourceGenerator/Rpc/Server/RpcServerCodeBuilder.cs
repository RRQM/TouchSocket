using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace TouchSocket
{
    internal sealed class RpcServerCodeBuilder
    {
        private readonly INamedTypeSymbol m_rpcServer;

        public RpcServerCodeBuilder(INamedTypeSymbol rpcApi)
        {
            this.m_rpcServer = rpcApi;
        }

        public IEnumerable<string> Usings
        {
            get
            {
                yield return "using System;";
                yield return "using System.Diagnostics;";
                yield return "using TouchSocket.Core;";
                yield return "using TouchSocket.Sockets;";
                yield return "using TouchSocket.Rpc;";
                yield return "using System.Threading.Tasks;";
            }
        }

        public string GetFileName()
        {
            return this.m_rpcServer.ToDisplayString() + "Generator";
        }

        public SourceText ToSourceText()
        {
            var code = this.ToString();
            return SourceText.From(code, Encoding.UTF8);
        }

        public override string ToString()
        {
            var codeString = new StringBuilder();
            codeString.AppendLine("/*");
            codeString.AppendLine("此代码由Rpc工具直接生成，非必要请不要修改此处代码");
            codeString.AppendLine("*/");
            codeString.AppendLine("#pragma warning disable");

            foreach (var item in this.Usings)
            {
                codeString.AppendLine(item);
            }
            codeString.AppendLine($"namespace {this.m_rpcServer.ContainingNamespace}");
            codeString.AppendLine("{");
            codeString.AppendLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"TouchSocket.SourceGenerator\",\"{Assembly.GetExecutingAssembly().GetName().Version.ToString()}\")]");
            codeString.AppendLine($"partial class {this.m_rpcServer.Name}");
            codeString.AppendLine("{");
            var methods = this.FindMethods();

            foreach (var item in methods)
            {
                this.BuildMethodFunc(codeString, item);
                this.BuildMethod(codeString, item);
            }
            codeString.AppendLine("}");
            codeString.AppendLine("}");

            // System.Diagnostics.Debugger.Launch();
            return codeString.ToString();
        }

        private void BuildMethod(StringBuilder codeString, IMethodSymbol method)
        {
            codeString.AppendLine($"private static object {this.GetMethodName(method)}(object obj, object[] ps)");
            codeString.AppendLine("{");

            var ps = new List<string>();
            for (var i = 0; i < method.Parameters.Length; i++)
            {
                var parameter = method.Parameters[i];
                codeString.AppendLine($"var {parameter.Name}=({parameter.Type.ToDisplayString()})ps[{i}];");

                if (parameter.RefKind == RefKind.Ref)
                {
                    ps.Add($"ref {parameter.Name}");
                }
                else if (parameter.RefKind == RefKind.Out)
                {
                    ps.Add($"out {parameter.Name}");
                }
                else
                {
                    ps.Add(parameter.Name);
                }
            }
            if (ps.Count > 0)
            {
                if (method.ReturnsVoid)
                {
                    codeString.AppendLine($"(({method.ContainingType.ToDisplayString()})obj).{method.Name}({string.Join(",", ps)});");
                }
                else
                {
                    codeString.AppendLine($"var result = (({method.ContainingType.ToDisplayString()})obj).{method.Name}({string.Join(",", ps)});");
                }
            }
            else
            {
                if (method.ReturnsVoid)
                {
                    codeString.AppendLine($"(({method.ContainingType.ToDisplayString()})obj).{method.Name}();");
                }
                else
                {
                    codeString.AppendLine($"var result = (({method.ContainingType.ToDisplayString()})obj).{method.Name}();");
                }
            }
            for (var i = 0; i < method.Parameters.Length; i++)
            {
                var parameter = method.Parameters[i];

                if (parameter.RefKind == RefKind.Ref || parameter.RefKind == RefKind.Out)
                {
                    codeString.AppendLine($"ps[{i}]={parameter.Name};");
                }
            }

            if (method.ReturnsVoid)
            {
                codeString.AppendLine("return default;");
            }
            else
            {
                codeString.AppendLine("return result;");
            }
            codeString.AppendLine("}");
        }

        private void BuildMethodFunc(StringBuilder codeString, IMethodSymbol method)
        {
            codeString.AppendLine($"private static Func<object, object[], object> {this.GetMethodName(method)}Func => {this.GetMethodName(method)};");
        }

        private IEnumerable<IMethodSymbol> FindMethods()
        {
            //Debugger.Launch();
            var methods = new List<IMethodSymbol>();
            foreach (var item in this.m_rpcServer.GetMembers().OfType<IMethodSymbol>())
            {
                if (item.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }
                if (item.IsStatic)
                {
                    continue;
                }

                if (item.MethodKind != MethodKind.Ordinary)
                {
                    continue;
                }

                methods.Add(item);
            }
            return methods;
        }

        private string GetMethodName(IMethodSymbol method)
        {
            return $"{method.ContainingType.Name}{method.Name}";
        }
    }
}