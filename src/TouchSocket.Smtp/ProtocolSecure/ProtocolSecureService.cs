using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Smtp
{
    internal class ProtocolSecureService: IProtocolSecureService
    {
        readonly Dictionary<ushort, string> m_dic=new Dictionary<ushort, string>();
        public void RegisterProtocol(ushort protocol,string description)
        { 
        
        }
    }
}
