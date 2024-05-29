//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using TouchSocket.Core;

namespace TouchSocket.SerialPorts
{
    /// <summary>
    /// SerialPortConfigExtension
    /// </summary>
    public static class SerialPortConfigExtension
    {
        /// <summary>
        /// 设置串口适配器
        /// </summary>
        public static readonly DependencyProperty<Func<SingleStreamDataHandlingAdapter>> SerialDataHandlingAdapterProperty =
            DependencyProperty<Func<SingleStreamDataHandlingAdapter>>.Register("SerialDataHandlingAdapter", null);

        /// <summary>
        /// 串口属性。
        /// </summary>
        public static readonly DependencyProperty<SerialPortOption> SerialPortOptionProperty =
            DependencyProperty<SerialPortOption>.Register("SerialPortOption", new SerialPortOption());

        /// <summary>
        /// 设置(串口系)数据处理适配器。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetSerialDataHandlingAdapter(this TouchSocketConfig config, Func<SingleStreamDataHandlingAdapter> value)
        {
            config.SetValue(SerialDataHandlingAdapterProperty, value);
            return config;
        }

        /// <summary>
        /// 设置串口属性。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetSerialPortOption(this TouchSocketConfig config, SerialPortOption value)
        {
            config.SetValue(SerialPortOptionProperty, value);
            return config;
        }
    }
}