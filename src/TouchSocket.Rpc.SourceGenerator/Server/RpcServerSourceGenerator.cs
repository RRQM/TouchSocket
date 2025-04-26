//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace TouchSocket;

[Generator]
public class RpcServerSourceGenerator : ISourceGenerator
{
    public const string GeneratorRpcServerRegisterAttributeTypeName = "TouchSocket.Rpc.GeneratorRpcServerRegisterAttribute";

    private readonly string m_generatorServerAttribute = @"

/*
此代码由SourceGenerator工具直接生成，非必要请不要修改此处代码
*/

#pragma warning disable

using System;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 标识将通过源生成器生成Rpc服务的调用委托。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface | AttributeTargets.Assembly)]
    [Obsolete(""此特性已被弃用，默认已支持AOT"")]
    /*GeneratedCode*/
    internal class GeneratorRpcServerAttribute : Attribute
    {
    }

    /// <summary>
    /// 标识将通过源生成器生成Rpc服务的注册代码。
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    /*GeneratedCode*/
    internal class GeneratorRpcServerRegisterAttribute : Attribute
    {
        /// <summary>
        /// 方法名称。默认是“RegisterAllFrom+AssemblyName”
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// 扩展类类名，默认是“RegisterRpcServerFrom+AssemblyName+Extension”
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// 访问修饰。
        /// <para>
        /// 如果为<see cref=""Accessibility.Both""/>，将生成注册公共Rpc服务与非公共服务两个方法。其中非公共方法会在<see cref=""MethodName""/>之前以Internal开头。
        /// 如果为<see cref=""Accessibility.Internal""/>，将只生成注册非公共Rpc服务。
        /// 如果为<see cref=""Accessibility.Public""/>，将只生成注册公共Rpc服务。
        /// </para>
        /// </summary>
        public Accessibility Accessibility { get; set; }
    }

    /*GeneratedCode*/
    internal enum Accessibility
    {
        Both,
        Internal,
        Public
    }
}
";

    public void Execute(GeneratorExecutionContext context)
    {
        var s = context.Compilation.GetMetadataReference(context.Compilation.Assembly);

        if (context.SyntaxReceiver is RpcServerSyntaxReceiver receiver)
        {
            var rpcServerTypes = receiver
                .GetRpcServerTypes(context.Compilation)
                .Distinct(SymbolEqualityComparer.Default)
                .Cast<INamedTypeSymbol>();
            // Debugger.Launch();

            var generatorRpcServerRegisterAttribute = context.Compilation.GetTypeByMetadataName(GeneratorRpcServerRegisterAttributeTypeName);
            if (generatorRpcServerRegisterAttribute == null)
            {
                return;
            }
            if (context.Compilation.Assembly.HasAttribute(GeneratorRpcServerRegisterAttributeTypeName, out var attributeData))
            {
                var registerCodeBuilder = new RegisterRpcServerCodeBuilder(context.Compilation.Assembly, attributeData, rpcServerTypes.Where(a => RpcServerSyntaxReceiver.IsRegisterRpcServer(a)).ToArray());
                if (registerCodeBuilder.RpcApis.Length > 0)
                {
                    context.AddSource($"{registerCodeBuilder.GetFileName()}.g.cs", registerCodeBuilder.ToSourceText());
                }
            }
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(a =>
        {
            var sourceCode = this.m_generatorServerAttribute.Replace("/*GeneratedCode*/", $"[global::System.CodeDom.Compiler.GeneratedCode(\"TouchSocket.SourceGenerator\",\"{Assembly.GetExecutingAssembly().GetName().Version.ToString()}\")]");

            a.AddSource(nameof(this.m_generatorServerAttribute), sourceCode);
        });
        context.RegisterForSyntaxNotifications(() => new RpcServerSyntaxReceiver());
    }
}