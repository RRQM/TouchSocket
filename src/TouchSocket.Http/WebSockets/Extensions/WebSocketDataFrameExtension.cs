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

using System;
using System.Text;
using TouchSocket.Core;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// WSDataFrame辅助扩展类
    /// </summary>
    public static class WebSocketDataFrameExtension
    {
        /// <summary>
        /// 追加二进制流
        /// </summary>
        /// <param name="dataFrame"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static WSDataFrame AppendBinary(this WSDataFrame dataFrame, ReadOnlySpan<byte> span)
        {
            dataFrame.PayloadData ??= new ByteBlock(span.Length);
            dataFrame.PayloadData.Write(span);
            return dataFrame;
        }

        /// <summary>
        /// 追加文本
        /// </summary>
        /// <param name="dataFrame"></param>
        /// <param name="text"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static WSDataFrame AppendText(this WSDataFrame dataFrame, string text, Encoding encoding = default)
        {
            var data = (encoding == default ? Encoding.UTF8 : encoding).GetBytes(text);
            dataFrame.PayloadData ??= new ByteBlock(data.Length);
            dataFrame.PayloadData.Write(data);
            return dataFrame;
        }

        /// <summary>
        /// 构建请求数据（含Make）
        /// </summary>
        /// <param name="dataFrame"></param>
        /// <param name="byteBlock"></param>
        /// <returns></returns>
        public static void BuildRequest<TByteBlock>(this WSDataFrame dataFrame, ref TByteBlock byteBlock) where TByteBlock : IByteBlock
        {
            dataFrame.Mask = true;
            if (dataFrame.MaskingKey == null)
            {
                dataFrame.SetMaskString("RRQM");
            }
            dataFrame.Build(ref byteBlock);
        }

        /// <summary>
        /// 构建请求数据（含Make）
        /// </summary>
        /// <param name="dataFrame"></param>
        /// <returns></returns>
        public static byte[] BuildRequestToBytes(this WSDataFrame dataFrame)
        {
            dataFrame.Mask = true;
            if (dataFrame.MaskingKey == null)
            {
                dataFrame.SetMaskString("RRQM");
            }
            var byteBlock = new ValueByteBlock(dataFrame.MaxLength);
            try
            {
                dataFrame.Build(ref byteBlock);
                var data = byteBlock.ToArray();
                return data;
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 构建响应数据（无Make）
        /// </summary>
        /// <param name="dataFrame"></param>
        /// <param name="byteBlock"></param>
        /// <returns></returns>
        public static void BuildResponse<TByteBlock>(this WSDataFrame dataFrame, ref TByteBlock byteBlock) where TByteBlock : IByteBlock
        {
            dataFrame.Build(ref byteBlock);
        }

        /// <summary>
        /// 构建响应数据（无Make）
        /// </summary>
        /// <param name="dataFrame"></param>
        /// <returns></returns>
        public static byte[] BuildResponseToBytes(this WSDataFrame dataFrame)
        {
            var byteBlock = new ValueByteBlock(dataFrame.MaxLength);
            try
            {
                dataFrame.Build(ref byteBlock);
                var data = byteBlock.ToArray();
                return data;
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 当<see cref="WSDataType.Text"/>时，转换为Text消息。
        /// </summary>
        /// <param name="dataFrame"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ToText(this WSDataFrame dataFrame, Encoding encoding = default)
        {
            return dataFrame.PayloadData.Span.ToString(encoding == default ? Encoding.UTF8 : encoding);
        }
    }
}