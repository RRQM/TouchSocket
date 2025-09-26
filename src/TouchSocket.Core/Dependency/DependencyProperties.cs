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

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TouchSocket.Core;

/// <summary>
/// 依赖属性集合，继承自字典，用于存储依赖属性的键值对。
/// 提供了增强的调试视图和便捷的操作方法。
/// </summary>
[DebuggerTypeProxy(typeof(DependencyPropertiesDebugView))]
internal class DependencyProperties : Dictionary<int, object>
{
    /// <summary>
    /// 添加或更新依赖属性值。
    /// </summary>
    /// <param name="key">属性键</param>
    /// <param name="value">属性值</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddOrUpdate(int key, object value)
    {
        this[key] = value;
    }

    /// <summary>
    /// 获取依赖属性值，如果不存在则返回默认值。
    /// </summary>
    /// <param name="key">属性键</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>属性值或默认值</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object GetValueOrDefault(int key, object defaultValue = null)
    {
        return this.TryGetValue(key, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// 获取强类型的依赖属性值，如果不存在或类型不匹配则返回默认值。
    /// </summary>
    /// <typeparam name="T">属性值类型</typeparam>
    /// <param name="key">属性键</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>属性值或默认值</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOrDefault<T>(int key, T defaultValue = default)
    {
        if (this.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }

    /// <summary>
    /// 尝试移除指定键的值并返回被移除的值。
    /// </summary>
    /// <param name="key">要移除的键</param>
    /// <param name="value">被移除的值</param>
    /// <returns>如果成功移除返回true，否则返回false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRemove(int key, out object value)
    {
        if (this.TryGetValue(key, out value))
        {
            this.Remove(key);
            return true;
        }
        value = null;
        return false;
    }

    /// <summary>
    /// 检查是否包含指定的依赖属性。
    /// </summary>
    /// <param name="propertyId">属性ID</param>
    /// <returns>如果包含返回true，否则返回false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasProperty(int propertyId)
    {
        return this.ContainsKey(propertyId);
    }

    /// <summary>
    /// 获取所有属性的键值对集合，以便于调试和枚举。
    /// </summary>
    /// <returns>包含属性名称和值的键值对数组</returns>
    public KeyValuePair<string, object>[] GetNamedProperties()
    {
        if (this.Count == 0)
        {
            return Array.Empty<KeyValuePair<string, object>>();
        }

        var result = new KeyValuePair<string, object>[this.Count];
        var index = 0;

        foreach (var kvp in this)
        {
            var propertyName = DependencyPropertyBase.s_keyNameMap.TryGetValue(kvp.Key, out var name)
                ? name
                : $"UnknownProperty_{kvp.Key}";

            result[index++] = new KeyValuePair<string, object>(propertyName, kvp.Value);
        }

        return result;
    }

    /// <summary>
    /// 清空所有属性并释放相关资源。
    /// </summary>
    public new void Clear()
    {
        // 对于实现了IDisposable的值，进行资源释放
        foreach (var value in this.Values)
        {
            if (value is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                }
                catch
                {
                    // 忽略释放过程中的异常，避免影响清理流程
                }
            }
        }

        base.Clear();
    }

    #region 调试视图

    /// <summary>
    /// 依赖属性调试视图，用于在调试器中更友好地显示属性信息。
    /// </summary>
    private sealed class DependencyPropertiesDebugView
    {
        private readonly DependencyProperties _instance;

        /// <summary>
        /// 初始化调试视图。
        /// </summary>
        /// <param name="instance">依赖属性实例</param>
        public DependencyPropertiesDebugView(DependencyProperties instance)
        {
            this._instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        /// <summary>
        /// 获取用于调试显示的属性项数组。
        /// 将依赖属性的ID转换为可读的属性名称。
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public DependencyPropertyDebugItem[] Items
        {
            get
            {
                if (this._instance.Count == 0)
                {
                    return Array.Empty<DependencyPropertyDebugItem>();
                }

                var items = new DependencyPropertyDebugItem[this._instance.Count];
                var index = 0;

                foreach (var kvp in this._instance)
                {
                    var propertyName = DependencyPropertyBase.s_keyNameMap.TryGetValue(kvp.Key, out var name)
                        ? name
                        : $"UnknownProperty_{kvp.Key}";

                    items[index++] = new DependencyPropertyDebugItem(
                        propertyName,
                        kvp.Key,
                        kvp.Value
                    );
                }

                return items;
            }
        }

        /// <summary>
        /// 获取属性总数。
        /// </summary>
        public int Count => this._instance.Count;
    }

    /// <summary>
    /// 用于调试显示的依赖属性项。
    /// </summary>
    [DebuggerDisplay("{PropertyName} (ID: {PropertyId}) = {Value}")]
    private readonly struct DependencyPropertyDebugItem
    {
        /// <summary>
        /// 初始化依赖属性调试项。
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="propertyId">属性ID</param>
        /// <param name="value">属性值</param>
        public DependencyPropertyDebugItem(string propertyName, int propertyId, object value)
        {
            this.PropertyName = propertyName;
            this.PropertyId = propertyId;
            this.Value = value;
            this.ValueType = value?.GetType()?.Name ?? "null";
        }

        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// 属性ID
        /// </summary>
        public int PropertyId { get; }

        /// <summary>
        /// 属性值
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// 值类型名称
        /// </summary>
        public string ValueType { get; }
    }

    #endregion
}