//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Helper;
using System;

namespace RRQMSocket.Http
{
    /// <summary>
    /// 响应扩展
    /// </summary>
    public static class ResponseHelper
    {
        ///// <summary>
        ///// 从文件
        ///// </summary>
        ///// <param name="response"></param>
        ///// <param name="fileName"></param>
        ///// <returns></returns>
        //public static HttpResponse FromFile(this HttpResponse response, string fileName)
        //{
        //    if (!File.Exists(fileName))
        //    {
        //        response.SetContent("<html><body><h1>404 -RRQM Not Found</h1></body></html>");
        //        response.StatusCode = "404";
        //        response.Content_Type = "text/html";
        //        return response;
        //    }

        //    var content = File.ReadAllBytes(fileName);
        //    response.SetContent(content);
        //    return response.FromSuccess();
        //}

        /// <summary>
        /// 从Xml格式
        /// </summary>
        /// <param name="response"></param>
        /// <param name="xmlText"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static HttpResponse FromXML(this HttpResponse response, string xmlText, string statusCode = "200")
        {
            response.SetContent(xmlText);
            response.Content_Type = "text/xml";
            return response.FromSuccess(statusCode);
        }

        /// <summary>
        /// 从Json
        /// </summary>
        /// <param name="response"></param>
        /// <param name="jsonText"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static HttpResponse FromJson(this HttpResponse response, string jsonText, string statusCode = "200")
        {
            response.SetContent(jsonText);
            response.Content_Type = "text/json";
            return response.FromSuccess(statusCode);
        }

        /// <summary>
        /// 从文本
        /// </summary>
        /// <param name="response"></param>
        /// <param name="statusCode"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static HttpResponse FromText(this HttpResponse response, string text, string statusCode = "200")
        {
            response.SetContent(text);
            response.Content_Type = "text/plain";

            return response.FromSuccess(statusCode);
        }

        /// <summary>
        /// 返回成功
        /// </summary>
        /// <param name="response"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static HttpResponse FromSuccess(this HttpResponse response, string statusCode = "200")
        {
            response.StatusCode = statusCode;
            response.SetHeader(ResponseHeader.Server, $"RRQMSocket.Http {HttpResponse.ServerVersion}");
            response.SetHeader(ResponseHeader.Date, DateTime.Now.ToGMTString("r"));
            return response;
        }
    }
}