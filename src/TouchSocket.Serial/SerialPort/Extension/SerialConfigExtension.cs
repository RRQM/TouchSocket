using TouchSocket.Core;

namespace TouchSocket.Serial
{


    /// <summary>
    /// 串口附加属性
    /// </summary>
    public static class SerialConfigExtension
    {
        /// <summary>
        /// 串口属性
        /// </summary>
        public static readonly DependencyProperty<SerialProperty> SerialProperty =
            DependencyProperty<SerialProperty>.Register("SerialProperty", new Serial.SerialProperty());

        /// <summary>
        /// 设置串口
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetSerialProperty(this TouchSocketConfig config, SerialProperty value)
        {
            config.SetValue(SerialProperty, value);
            return config;
        }

    }
}