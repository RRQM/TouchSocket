using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace RRQMSocket.Http
{
    /// <summary>
    /// 响应扩展
    /// </summary>
    public static class ResponseHelper
    {
        /// <summary>
        /// 从文件
        /// </summary>
        /// <param name="response"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static HttpResponse FromFile(this HttpResponse response, string fileName)
        {
            if (!File.Exists(fileName))
            {
                response.SetContent("<html><body><h1>404 -RRQM Not Found</h1></body></html>");
                response.StatusCode = "404";
                response.Content_Type = "text/html";
                return response;
            }

            var content = File.ReadAllBytes(fileName);
            response.SetContent(content);
            response.StatusCode = "200";
            return response;
        }

        /// <summary>
        /// 从Xml格式
        /// </summary>
        /// <param name="response"></param>
        /// <param name="xmlText"></param>
        /// <returns></returns>
        public static HttpResponse FromXML(this HttpResponse response, string xmlText)
        {
            response.SetContent(xmlText);
            response.Content_Type = "text/xml";
            response.StatusCode = "200";
            return response;
        }

        /// <summary>
        /// 从Json
        /// </summary>
        /// <param name="response"></param>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        public static HttpResponse FromJson(this HttpResponse response, string jsonText)
        {
            response.SetContent(jsonText);
            response.Content_Type = "text/json";
            response.StatusCode = "200";
            return response;
        }

        /// <summary>
        /// 从文本
        /// </summary>
        /// <param name="response"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static HttpResponse FromText(this HttpResponse response, string text)
        {
            response.SetContent(text);
            response.Content_Type = "text/plain";
            response.StatusCode = "200";
            return response;
        }

      
    }
}
