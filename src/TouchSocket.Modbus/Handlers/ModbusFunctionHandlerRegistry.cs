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

namespace TouchSocket.Modbus;

/// <summary>
/// Modbus功能码处理器注册表，管理<see cref="IModbusFunctionHandler"/>的注册与查找。
/// 通过注册自定义处理器可扩展对非标准功能码的支持。
/// </summary>
public sealed class ModbusFunctionHandlerRegistry
{
    private static readonly ModbusFunctionHandlerRegistry s_default = CreateDefault();
    private readonly Dictionary<FunctionCode, IModbusFunctionHandler> m_handlers = new();
    private readonly ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();

    /// <summary>
    /// 全局默认注册表实例，预置了所有标准Modbus功能码处理器
    /// </summary>
    public static ModbusFunctionHandlerRegistry Default => s_default;

    private static ModbusFunctionHandlerRegistry CreateDefault()
    {
        var registry = new ModbusFunctionHandlerRegistry();
        registry.Register(new ReadFunctionHandler(FunctionCode.ReadCoils));
        registry.Register(new ReadFunctionHandler(FunctionCode.ReadDiscreteInputs));
        registry.Register(new ReadFunctionHandler(FunctionCode.ReadHoldingRegisters));
        registry.Register(new ReadFunctionHandler(FunctionCode.ReadInputRegisters));
        registry.Register(new WriteSingleFunctionHandler(FunctionCode.WriteSingleCoil));
        registry.Register(new WriteSingleFunctionHandler(FunctionCode.WriteSingleRegister));
        registry.Register(new WriteMultipleFunctionHandler(FunctionCode.WriteMultipleCoils));
        registry.Register(new WriteMultipleFunctionHandler(FunctionCode.WriteMultipleRegisters));
        registry.Register(new ReadWriteMultipleRegistersHandler());
        return registry;
    }

    /// <summary>
    /// 注册功能码处理器，若已存在同功能码的处理器则覆盖
    /// </summary>
    /// <param name="handler">要注册的处理器</param>
    public void Register(IModbusFunctionHandler handler)
    {
        this.m_lock.EnterWriteLock();
        try
        {
            this.m_handlers[handler.FunctionCode] = handler;
        }
        finally
        {
            this.m_lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// 注销指定功能码的处理器
    /// </summary>
    /// <param name="functionCode">要注销的功能码</param>
    public void Unregister(FunctionCode functionCode)
    {
        this.m_lock.EnterWriteLock();
        try
        {
            this.m_handlers.Remove(functionCode);
        }
        finally
        {
            this.m_lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// 获取指定功能码的处理器，若未注册则返回<see langword="null"/>
    /// </summary>
    /// <param name="functionCode">功能码</param>
    /// <returns>对应的处理器，若未找到则返回<see langword="null"/></returns>
    public IModbusFunctionHandler GetHandler(FunctionCode functionCode)
    {
        this.m_lock.EnterReadLock();
        try
        {
            this.m_handlers.TryGetValue(functionCode, out var handler);
            return handler;
        }
        finally
        {
            this.m_lock.ExitReadLock();
        }
    }
}
