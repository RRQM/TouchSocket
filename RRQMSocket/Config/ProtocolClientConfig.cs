//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using RRQMCore.Dependency;

namespace RRQMSocket
{
    /// <summary>
    /// 协议客户端配置
    /// </summary>
    public class ProtocolClientConfig : TokenClientConfig
    {
        /// <summary>
        /// 心跳频率，默认为-1。（设置为-1时禁止心跳）
        /// </summary>
        public int HeartbeatFrequency
        {
            get { return (int)this.GetValue(HeartbeatFrequencyProperty); }
            set { this.SetValue(HeartbeatFrequencyProperty, value); }
        }

        /// <summary>
        /// 心跳频率，默认为-1。（设置为-1时禁止心跳），
        ///  所需类型<see cref="string"/>
        /// </summary>
        public static readonly DependencyProperty HeartbeatFrequencyProperty =
            DependencyProperty.Register("HeartbeatFrequency", typeof(int), typeof(ProtocolClientConfig), -1);
    }
}