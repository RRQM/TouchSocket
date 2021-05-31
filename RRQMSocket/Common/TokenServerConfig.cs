using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.Dependency;

namespace RRQMSocket
{
    /// <summary>
    /// TokenTcp服务配置
    /// </summary>
    public class TokenServerConfig : TcpServerConfig
    {
        /// <summary>
        /// 连接令箭,当为null或空时，重置为默认值“rrqm”
        /// </summary>
        public string VerifyToken
        {
            get { return (string)GetValue(VerifyTokenProperty); }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = "rrqm";
                }
                SetValue(VerifyTokenProperty, value);
            }
        }

        /// <summary>
        /// 连接令箭,当为null或空时，重置为默认值“rrqm”
        /// </summary>
        public static readonly DependencyProperty VerifyTokenProperty =
            DependencyProperty.Register("VerifyToken", typeof(string), typeof(TokenServerConfig), "rrqm");


        /// <summary>
        /// 验证超时时间,默认为3秒；
        /// </summary>
        public int VerifyTimeout
        {
            get { return (int)GetValue(VerifyTimeoutProperty); }
            set
            {
                if (value < 1)
                {
                    value = 1;
                }
                SetValue(VerifyTimeoutProperty, value);
            }
        }

        /// <summary>
        /// 验证超时时间,默认为3秒；
        /// </summary>
        public static readonly DependencyProperty VerifyTimeoutProperty =
            DependencyProperty.Register("VerifyTimeout", typeof(int), typeof(TokenServerConfig), 3);
    }
}
