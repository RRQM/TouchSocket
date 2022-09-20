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
using System.Linq;
using System.Text;
using TouchSocket.Sockets;

namespace XUnitTest.DataAdapter
{
    internal class MyCustomBetweenAndDataHandlingAdapter : CustomBetweenAndDataHandlingAdapter<MyBetweenAndRequestInfo>
    {
        public MyCustomBetweenAndDataHandlingAdapter(byte[] startCode, byte[] endCode)
        {
            this.StartCode = startCode ?? throw new ArgumentNullException(nameof(startCode));
            this.EndCode = endCode ?? throw new ArgumentNullException(nameof(endCode));
        }

        public MyCustomBetweenAndDataHandlingAdapter(string startStr, string endStr, Encoding encoding)
        {
            this.StartCode = string.IsNullOrEmpty(startStr) ? new byte[0] : encoding.GetBytes(startStr);
            this.EndCode = encoding.GetBytes(endStr) ?? throw new ArgumentNullException(nameof(endStr));
        }

        public override byte[] StartCode { get; }

        public override byte[] EndCode { get; }

        public override bool CanSendRequestInfo => false;

        protected override MyBetweenAndRequestInfo GetInstance()
        {
            return new MyBetweenAndRequestInfo(this.StartCode, this.EndCode);
        }

        protected override void PreviewSend(IRequestInfo requestInfo, bool isAsync)
        {
            
        }
    }

    internal class MyBetweenAndRequestInfo : IBetweenAndRequestInfo
    {
        private byte[] m_startCode;

        private byte[] m_endCode;

        private byte[] m_data;

        public MyBetweenAndRequestInfo(byte[] startCode, byte[] endCode)
        {
            this.m_startCode = startCode;
            this.m_endCode = endCode;
        }

        public byte[] Data
        {
            get { return m_data; }
        }

        void IBetweenAndRequestInfo.OnParsingBody(byte[] body)
        {
            this.m_data = body;
        }

        bool IBetweenAndRequestInfo.OnParsingEndCode(byte[] endCode)
        {
            if (endCode.SequenceEqual(this.m_endCode))
            {
                return true;
            }
            return false;
        }

        bool IBetweenAndRequestInfo.OnParsingStartCode(byte[] startCode)
        {
            if (startCode.SequenceEqual(this.m_startCode))
            {
                return true;
            }
            return false;
        }
    }
}