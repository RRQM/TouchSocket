using TouchSocket.Core;
using TouchSocket.Sockets;

namespace AdapterConsoleApp
{
    /// <summary>
    /// 用户自定义适配器
    /// </summary>
    internal class MyCustomDataHandlingAdapter : CustomDataHandlingAdapter<MyRequestInfo>
    {
        public override bool CanSendRequestInfo => false;

        /// <summary>
        /// 筛选解析数据。实例化的TRequest会一直保存，直至解析成功，或手动清除。
        /// <para>当不满足解析条件时，请返回<see cref="FilterResult.Cache"/>，此时会保存<see cref="ByteBlock.CanReadLen"/>的数据</para>
        /// <para>当数据部分异常时，请移动<see cref="ByteBlock.Pos"/>到指定位置，然后返回<see cref="FilterResult.GoOn"/></para>
        /// <para>当完全满足解析条件时，请返回<see cref="FilterResult.Success"/>最后将<see cref="ByteBlock.Pos"/>移至指定位置。</para>
        /// </summary>
        /// <param name="byteBlock">字节块</param>
        /// <param name="beCached">是否为上次遗留对象，当该参数为True时，request也将是上次实例化的对象。</param>
        /// <param name="request">对象。</param>
        /// <returns></returns>
        protected override FilterResult Filter(in ByteBlock byteBlock, bool beCached, ref MyRequestInfo request, ref int tempCapacity)
        {
            //以下解析思路为一次性解析，不考虑缓存的临时对象。

            if (byteBlock.CanReadLen < 3)
            {
                return FilterResult.Cache;//当头部都无法解析时，直接缓存
            }

            int pos = byteBlock.Pos;//记录初始游标位置，防止本次无法解析时，回退游标。

            MyRequestInfo myRequestInfo = new MyRequestInfo();

            byteBlock.Read(out byte[] header, 3);//填充header

            //因为第一个字节表示所有长度，而DataType、OrderType已经包含在了header里面。
            //所有只需呀再读取header[0]-2个长度即可。
            byte bodyLength = (byte)(header[0] - 2);

            if (bodyLength > byteBlock.CanReadLen)
            {
                //body数据不足。
                byteBlock.Pos = pos;//回退游标
                return FilterResult.Cache;
            }
            else
            {
                byteBlock.Read(out byte[] body, bodyLength);//填充body

                myRequestInfo.Header = header;
                myRequestInfo.DataType = header[1];
                myRequestInfo.OrderType = header[2];
                myRequestInfo.Body = body;
                request = myRequestInfo;//赋值ref
                return FilterResult.Success;//返回成功
            }
        }

        protected override void PreviewSend(IRequestInfo requestInfo)
        {
            throw new System.NotImplementedException();
        }
    }

    internal class MyRequestInfo : IRequestInfo
    {
        /// <summary>
        /// 自定义属性,Body
        /// </summary>
        public byte[] Body { get; internal set; }

        /// <summary>
        /// 自定义属性,Header
        /// </summary>
        public byte[] Header { get; internal set; }

        /// <summary>
        /// 自定义属性,DataType
        /// </summary>
        public byte DataType { get; internal set; }

        /// <summary>
        /// 自定义属性,OrderType
        /// </summary>
        public byte OrderType { get; internal set; }
    }
}