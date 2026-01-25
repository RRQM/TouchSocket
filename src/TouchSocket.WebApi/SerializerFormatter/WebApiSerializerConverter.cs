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
using TouchSocket.Http;

namespace TouchSocket.WebApi;

/// <summary>
/// 适用于WebApi的序列化器
/// </summary>
public class WebApiSerializerConverter : TouchSocketSerializerConverter<string, HttpContext>
{
    /// <summary>
    /// 适用于WebApi的序列化器
    /// </summary>
    public WebApiSerializerConverter()
    {
        this.AddSystemTextJsonSerializerFormatter(options =>
        {
            options.TypeInfoResolverChain.Clear();
            options.TypeInfoResolver = default;
        });
    }
    /// <inheritdoc/>
    public override string Serialize(HttpContext state, in object target)
    {
        if (target == null)
        {
            return string.Empty;
        }
        var accept = state.Request.Accept;
        if (accept.Equals("text/plain"))
        {
            if ((target.GetType().IsPrimitive || target.GetType() == typeof(string)))
            {
                return target.ToString();
            }
        }
        return base.Serialize(state, target);
    }

    /// <summary>
    /// 添加Xml序列化器
    /// </summary>
    [RequiresUnreferencedCode("Members from deserialized types may be trimmed if not referenced directly")]
    public void AddXmlSerializerFormatter()
    {
        this.Add(new WebApiXmlSerializerFormatter());
    }

    /// <summary>
    /// 添加System.Text.Json序列化器
    /// </summary>
    /// <param name="options">配置JsonSerializerOptions的操作</param>
    public void AddSystemTextJsonSerializerFormatter(Action<System.Text.Json.JsonSerializerOptions> options)
    {
        var jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions();
        jsonSerializerOptions.TypeInfoResolverChain.Add(WebApiSystemTextJsonSerializerContext.Default);
        options.Invoke(jsonSerializerOptions);

        this.Add(new WebApiSystemTextJsonSerializerFormatter(jsonSerializerOptions));
    }
}