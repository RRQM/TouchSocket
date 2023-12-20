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
    }
}
