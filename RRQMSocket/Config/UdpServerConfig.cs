using RRQMCore.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// UDP服务器配置
    /// </summary>
    public class UdpServerConfig : ServerConfig
    {

        /// <summary>
        /// 默认远程节点
        /// </summary>
        public EndPoint DefaultRemotePoint
        {
            get { return (EndPoint)GetValue(DefaultRemotePointProperty); }
            set { SetValue(DefaultRemotePointProperty, value); }
        }

        /// <summary>
        /// 默认远程节点
        /// </summary>
        public static readonly DependencyProperty DefaultRemotePointProperty =
            DependencyProperty.Register("DefaultRemotePoint", typeof(EndPoint), typeof(UdpServerConfig), null);


        /// <summary>
        /// 使用绑定
        /// </summary>
        public bool UseBind
        {
            get { return (bool)GetValue(UseBindProperty); }
            set { SetValue(UseBindProperty, value); }
        }

        /// <summary>
        /// 使用绑定
        /// </summary>
        public static readonly DependencyProperty UseBindProperty =
            DependencyProperty.Register("UseBind", typeof(bool), typeof(UdpServerConfig), false);



    }
}
