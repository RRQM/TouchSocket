//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
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
