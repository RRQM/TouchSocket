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

using System;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Sockets;

namespace XUnitTest.DataAdapter
{
    internal class MyCustomBigFixedHeaderDataHandlingAdapter : CustomBigFixedHeaderDataHandlingAdapter<MyBigFixedHeaderRequestInfo>
    {
        public override int HeaderLength => 4;

        public override bool CanSendRequestInfo => false;

        protected override MyBigFixedHeaderRequestInfo GetInstance()
        {
            return new MyBigFixedHeaderRequestInfo();
        }

        protected override void PreviewSend(IRequestInfo requestInfo, bool isAsync)
        {
            throw new NotImplementedException();
        }
    }

    internal class MyBigFixedHeaderRequestInfo : IBigFixedHeaderRequestInfo
    {
        private long bodyLength;
        public long BodyLength => bodyLength;

        private ByteBlock m_storeByteBlock;

        private byte[] m_data;

        public byte[] Data
        {
            get { return m_data; }
        }

        public void OnAppendBody(byte[] buffer, int offset, int length)
        {
            m_storeByteBlock.Write(buffer, offset, length);
        }

        public bool OnFinished()
        {
            if (this.m_storeByteBlock.Len == this.bodyLength)
            {
                this.m_data = this.m_storeByteBlock.ToArray();
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
                this.bodyLength = TouchSocketBitConverter.Default.ToInt32(header, 0);
                this.m_storeByteBlock = new ByteBlock((int)this.bodyLength);
                return true;
            }
            return false;
        }
    }
}