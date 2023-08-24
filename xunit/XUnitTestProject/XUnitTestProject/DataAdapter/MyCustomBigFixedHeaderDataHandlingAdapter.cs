//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using TouchSocket.Core;

namespace XUnitTestProject.DataAdapter
{
    internal class MyCustomBigFixedHeaderDataHandlingAdapter : CustomBigFixedHeaderDataHandlingAdapter<MyBigFixedHeaderRequestInfo>
    {
        public override int HeaderLength => 4;

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

    internal class MyBigFixedHeaderRequestInfo : IBigFixedHeaderRequestInfo
    {
        public long BodyLength { get; private set; }

        private ByteBlock m_storeByteBlock;

        public byte[] Data { get; private set; }

        public void OnAppendBody(byte[] buffer)
        {
            this.m_storeByteBlock.Write(buffer);
        }

        public bool OnFinished()
        {
            if (this.m_storeByteBlock.Len == this.BodyLength)
            {
                this.Data = this.m_storeByteBlock.ToArray();
                this.m_storeByteBlock.SafeDispose();
                return true;
            }
            this.m_storeByteBlock.SafeDispose();
            return false;
        }

        public bool OnParsingHeader(byte[] header)
        {
            if (header.Length == 4)
            {
                this.BodyLength = TouchSocketBitConverter.Default.ToInt32(header, 0);
                this.m_storeByteBlock = new ByteBlock((int)this.BodyLength);
                return true;
            }
            return false;
        }
    }
}