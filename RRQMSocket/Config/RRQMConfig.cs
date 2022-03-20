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
using RRQMCore;
using RRQMCore.Dependency;

namespace RRQMSocket
{
    /// <summary>
    /// 配置文件基类
    /// </summary>
    public class RRQMConfig : RRQMDependencyObject
    {
        /// <summary>
        /// 接收缓存容量，默认1024*10，其作用有两个：
        /// <list type="number">
        /// <item>指示单次可接受的最大数据量</item>
        /// <item>指示常规申请内存块的长度</item>
        /// </list>
        /// </summary>
        public int BufferLength
        {
            get => (int)this.GetValue(BufferLengthProperty);
            set => this.SetValue(BufferLengthProperty, value);
        }

        /// <summary>
        /// 接收缓存容量，默认1024*10，其作用有两个：
        /// <list type="number">
        /// <item>指示单次可接受的最大数据量</item>
        /// <item>指示常规申请内存块的长度</item>
        /// </list>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public RRQMConfig SetBufferLength(int value)
        {
            this.BufferLength = value;
            return this;
        }

        /// <summary>
        /// 接收缓存容量，默认1024*10，其作用有两个：
        /// <list type="number">
        /// <item>指示单次可接受的最大数据量</item>
        /// <item>指示常规申请内存块的长度</item>
        /// </list>
        /// 所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty BufferLengthProperty =
            DependencyProperty.Register("BufferLength", typeof(int), typeof(RRQMConfig), 1024 * 10);

        /// <summary>
        /// 接收类型，默认为<see cref="ReceiveType.Auto"/>
        /// <para><see cref="ReceiveType.Auto"/>为自动接收数据，然后主动触发。</para>
        /// <para><see cref="ReceiveType.None"/>为不投递IO接收申请，用户可通过<see cref="ITcpClientBase.GetStream"/>，获取到流以后，自己处理接收。注意：连接端不会感知主动断开</para>
        /// </summary>
        public ReceiveType ReceiveType
        {
            get => (ReceiveType)this.GetValue(ReceiveTypeProperty);
            set => this.SetValue(ReceiveTypeProperty, value);
        }

        /// <summary>
        /// 接收类型，默认为<see cref="ReceiveType.Auto"/>
        /// <para><see cref="ReceiveType.Auto"/>为自动接收数据，然后主动触发。</para>
        /// <para><see cref="ReceiveType.None"/>为不投递IO接收申请，用户可通过<see cref="ITcpClientBase.GetStream"/>，获取到流以后，自己处理接收。注意：连接端不会感知主动断开</para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public RRQMConfig SetReceiveType(ReceiveType value)
        {
            this.ReceiveType = value;
            return this;
        }

        /// <summary>
        /// 接收类型，默认为<see cref="ReceiveType.Auto"/>
        /// <para><see cref="ReceiveType.Auto"/>为自动接收数据，然后主动触发。</para>
        /// <para><see cref="ReceiveType.None"/>为不投递IO接收申请，用户可通过<see cref="ITcpClientBase.GetStream"/>，获取到流以后，自己处理接收。注意：连接端不会感知主动断开</para>
        /// 所需类型<see cref="RRQMSocket. ReceiveType"/>
        /// </summary>
        public static readonly DependencyProperty ReceiveTypeProperty =
            DependencyProperty.Register("ReceiveType", typeof(ReceiveType), typeof(RRQMConfig), ReceiveType.Auto);

        /// <summary>
        /// 使用插件
        /// </summary>
        public bool IsUsePlugin
        {
            get => (bool)this.GetValue(IsUsePluginProperty);
            set => this.SetValue(IsUsePluginProperty, value);
        }

        /// <summary>
        /// 启用插件
        /// </summary>
        /// <returns></returns>
        public RRQMConfig UsePlugin()
        {
            this.IsUsePlugin = true;
            return this;
        }

        /// <summary>
        /// 使用插件,
        /// 所需类型<see cref="bool"></see>
        /// </summary>
        public static readonly DependencyProperty IsUsePluginProperty =
            DependencyProperty.Register("IsUsePlugin", typeof(bool), typeof(RRQMConfig), false);
    }
}