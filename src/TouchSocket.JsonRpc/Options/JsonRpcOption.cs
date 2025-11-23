// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

namespace TouchSocket.JsonRpc;

/// <summary>
/// JsonRpc配置选项
/// </summary>
public class JsonRpcOption
{
    public JsonRpcOption()
    {
        this.m_serializerConverter.Add(new JsonStringToClassSerializerFormatter<JsonRpcActor>());
    }
    private readonly TouchSocketSerializerConverter<string, JsonRpcActor> m_serializerConverter = new TouchSocketSerializerConverter<string, JsonRpcActor>();

    /// <summary>
    /// 获取序列化转换器
    /// </summary>
    public TouchSocketSerializerConverter<string, JsonRpcActor> SerializerConverter => this.m_serializerConverter;

    /// <summary>
    /// 清除所有序列化格式化器
    /// </summary>
    public void ClearAllFormatters()
    {
        this.SerializerConverter.Clear();
    }

    /// <summary>
    /// 配置序列化转换器的高级设置
    /// </summary>
    /// <param name="configureAction">配置序列化转换器的操作</param>
    public void ConfigureAdvanced(Action<TouchSocketSerializerConverter<string, JsonRpcActor>> configureAction)
    {
        configureAction?.Invoke(this.SerializerConverter);
    }

    /// <summary>
    /// 移除指定类型的序列化格式化器
    /// </summary>
    /// <typeparam name="TFormatter">要移除的格式化器类型</typeparam>
    public void RemoveFormatter<TFormatter>() where TFormatter : ISerializerFormatter<string, JsonRpcActor>
    {
        this.SerializerConverter.Remove(typeof(TFormatter));
    }

    /// <summary>
    /// 使用自定义序列化格式化器
    /// </summary>
    /// <typeparam name="TFormatter">格式化器类型</typeparam>
    public void UseCustomFormatter<TFormatter>() where TFormatter : ISerializerFormatter<string, JsonRpcActor>, new()
    {
        this.SerializerConverter.Add(new TFormatter());
    }

    /// <summary>
    /// 使用自定义序列化格式化器实例
    /// </summary>
    /// <param name="formatter">格式化器实例</param>
    public void UseCustomFormatter(ISerializerFormatter<string, JsonRpcActor> formatter)
    {
        this.SerializerConverter.Add(formatter);
    }

    /// <summary>
    /// 使用默认的Json序列化格式化器
    /// </summary>
    public void UseDefaultJsonFormatter()
    {
        this.SerializerConverter.Add(new JsonStringToClassSerializerFormatter<JsonRpcActor>());
    }

    /// <summary>
    /// 使用Newtonsoft.Json序列化格式化器，并配置Json设置
    /// </summary>
    /// <param name="configureSettings">配置Json设置的操作</param>
    public void UseNewtonsoftJsonFormatter(Action<Newtonsoft.Json.JsonSerializerSettings> configureSettings = null)
    {
        var formatter = new JsonStringToClassSerializerFormatter<JsonRpcActor>();
        configureSettings?.Invoke(formatter.JsonSettings);
        this.SerializerConverter.Add(formatter);
    }

    /// <summary>
    /// 使用System.Text.Json序列化格式化器
    /// </summary>
    public void UseSystemTextJsonFormatter()
    {
        this.SerializerConverter.Add(new SystemTextJsonStringToClassSerializerFormatter<JsonRpcActor>());
    }

    /// <summary>
    /// 使用System.Text.Json序列化格式化器，并配置Json选项
    /// </summary>
    /// <param name="configureOptions">配置Json选项的操作</param>
    public void UseSystemTextJsonFormatter(Action<System.Text.Json.JsonSerializerOptions> configureOptions)
    {
        var formatter = new SystemTextJsonStringToClassSerializerFormatter<JsonRpcActor>();
        configureOptions?.Invoke(formatter.JsonSettings);
        this.SerializerConverter.Add(formatter);
    }
}