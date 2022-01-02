using RRQMCore.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 端口转发配置
    /// </summary>
    public class NATServiceConfig : TcpServiceConfig
    {

        /// <summary>
        /// 转发的目标地址
        /// </summary>
        public IPHost TargetIPHost
        {
            get { return (IPHost)GetValue(TargetIPHostProperty); }
            set { SetValue(TargetIPHostProperty, value); }
        }

        /// <summary>
        /// 转发的目标地址，
        /// 所需类型<see cref="IPHost"/>
        /// </summary>
        public static readonly DependencyProperty TargetIPHostProperty =
            DependencyProperty.Register("TargetIPHost", typeof(IPHost), typeof(NATServiceConfig), null);


    }
}
