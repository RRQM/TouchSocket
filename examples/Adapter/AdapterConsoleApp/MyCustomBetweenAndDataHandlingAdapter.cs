using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace AdapterConsoleApp
{
    internal class MyCustomBetweenAndDataHandlingAdapter : CustomBetweenAndDataHandlingAdapter<MyBetweenAndRequestInfo>
    {
        public MyCustomBetweenAndDataHandlingAdapter()
        {
            this.MinSize = 5;//表示，实际数据体不会小于5，例如“**12##12##”数据，解析后会解析成“12##12”
        }

        public override byte[] StartCode => Encoding.UTF8.GetBytes("**");//可以为0长度字节，意味着没有起始标识。

        public override byte[] EndCode => Encoding.UTF8.GetBytes("##");//必须为有效值。

        public override bool CanSendRequestInfo => false;

        protected override MyBetweenAndRequestInfo GetInstance()
        {
            return new MyBetweenAndRequestInfo();
        }

        protected override void PreviewSend(IRequestInfo requestInfo)
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// 以**12##12##，Min=5为例。
    /// </summary>
    internal class MyBetweenAndRequestInfo : IBetweenAndRequestInfo
    {
        public byte[] Body { get; private set; }

        public void OnParsingBody(byte[] body)
        {
            this.Body = body;
            //这里的Body应该为12##12
        }

        public bool OnParsingEndCode(byte[] endCode)
        {
            return true;//该返回值决定，是否执行Receive
        }

        public bool OnParsingStartCode(byte[] startCode)
        {
            return true;
        }
    }
}