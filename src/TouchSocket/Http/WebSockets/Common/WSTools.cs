//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在TouchSocket.Core.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

/* 项目“TouchSocketPro (net5)”的未合并的更改
在此之前:
using TouchSocket.Core;
在此之后:
using System;
using System.Text;
using TouchSocket.Core;
*/

/* 项目“TouchSocketPro (netcoreapp3.1)”的未合并的更改
在此之前:
using TouchSocket.Core;
在此之后:
using System;
using System.Text;
using TouchSocket.Core;
*/

/* 项目“TouchSocketPro (netstandard2.0)”的未合并的更改
在此之前:
using TouchSocket.Core;
在此之后:
using System;
using System.Text;
using TouchSocket.Core;
*/

using System;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;

/* 项目“TouchSocketPro (net5)”的未合并的更改
在此之前:
using TouchSocket.Http;
using System;
using System.Text;
在此之后:
using TouchSocket.Http;
*/

/* 项目“TouchSocketPro (netcoreapp3.1)”的未合并的更改
在此之前:
using TouchSocket.Http;
using System;
using System.Text;
在此之后:
using TouchSocket.Http;
*/

/* 项目“TouchSocketPro (netstandard2.0)”的未合并的更改
在此之前:
using TouchSocket.Http;
using System;
using System.Text;
在此之后:
using TouchSocket.Http;
*/

using TouchSocket.Core.Extensions;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// WSTools
    /// </summary>
    public static class WSTools
    {
        /// <summary>
        /// 应答。
        /// </summary>
        public const string acceptMask = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        /// <summary>
        /// 获取Base64随即字符串。
        /// </summary>
        /// <returns></returns>
        public static string CreateBase64Key()
        {
            var src = new byte[16];
            new Random().NextBytes(src);
            return Convert.ToBase64String(src);
        }

        /// <summary>
        /// 获取WS的请求头
        /// </summary>
        /// <param name="host"></param>
        /// <param name="url"></param>
        /// <param name="version"></param>
        /// <param name="base64Key"></param>
        /// <returns></returns>
        public static HttpRequest GetWSRequest(string host, string url, string version, out string base64Key)
        {
            HttpRequest request = new HttpRequest
            {
                Method = "GET"
            };
            request.SetUrl(url);
            request.Protocols = "HTTP";
            request.ProtocolVersion = "1.1";
            request.SetHeader(HttpHeaders.Host, host);
            request.SetHeader(HttpHeaders.Connection, "Upgrade");
            request.SetHeader(HttpHeaders.Pragma, "no-cache");
            request.SetHeader(HttpHeaders.UserAgent, "TouchSocket.Http.WebSockets");
            request.SetHeader(HttpHeaders.Upgrade, "websocket");
            request.SetHeader(HttpHeaders.Origin, "RRQM");
            request.SetHeader("sec-websocket-version", $"{version}");
            request.SetHeader(HttpHeaders.AcceptEncoding, "deflate, br");
            base64Key = CreateBase64Key();
            request.SetHeader("sec-websocket-key", $"{base64Key}");

            return request;
        }

        /// <summary>
        /// 计算Base64值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string CalculateBase64Key(string str, Encoding encoding)
        {
            return (str + acceptMask).ToSha1(encoding).ToBase64();
        }

        /// <summary>
        /// 获取响应
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static bool TryGetResponse(HttpRequest request, HttpResponse response)
        {
            string upgrade = request.GetHeader(HttpHeaders.Upgrade);
            if (string.IsNullOrEmpty(upgrade))
            {
                return false;
            }
            string connection = request.GetHeader(HttpHeaders.Connection);
            if (string.IsNullOrEmpty(connection))
            {
                return false;
            }
            string secWebSocketKey = request.GetHeader("sec-websocket-key");
            if (string.IsNullOrEmpty(secWebSocketKey))
            {
                return false;
            }

            response.StatusCode = "101";
            response.StatusMessage = "switching protocols";
            response.SetHeader(HttpHeaders.Connection, "upgrade");
            response.SetHeader(HttpHeaders.Upgrade, "websocket");
            response.SetHeader("sec-websocket-accept", CalculateBase64Key(secWebSocketKey, request.Encoding));
            return true;
        }

        /// <summary>
        /// 掩码运算
        /// </summary>
        /// <param name="storeBuf"></param>
        /// <param name="sOffset"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="masks"></param>
        public static void DoMask(byte[] storeBuf, int sOffset, byte[] buffer, int offset, int length, byte[] masks)
        {
            for (var i = 0; i < length; i++)
            {
                storeBuf[sOffset + i] = (byte)(buffer[offset + i] ^ masks[i % 4]);
            }
        }

        /// <summary>
        /// 构建数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="dataFrame"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool Build(ByteBlock byteBlock, WSDataFrame dataFrame, byte[] buffer, int offset, int length)
        {
            int payloadLength;

            byte[] extLen;

            if (length < 126)
            {
                payloadLength = length;
                extLen = new byte[0];
            }
            else if (length < 65536)
            {
                payloadLength = 126;
                extLen = TouchSocketBitConverter.BigEndian.GetBytes((ushort)length);
            }
            else
            {
                payloadLength = 127;
                extLen = TouchSocketBitConverter.BigEndian.GetBytes((ulong)length);
            }

            int header = dataFrame.FIN ? 1 : 0;
            header = (header << 1) + (dataFrame.RSV1 ? 1 : 0);
            header = (header << 1) + (dataFrame.RSV2 ? 1 : 0);
            header = (header << 1) + (dataFrame.RSV3 ? 1 : 0);
            header = (header << 4) + (ushort)dataFrame.Opcode;

            if (dataFrame.Mask)
            {
                header = (header << 1) + 1;
            }
            else
            {
                header = (header << 1) + 0;
            }

            header = (header << 7) + payloadLength;

            byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes((ushort)header));

            if (payloadLength > 125)
            {
                byteBlock.Write(extLen, 0, extLen.Length);
            }

            if (dataFrame.Mask)
            {
                byteBlock.Write(dataFrame.MaskingKey, 0, 4);
            }

            if (payloadLength > 0)
            {
                if (dataFrame.Mask)
                {
                    if (byteBlock.Capacity < byteBlock.Pos + length)
                    {
                        byteBlock.SetCapacity(byteBlock.Pos + length, true);
                    }
                    WSTools.DoMask(byteBlock.Buffer, byteBlock.Pos, buffer, offset, length, dataFrame.MaskingKey);
                    byteBlock.SetLength(byteBlock.Pos + length);
                }
                else
                {
                    byteBlock.Write(buffer, offset, length);
                }
            }
            return true;
        }
    }
}