//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using TouchSocket.Core;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// ModbusResponseExtension
    /// </summary>
    public static class ModbusResponseExtension
    {
        ///// <summary>
        ///// 获取Modbus错误代码。如果没有错误，会返回<see cref="ModbusErrorCode.Success"/>
        ///// </summary>
        ///// <param name="response"></param>
        ///// <returns></returns>
        //public static ModbusErrorCode GetErrorCode(this IModbusResponse response)
        //{
        //    var value = (byte)(response.FunctionCode);
        //    if ((value & 0x80) == 0)
        //    {
        //        return 0;
        //    }

        //    value = value.SetBit(7, 0);
        //    return (ModbusErrorCode)value;
        //}

        /// <summary>
        /// 获取一个读取器。
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static ValueByteBlock CreateReader(this IModbusResponse response)
        {
            return new ValueByteBlock(response.Data);
        }
    }
}