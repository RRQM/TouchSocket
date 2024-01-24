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

using TouchSocket.Core;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// |MBAP|功能码|数据|
    ///
    /// MBAP=|事务处理标识符(2)|协议标识符(2)|长度(2)|单元标识符(1)|
    /// 长度表示后续所有。
    ///
    /// 此处解析思路是，将数据之前的所有信息，均认为固定包头。则包头长度是8
    /// </summary>
    internal class ModbusTcpAdapterForPoll : CustomFixedHeaderDataHandlingAdapter<ModbusTcpResponse>
    {
        public override int HeaderLength => 8;

        public override bool CanSendRequestInfo => true;

        protected override ModbusTcpResponse GetInstance()
        {
            return new ModbusTcpResponse();
        }
    }
}