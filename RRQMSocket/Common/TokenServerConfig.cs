using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// TokenTcp服务配置
    /// </summary>
    public class TokenServerConfig : TcpServerConfig
    {
        private string verifyToken = "rrqm";

        /// <summary>
        /// 连接令箭,当为null或空时，重置为默认值“rrqm”
        /// </summary>
        public string VerifyToken
        {
            get { return verifyToken; }
            set
            {
                if (value == null || value == string.Empty)
                {
                    value = "rrqm";
                }
                verifyToken = value;
            }
        }

       
        private int verifyTimeout=3;
        /// <summary>
        /// 验证超时时间,默认为3秒；
        /// </summary>
        public int VerifyTimeout
        {
            get { return verifyTimeout; }
            set 
            {
                if (value<1)
                {
                    value = 1;
                }
                verifyTimeout = value; 
            }
        }

    }
}
