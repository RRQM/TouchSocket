using TouchSocket.Core;
using TouchSocket.Sockets;

namespace AdapterConsoleApp
{
    /// <summary>
    /// 模板解析“固定包头”数据适配器
    /// </summary>
    public class MyFixedHeaderCustomDataHandlingAdapter : CustomFixedHeaderDataHandlingAdapter<MyFixedHeaderRequestInfo>
    {
        public MyFixedHeaderCustomDataHandlingAdapter()
        {
            this.MaxPackageSize = 1024;
        }

        /// <summary>
        /// 接口实现，指示固定包头长度
        /// </summary>
        public override int HeaderLength => 3;

        public override bool CanSendRequestInfo => false;

        /// <summary>
        /// 获取新实例
        /// </summary>
        /// <returns></returns>
        protected override MyFixedHeaderRequestInfo GetInstance()
        {
            return new MyFixedHeaderRequestInfo();
        }

        protected override void PreviewSend(IRequestInfo requestInfo)
        {
            throw new System.NotImplementedException();
        }
    }

    public class MyFixedHeaderRequestInfo : IFixedHeaderRequestInfo
    {
        /// <summary>
        /// 接口实现，标识数据长度
        /// </summary>
        public int BodyLength { get; private set; }

        /// <summary>
        /// 自定义属性，标识数据类型
        /// </summary>
        public byte DataType { get; private set; }

        /// <summary>
        /// 自定义属性，标识指令类型
        /// </summary>
        public byte OrderType { get; private set; }

        /// <summary>
        /// 自定义属性，标识实际数据
        /// </summary>
        public byte[] Body { get; private set; }

        public bool OnParsingBody(byte[] body)
        {
            if (body.Length == this.BodyLength)
            {
                this.Body = body;
                return true;
            }
            return false;
        }

        public bool OnParsingHeader(byte[] header)
        {
            //在该示例中，第一个字节表示后续的所有数据长度，但是header设置的是3，所以后续还应当接收length-2个长度。
            this.BodyLength = header[0] - 2;
            this.DataType = header[1];
            this.OrderType = header[2];
            return true;
        }
    }
}