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
using System;
using System.Text.Json;
using TouchSocket.Core;
using TouchSocket.Http;

namespace TouchSocket.WebApi;

internal sealed class WebApiSystemTextJsonSerializerFormatter : ISerializerFormatter<string, HttpContext>
{
    private readonly JsonSerializerOptions m_jsonSerializerOptions;

    public WebApiSystemTextJsonSerializerFormatter(JsonSerializerOptions jsonSerializerOptions)
    {
        this.m_jsonSerializerOptions = jsonSerializerOptions;
    }

    public int Order { get; set; }

    public bool TryDeserialize(HttpContext state, in string source, Type targetType, out object target)
    {
        try
        {
            target = System.Text.Json.JsonSerializer.Deserialize(source, targetType, this.m_jsonSerializerOptions);
            return true;
        }
        catch
        {
            target = default;
            return false;
        }
    }

    public bool TrySerialize(HttpContext state, in object target, out string source)
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
                        source = System.Text.Json.JsonSerializer.Serialize(target, target.GetType(), this.m_jsonSerializerOptions);
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
