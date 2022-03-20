//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
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
using RRQMCore;
using RRQMCore.ByteManager;
using RRQMCore.Extensions;
using System.Collections.Generic;
using System.Text;

namespace RRQMSocket.Http
{
    /// <summary>
    /// Http客户端数据处理适配器
    /// </summary>
    public class HttpClientDataHandlingAdapter : CustomDataHandlingAdapter<HttpResponse>
    {
        private bool Chunked;
        private static byte[] rnCode = Encoding.UTF8.GetBytes("\r\n");

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxSize"></param>
        public HttpClientDataHandlingAdapter(int maxSize)
        {
            this.MaxPackageSize = maxSize;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSplicingSend => false;


        private FilterResult ReadChunk(ByteBlock byteBlock, int length, HttpResponse response)
        {
            int position = byteBlock.Pos;
            string ss = byteBlock.ToString();
            int index = byteBlock.Buffer.IndexOfFirst(byteBlock.Pos, length, rnCode);
            if (index > 0)
            {
                int headerLength = index - byteBlock.Pos;
                string hex = Encoding.ASCII.GetString(byteBlock.Buffer, byteBlock.Pos, headerLength - 1);
                int count = hex.ByHexStringToInt32();
                byteBlock.Pos += headerLength+1;

                if (count > 0)
                {
                    if (count > byteBlock.CanReadLen)
                    {
                        byteBlock.Pos = position;
                        return FilterResult.Cache;
                    }
                    response.Chunks.Add(byteBlock.ToArray(byteBlock.Pos, count));
                    byteBlock.Pos += count;
                    byteBlock.Pos += 2;
                    return FilterResult.GoOn;
                }
                else
                {
                    byteBlock.Pos += 2;
                    this.Chunked = false;
                    return FilterResult.Success;
                }
            }
            else
            {
                return FilterResult.Cache;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="length"></param>
        /// <param name="beCached"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        protected override FilterResult Filter(ByteBlock byteBlock, int length, bool beCached, ref HttpResponse response)
        {
            int position = byteBlock.Pos;

            if (this.Chunked)
            {
               return ReadChunk(byteBlock,length,response);
            }

            HttpResponse httpResponse = new HttpResponse();

            var result = httpResponse.ParsingHeader(byteBlock, length);
            
            if (result == FilterResult.Success)
            {
                if (httpResponse.ContentLength > byteBlock.CanReadLen)
                {
                    byteBlock.Pos = position;
                    return FilterResult.Cache;
                }
                byteBlock.Pos++;
                if (httpResponse.ParsingContent(byteBlock.ToArray(byteBlock.Pos, httpResponse.ContentLength)))
                {
                    response = httpResponse;
                    if (httpResponse.HasChunk)
                    {
                        this.Chunked = true;
                        httpResponse.Chunks = new List<byte[]>();
                        return ReadChunk(byteBlock, length, response);
                    }
                    else
                    {
                        byteBlock.Pos += httpResponse.ContentLength;
                        return FilterResult.Success;
                    }
                }
                else
                {
                    byteBlock.Pos += httpResponse.ContentLength;
                    return FilterResult.GoOn;
                }
            }
            else
            {
                return result;
            }
        }
    }
}