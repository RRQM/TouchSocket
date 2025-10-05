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
/// JsonRpc序列化转换器配置器
/// </summary>
public static class JsonRpcConverterConfigurator
{
    /// <summary>
    /// 使用默认的Json序列化格式化器
    /// </summary>
    /// <typeparam name="TOption">JsonRpc选项类型</typeparam>
    /// <param name="option">JsonRpc选项实例</param>
    /// <returns>返回当前选项实例，支持链式调用</returns>
    public static TOption UseDefaultJsonFormatter<TOption>(this TOption option)
        where TOption : JsonRpcOption
    {
        option.SerializerConverter.Add(new JsonStringToClassSerializerFormatter<JsonRpcActor>());
        return option;
    }

    /// <summary>
    /// 使用System.Text.Json序列化格式化器
    /// </summary>
    /// <typeparam name="TOption">JsonRpc选项类型</typeparam>
    /// <param name="option">JsonRpc选项实例</param>
    /// <returns>返回当前选项实例，支持链式调用</returns>
    public static TOption UseSystemTextJsonFormatter<TOption>(this TOption option)
        where TOption : JsonRpcOption
    {
        option.SerializerConverter.Add(new SystemTextJsonStringToClassSerializerFormatter<JsonRpcActor>());
        return option;
    }

    /// <summary>
    /// 使用Newtonsoft.Json序列化格式化器，并配置Json设置
    /// </summary>
    /// <typeparam name="TOption">JsonRpc选项类型</typeparam>
    /// <param name="option">JsonRpc选项实例</param>
    /// <param name="configureSettings">配置Json设置的操作</param>
    /// <returns>返回当前选项实例，支持链式调用</returns>
    public static TOption UseNewtonsoftJsonFormatter<TOption>(this TOption option, Action<Newtonsoft.Json.JsonSerializerSettings> configureSettings = null)
        where TOption : JsonRpcOption
    {
        var formatter = new JsonStringToClassSerializerFormatter<JsonRpcActor>();
        configureSettings?.Invoke(formatter.JsonSettings);
        option.SerializerConverter.Add(formatter);
        return option;
    }

    /// <summary>
    /// 使用System.Text.Json序列化格式化器，并配置Json选项
    /// </summary>
    /// <typeparam name="TOption">JsonRpc选项类型</typeparam>
    /// <param name="option">JsonRpc选项实例</param>
    /// <param name="configureOptions">配置Json选项的操作</param>
    /// <returns>返回当前选项实例，支持链式调用</returns>
    public static TOption UseSystemTextJsonFormatter<TOption>(this TOption option, Action<System.Text.Json.JsonSerializerOptions> configureOptions)
        where TOption : JsonRpcOption
    {
        var formatter = new SystemTextJsonStringToClassSerializerFormatter<JsonRpcActor>();
        configureOptions?.Invoke(formatter.JsonSettings);
        option.SerializerConverter.Add(formatter);
        return option;
    }

    /// <summary>
    /// 使用自定义序列化格式化器
    /// </summary>
    /// <typeparam name="TOption">JsonRpc选项类型</typeparam>
    /// <typeparam name="TFormatter">格式化器类型</typeparam>
    /// <param name="option">JsonRpc选项实例</param>
    /// <returns>返回当前选项实例，支持链式调用</returns>
    public static TOption UseCustomFormatter<TOption, TFormatter>(this TOption option)
        where TOption : JsonRpcOption
        where TFormatter : ISerializerFormatter<string, JsonRpcActor>, new()
    {
        option.SerializerConverter.Add(new TFormatter());
        return option;
    }

    /// <summary>
    /// 使用自定义序列化格式化器实例
    /// </summary>
    /// <typeparam name="TOption">JsonRpc选项类型</typeparam>
    /// <param name="option">JsonRpc选项实例</param>
    /// <param name="formatter">格式化器实例</param>
    /// <returns>返回当前选项实例，支持链式调用</returns>
    public static TOption UseCustomFormatter<TOption>(this TOption option, ISerializerFormatter<string, JsonRpcActor> formatter)
        where TOption : JsonRpcOption
    {
        option.SerializerConverter.Add(formatter);
        return option;
    }

    /// <summary>
    /// 移除指定类型的序列化格式化器
    /// </summary>
    /// <typeparam name="TOption">JsonRpc选项类型</typeparam>
    /// <typeparam name="TFormatter">要移除的格式化器类型</typeparam>
    /// <param name="option">JsonRpc选项实例</param>
    /// <returns>返回当前选项实例，支持链式调用</returns>
    public static TOption RemoveFormatter<TOption, TFormatter>(this TOption option)
        where TOption : JsonRpcOption
        where TFormatter : ISerializerFormatter<string, JsonRpcActor>
    {
        option.SerializerConverter.Remove(typeof(TFormatter));
        return option;
    }

    /// <summary>
    /// 清除所有序列化格式化器
    /// </summary>
    /// <typeparam name="TOption">JsonRpc选项类型</typeparam>
    /// <param name="option">JsonRpc选项实例</param>
    /// <returns>返回当前选项实例，支持链式调用</returns>
    public static TOption ClearAllFormatters<TOption>(this TOption option)
        where TOption : JsonRpcOption
    {
        option.SerializerConverter.Clear();
        return option;
    }

    /// <summary>
    /// 配置序列化转换器的高级设置
    /// </summary>
    /// <typeparam name="TOption">JsonRpc选项类型</typeparam>
    /// <param name="option">JsonRpc选项实例</param>
    /// <param name="configureAction">配置序列化转换器的操作</param>
    /// <returns>返回当前选项实例，支持链式调用</returns>
    public static TOption ConfigureAdvanced<TOption>(this TOption option, Action<TouchSocketSerializerConverter<string, JsonRpcActor>> configureAction)
        where TOption : JsonRpcOption
    {
        configureAction?.Invoke(option.SerializerConverter);
        return option;
    }
}