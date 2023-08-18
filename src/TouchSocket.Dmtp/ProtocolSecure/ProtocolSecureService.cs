using System.Collections.Generic;

namespace TouchSocket.Dmtp
{
    internal class ProtocolSecureService : IProtocolSecureService
    {
        readonly Dictionary<ushort, string> m_dic = new Dictionary<ushort, string>();
        public void RegisterProtocol(ushort protocol, string description)
        {

        }
    }
}
