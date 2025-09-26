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

namespace TouchSocket.JsonRpc;

/// <summary>
/// 提供JsonRpcParserPlugin的扩展方法。
/// </summary>
public static class JsonRpcParserPluginExtension
{
    /// <summary>
    /// 使用System.Text.Json进行序列化
    /// </summary>
    /// <param name="jsonRpcParserPlugin">JsonRpcParserPlugin实例。</param>
    /// <param name="options">配置JsonSerializer的选项。</param>
    /// <typeparam name="TJsonRpcParserPlugin">JsonRpcParserPlugin的类型。</typeparam>
    /// <returns>配置后的JsonRpcParserPlugin实例。</returns>
    public static TJsonRpcParserPlugin UseSystemTextJson<TJsonRpcParserPlugin>(this TJsonRpcParserPlugin jsonRpcParserPlugin, Action<System.Text.Json.JsonSerializerOptions> options)
        where TJsonRpcParserPlugin : JsonRpcParserPluginBase
    {
        var serializerOptions = new System.Text.Json.JsonSerializerOptions();
        options.Invoke(serializerOptions);
        jsonRpcParserPlugin.SerializerConverter.Clear();
        jsonRpcParserPlugin.SerializerConverter.Add(new SystemTextJsonStringToClassSerializerFormatter<JsonRpcActor>() { JsonSettings = serializerOptions });

        return jsonRpcParserPlugin;
    }
}