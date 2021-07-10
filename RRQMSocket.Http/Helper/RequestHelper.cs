using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.Http
{
    /// <summary>
    /// 请求辅助类
    /// </summary>
    public static class RequestHelper
    {
        /// <summary>
        /// 从Xml格式
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="xmlText"></param>
        /// <returns></returns>
        public static HttpRequest FromXML(this HttpRequest  httpRequest, string xmlText)
        {
            httpRequest.SetContent(xmlText);
            httpRequest.Content_Type = "text/xml";
            return httpRequest;
        }

        /// <summary>
        /// 从Json
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        public static HttpRequest FromJson(this HttpRequest httpRequest, string jsonText)
        {
            httpRequest.SetContent(jsonText);
            httpRequest.Content_Type = "text/json";
            return httpRequest;
        }

        /// <summary>
        /// 从文本
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static HttpRequest FromText(this HttpRequest httpRequest, string text)
        {
            httpRequest.SetContent(text);
            httpRequest.Content_Type = "text/plain";
            return httpRequest;
        }
    }
}
