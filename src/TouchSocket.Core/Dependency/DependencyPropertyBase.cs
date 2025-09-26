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

namespace TouchSocket.Core;

/// <summary>
/// 提供依赖属性(DependencyProperty)的基础实现。
/// </summary>
public abstract class DependencyPropertyBase
{
    internal static readonly Dictionary<int, string> s_keyNameMap = new();

    /// <summary>
    /// 用于生成依赖属性标识的唯一ID。
    /// </summary>
    private static int s_idCount = 0;

    /// <summary>
    /// 初始化依赖属性对象，为每个属性分配唯一的ID。
    /// </summary>
    public DependencyPropertyBase(string name)
    {
        // 原子性增加s_idCount并赋值给当前实例的Id，保证Id的唯一性。
        this.Id = Interlocked.Increment(ref s_idCount);
        this.Name = name;

        lock (s_keyNameMap)
        {
            s_keyNameMap.Add(this.Id, name);
        }
    }

    /// <summary>
    /// 标识属性的唯一ID。
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// 属性名称
    /// </summary>
    public string Name { get; }
}