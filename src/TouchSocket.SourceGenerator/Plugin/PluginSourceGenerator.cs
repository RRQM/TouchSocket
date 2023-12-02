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
using System.Linq;

namespace TouchSocket
{
    [Generator]
    public class PluginSourceGenerator : ISourceGenerator
    {
        private string m_generatorPluginAttribute = @"
using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// 使用源生成插件的调用。
    /// </summary>
    internal class GeneratorPluginAttribute:Attribute
    {
        public string PluginName { get; set; }

        /// <summary>
        /// 使用源生成插件的调用。
        /// </summary>
        /// <param name=""pluginName"">插件名称，一般建议使用nameof()解决。</param>
        public GeneratorPluginAttribute(string pluginName)
        {
            this.PluginName = pluginName;
        }
    }
}

";

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(a =>
            {
                a.AddSource(nameof(this.m_generatorPluginAttribute), this.m_generatorPluginAttribute);
            });
            context.RegisterForSyntaxNotifications(() => new PluginSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var s = context.Compilation.GetMetadataReference(context.Compilation.Assembly);

            if (context.SyntaxReceiver is PluginSyntaxReceiver receiver)
            {
                var builders = receiver
                    .GetPluginClassTypes(context.Compilation)
                    .Select(i => new PluginCodeBuilder(i))
                    .Distinct();
                //Debugger.Launch();
                foreach (var builder in builders)
                {
                    if (builder.TryToSourceText(out var sourceText))
                    {
                        var tree = CSharpSyntaxTree.ParseText(sourceText);
                        var root = tree.GetRoot().NormalizeWhitespace();
                        var ret = root.ToFullString();
                        context.AddSource($"{builder.GetFileName()}.g.cs", ret);
                    }
                }
            }
        }
    }
}