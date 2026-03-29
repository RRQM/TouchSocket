// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

namespace TouchSocket.Core;

/// <summary>
/// 委托模式的 <see cref="IDependencyObject"/> 包装，将所有依赖属性操作转发给内部持有的 <see cref="IDependencyObject"/> 实例。
/// </summary>
public class RoomDependencyObject : IDependencyObject
{
    private readonly IDependencyObject m_dependencyObject;

    /// <summary>
    /// 初始化 <see cref="RoomDependencyObject"/> 类的新实例。
    /// </summary>
    /// <param name="dependencyObject">被委托的 <see cref="IDependencyObject"/> 实例。</param>
    public RoomDependencyObject(IDependencyObject dependencyObject)
    {
        this.m_dependencyObject = dependencyObject;
    }

    /// <inheritdoc/>
    public bool DisposedValue => this.m_dependencyObject.DisposedValue;

    /// <inheritdoc/>
    public void Dispose()
    {
        this.m_dependencyObject.Dispose();
    }

    /// <inheritdoc/>
    public TValue GetValue<TValue>(DependencyProperty<TValue> dp)
    {
        return this.m_dependencyObject.GetValue(dp);
    }

    /// <inheritdoc/>
    public bool HasValue<TValue>(DependencyProperty<TValue> dp)
    {
        return this.m_dependencyObject.HasValue(dp);
    }

    /// <inheritdoc/>
    public void RemoveValue<TValue>(DependencyProperty<TValue> dp)
    {
        this.m_dependencyObject.RemoveValue(dp);
    }

    /// <inheritdoc/>
    public void SetValue<TValue>(DependencyProperty<TValue> dp, TValue value)
    {
        this.m_dependencyObject.SetValue(dp, value);
    }

    /// <inheritdoc/>
    public bool TryGetValue<TValue>(DependencyProperty<TValue> dp, out TValue value)
    {
        return this.m_dependencyObject.TryGetValue(dp, out value);
    }

    /// <inheritdoc/>
    public bool TryRemoveValue<TValue>(DependencyProperty<TValue> dp, out TValue value)
    {
        return this.m_dependencyObject.TryRemoveValue(dp, out value);
    }
}
