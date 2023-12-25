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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// Modbus功能码
    /// </summary>
    public enum FunctionCode : byte
    {
        /// <summary>
        /// 读线圈寄存器	位	取得一组逻辑线圈的当前状态（ON/OFF )
        /// </summary>
        ReadCoils = 1,

        /// <summary>
        /// 读离散输入寄存器	位	取得一组开关输入的当前状态（ON/OFF )
        /// </summary>
        ReadDiscreteInputs = 2,

        /// <summary>
        /// 读保持寄存器	整型、浮点型、字符型	在一个或多个保持寄存器中取得当前的二进制值
        /// </summary>
        ReadHoldingRegisters = 3,

        /// <summary>
        /// 读输入寄存器	整型、浮点型	在一个或多个输入寄存器中取得当前的二进制值
        /// </summary>
        ReadInputRegisters = 4,

        /// <summary>
        /// 写单个线圈寄存器	位	强置一个逻辑线圈的通断状态
        /// </summary>
        WriteSingleCoil = 5,

        /// <summary>
        /// 写单个保持寄存器	整型、浮点型、字符型	把具体二进值装入一个保持寄存器
        /// </summary>
        WriteSingleRegister = 6,

        /// <summary>
        /// 写多个线圈寄存器	位	强置一串连续逻辑线圈的通断
        /// </summary>
        WriteMultipleCoils = 15,

        /// <summary>
        /// 写多个保持寄存器	整型、浮点型、字符型	把具体的二进制值装入一串连续的保持寄存器
        /// </summary>
        WriteMultipleRegisters = 16,

        /// <summary>
        /// 读写多个保持寄存器	整型、浮点型、字符型	把具体的二进制值装入一串连续的保持寄存器 并读取一系列寄存器
        /// </summary>
        ReadWriteMultipleRegisters = 23
    }
}
