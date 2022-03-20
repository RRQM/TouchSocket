using RRQMCore;
using RRQMCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.Http
{
    /// <summary>
    /// HttpResponse扩展
    /// </summary>
    public static class HttpResponseExtensions
    {
        /// <summary>
        /// 路径文件没找到
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static HttpResponse FileNotFind(this HttpResponse response)
        {
            response.SetContent("<html><body><h1>404 -RRQM Not Found</h1></body></html>");
            response.StatusCode = "404";
            response.ContentType = "text/html;charset=utf-8";
            return response;
        }

        /// <summary>
        /// 从扩展名设置内容类型，不包含“.”
        /// </summary>
        /// <param name="response"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static HttpResponse SetContentTypeByExtension(this HttpResponse response, string extension)
        {
            response.SetHeader(HttpHeaders.ContentType, GetContentTypeFromExtension(extension));
            return response;
        }

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
            response.ContentType = "text/xml;charset=utf-8";
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
            response.ContentType = "text/json;charset=utf-8";
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
            response.ContentType = "text/plain;charset=utf-8";

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
            response.SetHeader(HttpHeaders.Server, $"RRQMSocket.Http {HttpResponse.ServerVersion}");
            response.SetHeader(HttpHeaders.Date, DateTime.Now.ToGMTString("r"));
            return response;
        }


        /// <summary>
        /// 从扩展名获取ContentType
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetContentTypeFromExtension(string extension)
        {
            switch (extension)
            {
                case "html":
                    return "text/html";
                case "css":
                    return "text/css";
                case "js":
                    return "text/javascript";
                case "xml":
                    return "text/xml";
                case "gzip":
                    return "application/gzip";
                case "json":
                    return "application/json";
                case "map":
                    return "application/json";
                case "pdf":
                    return "application/pdf";
                case "zip":
                    return "application/zip";
                case "mp3":
                    return "audio/mpeg";
                case "jpg":
                    return "image/jpeg";
                case "gif":
                    return "image/gif";
                case "png":
                    return "image/png";
                case "svg":
                    return "image/svg+xml";
                case "mp4":
                    return "video/mp4";
                case "atom":
                    return "application/atom+xml";
                case "fastsoap":
                    return "application/fastsoap";
                case "ps":
                    return "application/postscript";
                case "soap":
                    return "application/soap+xml";
                case "sql":
                    return "application/sql";
                case "xslt":
                    return "application/xslt+xml";
                case "zlib":
                    return "application/zlib";
                case "aac":
                    return "audio/aac";
                case "ac3":
                    return "audio/ac3";
                case "ogg":
                    return "audio/ogg";
                case "ttf":
                    return "font/ttf";
                case "bmp":
                    return "image/bmp";
                case "jpm":
                    return "image/jpm";
                case "jpx":
                    return "image/jpx";
                case "jrx":
                    return "image/jrx";
                case "tiff":
                    return "image/tiff";
                case "emf":
                    return "image/emf";
                case "wmf":
                    return "image/wmf";
                case "http":
                    return "message/http";
                case "s-http":
                    return "message/s-http";
                case "mesh":
                    return "model/mesh";
                case "vrml":
                    return "model/vrml";
                case "csv":
                    return "text/csv";
                case "plain":
                    return "text/plain";
                case "richtext":
                    return "text/richtext";
                case "rtf":
                    return "text/rtf";
                case "rtx":
                    return "text/rtx";
                case "sgml":
                    return "text/sgml";
                case "strings":
                    return "text/strings";
                case "url":
                    return "text/uri-list";
                case "H264":
                    return "video/H264";
                case "H265":
                    return "video/H265";
                case "mpeg":
                    return "video/mpeg";
                case "raw":
                    return "video/raw";
                default:
                    return string.Empty;
            }
        }
    }
}
