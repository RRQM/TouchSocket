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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using TouchSocket.Http;

namespace TouchSocket.WebApi;

[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "使用该序列化时，会和源生成配合使用")]
[UnconditionalSuppressMessage("AOT", "IL3050:", Justification = "使用该序列化时，会和源生成配合使用")]
internal sealed class WebApiSystemTextJsonSerializerFormatter : SystemTextJsonSerializerFormatter<string, HttpContext>
{
    public WebApiSystemTextJsonSerializerFormatter(JsonSerializerOptions jsonSerializerOptions)
    {
        this.JsonSettings = jsonSerializerOptions;
    }

    public override bool TryDeserialize(HttpContext state, in string source, Type targetType, out object target)
    {
        try
        {
            target = System.Text.Json.JsonSerializer.Deserialize(source, targetType, this.JsonSettings);
            return true;
        }
        catch
        {
            target = default;
            return false;
        }
    }

    public override bool TrySerialize<TTarget>(HttpContext state, in TTarget target, out string source)
    {
        switch (state.Request.Accept)
        {
            case "application/xml":
            case "text/xml":
                {
                    source = default;
                    return false;
                }
            case "application/json":
            case "text/json":
            case "text/plain":
            default:
                {
                    try
                    {
                        source = System.Text.Json.JsonSerializer.Serialize(target, target.GetType(), this.JsonSettings);
                        return true;
                    }
                    catch
                    {
                        source = default;
                        return false;
                    }
                }
        }
    }
}
