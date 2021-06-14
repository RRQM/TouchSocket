using RRQMCore.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 协议服务配置
    /// </summary>
    public class ProtocolServerConfig : TokenServerConfig
    {

        /// <summary>
        /// 是否能重新设置ID
        /// </summary>
        public bool CanResetID
        {
            get { return (bool)GetValue(CanResetIDProperty); }
            set { SetValue(CanResetIDProperty, value); }
        }

        /// <summary>
        /// 是否能重新设置ID
        /// </summary>
        public static readonly DependencyProperty CanResetIDProperty =
            DependencyProperty.Register("CanResetID", typeof(bool), typeof(ProtocolServerConfig), true);


    }
}
