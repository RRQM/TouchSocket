using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Http;

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
            if (accept!=null&& accept.Equals("text/plain")&&(target.GetType().IsPrimitive||target.GetType()==TouchSocketCoreUtility.stringType))
            {
                return target.ToString();
            }
            return base.Serialize(state, target);
        }

        /// <summary>
        /// 添加Json序列化器
        /// </summary>
        /// <param name="settings"></param>
        public void AddJsonSerializerFormatter(JsonSerializerSettings settings)
        {
            this.Add(new WebApiJsonSerializerFormatter() { JsonSettings=settings });
        }
        
        /// <summary>
        /// 添加Xml序列化器
        /// </summary>
        public void AddXmlSerializerFormatter()
        {
            this.Add(new WebApiXmlSerializerFormatter());
        }
    }
}
