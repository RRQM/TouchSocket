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

using Newtonsoft.Json;
using TouchSocket.Core;
using TouchSocket.Http;
using System;

namespace TouchSocket.WebApi
{
    /// <summary>
    /// 适用于WebApi的序列化器
    /// </summary>
    public class WebApiSerializerConverter : TouchSocketSerializerConverter<string, HttpContext>
    {
        /// <inheritdoc/>
        public override string Serialize(HttpContext state, in object target)
        {
            var accept = state.Request.Accept;
            if (accept != null && accept.Equals("text/plain"))
            {
                if (target == null)
                {
                    return string.Empty;
                }
                if ((target.GetType().IsPrimitive || target.GetType() == typeof(string)))
                {
                    return target.ToString();
                }
            }
            return base.Serialize(state, target);
        }

        /// <summary>
        /// 添加Json序列化器
        /// </summary>
        /// <param name="settings"></param>
        public void AddJsonSerializerFormatter(JsonSerializerSettings settings)
        {
            this.Add(new WebApiJsonSerializerFormatter() { JsonSettings = settings });
        }

        /// <summary>
        /// 添加Xml序列化器
        /// </summary>
        public void AddXmlSerializerFormatter()
        {
            this.Add(new WebApiXmlSerializerFormatter());
        }

#if SystemTextJson
        public void AddSystemTextJsonSerializerFormatter(Action<System.Text.Json.JsonSerializerOptions> options)
        {
            var jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions();
            jsonSerializerOptions.TypeInfoResolverChain.Add(WebApiSystemTextJsonSerializerContext.Default);
            options.Invoke(jsonSerializerOptions);

            this.Add(new WebApiSystemTextJsonSerializerFormatter(jsonSerializerOptions));
        }
#endif
    }
}