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

using System.Runtime.CompilerServices;
using TouchSocket.Resources;

namespace TouchSocket.Core;

/// <summary>
/// 数据处理适配器
/// </summary>
public abstract class DataHandlingAdapter : SafetyDisposableObject
{
    /// <summary>
    /// 是否允许发送<see cref="IRequestInfo"/>对象。
    /// </summary>
    public abstract bool CanSendRequestInfo { get; }

    /// <summary>
    /// 日志记录器。
    /// </summary>
    public ILog Logger { get; private set; }

    /// <summary>
    /// 获取或设置适配器能接收的最大数据包长度。默认1024*1024 Byte。
    /// </summary>
    public long MaxPackageSize { get; set; } = 1024 * 1024 * 10;

    /// <summary>
    /// 如果指定的长度超过最大包大小，则抛出异常。
    /// </summary>
    /// <param name="length">待检查的长度值。</param>
    /// <remarks>
    /// 此方法用于确保传入的数据长度不会超过预设的最大包大小限制，
    /// 以避免处理过大的数据包导致的性能问题或内存溢出等问题。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ThrowIfMoreThanMaxPackageSize(long length)
    {
        if (length > this.MaxPackageSize)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(length), length, this.MaxPackageSize);
        }
    }

    /// <summary>
    /// 适配器所有者
    /// </summary>
    public object Owner { get; private set; }

    /// <summary>
    /// 当适配器在被第一次加载时调用。
    /// </summary>
    /// <param name="owner"></param>
    /// <exception cref="Exception">此适配器已被其他终端使用，请重新创建对象。</exception>
    public virtual void OnLoaded(object owner)
    {
        if (this.Owner != null)
        {
            throw new Exception(TouchSocketCoreResource.AdapterAlreadyUsed);
        }

        if (owner is ILoggerObject loggerObject)
        {
            this.Logger = loggerObject.Logger;
        }
        this.Owner = owner;
    }

    /// <summary>
    /// 重置解析器到初始状态。
    /// </summary>
    protected abstract void Reset();
}