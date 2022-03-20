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
using RRQMCore.ByteManager;

namespace RRQMSocket.Http
{
    /// <summary>
    /// Http服务器数据处理适配器
    /// </summary>
    public class HttpServerDataHandlingAdapter : CustomDataHandlingAdapter<HttpRequest>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxSize"></param>
        public HttpServerDataHandlingAdapter(int maxSize)
        {
            this.MaxPackageSize = maxSize;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSplicingSend => false;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"><inheritdoc/></param>
        /// <param name="length"><inheritdoc/></param>
        /// <param name="beCached"><inheritdoc/></param>
        /// <param name="request"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override FilterResult Filter(ByteBlock byteBlock, int length, bool beCached, ref HttpRequest request)
        {
            int position = byteBlock.Pos;
            request = new HttpRequest();

            var result = request.ParsingHeader(byteBlock, length);
            
            if (result == FilterResult.Success)
            {
                if (request.ContentLength > byteBlock.CanReadLen)
                {
                    byteBlock.Pos = position;
                    return FilterResult.Cache;
                }
                byteBlock.Pos++;
                if (request.ParsingContent(byteBlock.ToArray(byteBlock.Pos, request.ContentLength)))
                {
                    byteBlock.Pos += request.ContentLength;
                    return FilterResult.Success;
                }
                else
                {
                    byteBlock.Pos += request.ContentLength;
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