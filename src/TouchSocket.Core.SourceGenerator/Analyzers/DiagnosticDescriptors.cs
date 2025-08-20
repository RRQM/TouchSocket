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

namespace TouchSocket;

internal static class DiagnosticDescriptors
{
    #region IPackage

    public static readonly DiagnosticDescriptor m_rule_Package0001 = new DiagnosticDescriptor(
"Package0001",
"判断实现IPackage接口的是结构体还是类",
"只有结构体能直接实现IPackage，如果是类，请直接或间接继承PackageBase。",
"Package", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor m_rule_Package0002 = new DiagnosticDescriptor(
"Package0002",
"判断IPackage成员必须仍然为IPackage",
"属性{0}的类型{1}并非IPackage，所以无法解决类型嵌套问题。",
"Package", DiagnosticSeverity.Error, isEnabledByDefault: true);

    #endregion IPackage

    #region ByteBlock

    #endregion ByteBlock

    #region Plugin

    public static readonly DiagnosticDescriptor m_rule_Plugin0001 = new DiagnosticDescriptor(
"Plugin0001",
"判断声明的插件接口是否符合定义",
"声明的插件方法不符合定义，要求：返回值必须为Task，参数数量必须为2，第二个参数必须继承自PluginEventArgs。",
"Plugin", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor m_rule_Plugin0002 = new DiagnosticDescriptor(
"Plugin0002",
"判断声明的插件接口是否为泛型",
"声明的插件不允许为泛型。",
"Plugin", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor m_rule_Plugin0003 = new DiagnosticDescriptor(
"Plugin0003",
"判断声明的插件接口是否仅包含一个方法",
"声明的插件接口中必须有，且仅允许一个方法。",
"Plugin", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor m_rule_Plugin0004 = new DiagnosticDescriptor(
"Plugin0004",
"判断声明的插件接口是否包含[DynamicMethod]特性",
"声明的插件接口必须标识[DynamicMethod]特性。",
"Plugin", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor m_rule_Plugin0005 = new DiagnosticDescriptor(
"Plugin0005",
"判断声明的插件触发接口是否继承了IPlugin",
"类型{0}并不是有效的插件接口类",
"Plugin", DiagnosticSeverity.Error, isEnabledByDefault: true);
    #endregion Plugin

    #region FastSerialize
    public static readonly DiagnosticDescriptor m_rule_FastSerialize0001 = new DiagnosticDescriptor(
"FastSerialize0001",
"判断添加的类型是否已实现IPackage接口",
"类型{0}并非IPackage，所以无法使用源生成Fast序列化。",
"FastSerialize", DiagnosticSeverity.Error, isEnabledByDefault: true);
    #endregion

    #region CodeAnalysis
    public static readonly DiagnosticDescriptor m_rule_CodeAnalysis0001 = new DiagnosticDescriptor(
"CodeAnalysis0001",
"判断该方法是不是由异步转为的同步",
"方法{0}是由异步转为的同步，这在net6.0及以上平台使用时并不推荐，请使用{0}Async代替。",
"CodeAnalysis", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor m_rule_CodeAnalysis0002 = new DiagnosticDescriptor(
"CodeAnalysis0002",
"判断DependencyProperty属性是否为静态只读字段或者静态只读属性",
"依赖属性{0}应该使用静态只读字段（static readonly）或者静态只读属性（不包含set访问器）可能更加推荐。",
"CodeAnalysis", DiagnosticSeverity.Warning, isEnabledByDefault: true);
    #endregion
}