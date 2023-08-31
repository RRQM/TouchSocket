using Microsoft.CodeAnalysis;
using System.Linq;

namespace TouchSocket
{
    /// <summary>
    /// HttpApi代码生成器
    /// </summary>
    [Generator]
    public class PluginSourceGenerator : ISourceGenerator
    {
        string m_generatorPluginAttribute = @"
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
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(a => 
            {
                a.AddSource(nameof(this.m_generatorPluginAttribute), this.m_generatorPluginAttribute);
            });
            context.RegisterForSyntaxNotifications(() => new PluginSyntaxReceiver());
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="context"></param>
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
                    context.AddSource($"{builder.GetFileName()}.g.cs", builder.ToSourceText());
                }
            }
        }
    }
}
