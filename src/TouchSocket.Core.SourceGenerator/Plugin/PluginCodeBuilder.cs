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
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TouchSocket;

internal sealed class PluginCodeBuilder : CodeBuilder
{
    private const string IPluginManagerString = "TouchSocket.Core.IPluginManager";
    private const string PluginBaseString = "TouchSocket.Core.PluginBase";
    private const string PluginEventArgsString = "TouchSocket.Core.PluginEventArgs";
    private readonly INamedTypeSymbol m_pluginClass;

    public PluginCodeBuilder(INamedTypeSymbol pluginClass)
    {
        this.m_pluginClass = pluginClass;
    }

    public override string Id => this.m_pluginClass.ToDisplayString();
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

    public override string GetFileName()
    {
        return this.m_pluginClass.ToDisplayString() + "ExtensionsGenerator";
    }

    public override string ToString()
    {
        var method = this.FindMethod();
        if (method is null)
        {
            return null;
        }
        if (method.Parameters.Length != 2)
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

        var pluginClassName = this.GetPluginClassName();
        var pluginMethodName = method.Name;
        var firstType = method.Parameters[0].Type;
        var secondType = method.Parameters[1].Type;
        // var xml = ExtractSummary((method.GetDocumentationCommentXml() ?? this.m_pluginClass.GetDocumentationCommentXml()) ?? string.Empty);

        if (!this.m_pluginClass.ContainingNamespace.IsGlobalNamespace)
        {
            codeString.AppendLine($"namespace {this.m_pluginClass.ContainingNamespace}");
            codeString.AppendLine("{");
        }

        codeString.AppendLine($"/// <inheritdoc cref=\"{this.m_pluginClass.ToDisplayString()}\"/>");
        codeString.AppendLine($"public static class _{pluginClassName}Extensions");
        codeString.AppendLine("{");
        //1
        codeString.AppendLine($"/// <inheritdoc cref=\"{this.m_pluginClass.ToDisplayString()}.{method.Name}({firstType.ToDisplayString()},{secondType.ToDisplayString()})\"/>");
        codeString.AppendLine($"public static void Add{pluginClassName}(this IPluginManager pluginManager, Func<{firstType.ToDisplayString()}, {secondType.ToDisplayString()}, Task> func)");

        codeString.AppendLine("{");
        codeString.AppendLine($"pluginManager.Add(typeof({this.m_pluginClass.ToDisplayString()}), func);");
        codeString.AppendLine("}");

        //2
        codeString.AppendLine($"/// <inheritdoc cref=\"{this.m_pluginClass.ToDisplayString()}.{method.Name}({firstType.ToDisplayString()},{secondType.ToDisplayString()})\"/>");
        codeString.AppendLine($"public static void Add{pluginClassName}(this IPluginManager pluginManager, Action<{firstType.ToDisplayString()}> action)");

        codeString.AppendLine("{");
        codeString.AppendLine("Task newFunc(object sender, PluginEventArgs e)");
        codeString.AppendLine("{");
        codeString.AppendLine($"action(({firstType.ToDisplayString()})sender);");
        codeString.AppendLine("return e.InvokeNext();");
        codeString.AppendLine("}");
        codeString.AppendLine($"pluginManager.Add(typeof({this.m_pluginClass.ToDisplayString()}), newFunc, action);");
        codeString.AppendLine("}");

        //3
        codeString.AppendLine($"/// <inheritdoc cref=\"{this.m_pluginClass.ToDisplayString()}.{method.Name}({firstType.ToDisplayString()},{secondType.ToDisplayString()})\"/>");
        codeString.AppendLine($"public static void Add{pluginClassName}(this IPluginManager pluginManager, Action action)");

        codeString.AppendLine("{");
        codeString.AppendLine($"pluginManager.Add(typeof({this.m_pluginClass.ToDisplayString()}), action);");
        codeString.AppendLine("}");

        //4
        codeString.AppendLine($"/// <inheritdoc cref=\"{this.m_pluginClass.ToDisplayString()}.{method.Name}({firstType.ToDisplayString()},{secondType.ToDisplayString()})\"/>");
        codeString.AppendLine($"public static void Add{pluginClassName}(this IPluginManager pluginManager, Func<{secondType.ToDisplayString()},Task> func)");

        codeString.AppendLine("{");
        codeString.AppendLine($"pluginManager.Add(typeof({this.m_pluginClass.ToDisplayString()}), func);");
        codeString.AppendLine("}");

        //5
        codeString.AppendLine($"/// <inheritdoc cref=\"{this.m_pluginClass.ToDisplayString()}.{method.Name}({firstType.ToDisplayString()},{secondType.ToDisplayString()})\"/>");
        codeString.AppendLine($"public static void Add{pluginClassName}(this IPluginManager pluginManager, Func<{firstType.ToDisplayString()},Task> func)");

        codeString.AppendLine("{");
        codeString.AppendLine("async Task newFunc(object sender, PluginEventArgs e)");
        codeString.AppendLine("{");
        codeString.AppendLine($"await func(({firstType.ToDisplayString()})sender).ConfigureAwait(EasyTask.ContinueOnCapturedContext);");
        codeString.AppendLine("await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);");
        codeString.AppendLine("}");
        codeString.AppendLine($"pluginManager.Add(typeof({this.m_pluginClass.ToDisplayString()}), newFunc, func);");
        codeString.AppendLine("}");

        //6
        codeString.AppendLine($"/// <inheritdoc cref=\"{this.m_pluginClass.ToDisplayString()}.{method.Name}({firstType.ToDisplayString()},{secondType.ToDisplayString()})\"/>");
        codeString.AppendLine($"public static void Add{pluginClassName}(this IPluginManager pluginManager, Func<Task> func)");

        codeString.AppendLine("{");
        codeString.AppendLine($"pluginManager.Add(typeof({this.m_pluginClass.ToDisplayString()}), func);");
        codeString.AppendLine("}");

        //class end
        codeString.AppendLine("}");
        if (!this.m_pluginClass.ContainingNamespace.IsGlobalNamespace)
        {
            codeString.AppendLine("}");
        }

        // System.Diagnostics.Debugger.Launch();
        return codeString.ToString();
    }

    private string ExtractSummary(string xmlDoc)
    {
        if (string.IsNullOrEmpty(xmlDoc))
        {
            return string.Empty;
        }
        try
        {
            var doc = XDocument.Parse(xmlDoc);
            var summaryElement = doc.Descendants("summary").FirstOrDefault();
            var summary = summaryElement?.Value.Trim();

            if (string.IsNullOrEmpty(summary))
            {
                return string.Empty;
            }
            //去掉换行符
            return summary.Replace("\n", "").Replace("\r", "");
        }
        catch
        {
            return null;
        }
    }

    private string GetPluginClassName()
    {
        var name = this.m_pluginClass.Name;
        if (name.StartsWith("I"))
        {
            return name.Substring(1);
        }
        return this.m_pluginClass.Name;
    }

    private IMethodSymbol FindMethod()
    {
        return this.m_pluginClass.GetMembers().OfType<IMethodSymbol>().FirstOrDefault();
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


}