using System;
using TouchSocket.Sockets;

namespace AdapterConsoleApp
{
    /// <summary>
    /// 模板解析“大数据固定包头”数据适配器
    /// </summary>
    internal class MyBigFixedHeaderCustomDataHandlingAdapter : CustomBigFixedHeaderDataHandlingAdapter<MyBigFixedHeaderRequestInfo>
    {
        public override int HeaderLength => throw new NotImplementedException();

        public override bool CanSendRequestInfo => false;

        protected override MyBigFixedHeaderRequestInfo GetInstance()
        {
            return new MyBigFixedHeaderRequestInfo();
        }

        protected override void PreviewSend(IRequestInfo requestInfo)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 下列吗，没有实现逻辑，仅解释思路。
    /// </summary>
    internal class MyBigFixedHeaderRequestInfo : IBigFixedHeaderRequestInfo
    {
        public long BodyLength => throw new NotImplementedException();

        public void OnAppendBody(byte[] buffer, int offset, int length)
        {
            //在这里会一直追加数据体，用户自行实现数据的保存。
        }

        public bool OnFinished()
        {
            //触发该方法时，说明数据体接收完毕，返回true时，会触发Receive相关事件，否则不会。
            return true;
        }

        public bool OnParsingHeader(byte[] header)
        {
            //解析头部
            return true;
        }
    }
}