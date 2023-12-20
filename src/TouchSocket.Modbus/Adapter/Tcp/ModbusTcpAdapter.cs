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
    internal class ModbusTcpAdapter : CustomFixedHeaderDataHandlingAdapter<ModbusTcpResponse>
    {
        public override int HeaderLength => 8;

        public override bool CanSendRequestInfo => true;

        protected override ModbusTcpResponse GetInstance()
        {
            return new ModbusTcpResponse();
        }
    }
}
