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

namespace TouchSocket.Mcp;

/// <summary>
/// 定义 MCP 协议方法名称常量。
/// </summary>
public static class McpMethods
{
    /// <summary>initialize 方法，用于客户端与服务端握手。</summary>
    public const string Initialize = "initialize";

    /// <summary>ping 方法，用于保活检测。</summary>
    public const string Ping = "ping";

    /// <summary>tools/list 方法，枚举所有可用工具。</summary>
    public const string ToolsList = "tools/list";

    /// <summary>tools/call 方法，调用指定工具。</summary>
    public const string ToolsCall = "tools/call";

    /// <summary>resources/list 方法，枚举所有可用资源。</summary>
    public const string ResourcesList = "resources/list";

    /// <summary>resources/templates/list 方法，枚举资源模板。</summary>
    public const string ResourcesTemplatesList = "resources/templates/list";

    /// <summary>resources/read 方法，读取指定资源内容。</summary>
    public const string ResourcesRead = "resources/read";

    /// <summary>prompts/list 方法，枚举所有可用提示模板。</summary>
    public const string PromptsList = "prompts/list";

    /// <summary>prompts/get 方法，获取指定提示模板内容。</summary>
    public const string PromptsGet = "prompts/get";

    /// <summary>notifications/initialized 通知，客户端发送表示初始化完成。</summary>
    public const string NotificationsInitialized = "notifications/initialized";

    /// <summary>notifications/tools/list_changed 通知，服务端发送表示工具列表已变更。</summary>
    public const string NotificationsToolsListChanged = "notifications/tools/list_changed";

    /// <summary>notifications/resources/list_changed 通知，服务端发送表示资源列表已变更。</summary>
    public const string NotificationsResourcesListChanged = "notifications/resources/list_changed";

    /// <summary>notifications/prompts/list_changed 通知，服务端发送表示提示模板列表已变更。</summary>
    public const string NotificationsPromptsListChanged = "notifications/prompts/list_changed";
}
