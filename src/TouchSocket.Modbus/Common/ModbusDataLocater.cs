using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Modbus
{
    internal class ModbusDataLocater
    {
        private bool[] m_coils;
        private bool[] m_discreteInputs;
        private byte[] m_holdingRegisters;
        private byte[] m_inputRegisters;
        public ModbusDataLocater(int coilsQuantity, int discreteInputsQuantity, int holdingRegistersQuantity, int inputRegistersQuantity)
        {
            this.m_coils = new bool[coilsQuantity];
            this.m_discreteInputs = new bool[discreteInputsQuantity];
            this.m_holdingRegisters = new byte[holdingRegistersQuantity*2];
            this.m_inputRegisters=new byte[inputRegistersQuantity*2];
        }

        public IModbusResponse Execute(ModbusRequest modbusRequest)
        {
            return new InternalModbusResponse(default, FunctionCode.ReadCoils, ModbusErrorCode.Success);
        }

        class InternalModbusResponse : IModbusResponse
        {
            public InternalModbusResponse(byte[] data, FunctionCode functionCode, ModbusErrorCode errorCode)
            {
                this.Data = data;
                this.FunctionCode = functionCode;
                this.ErrorCode = errorCode;
            }

            public byte[] Data { get; }

            public FunctionCode FunctionCode { get; }

            public ModbusErrorCode ErrorCode { get; }
        }
    }
}
