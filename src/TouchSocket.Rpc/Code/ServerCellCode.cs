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

namespace TouchSocket.Rpc;

/// <summary>
/// 服务单元代码
/// </summary>
public class ServerCellCode
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public ServerCellCode()
    {
        this.Methods = new Dictionary<string, MethodCellCode>();
        this.ClassCellCodes = new Dictionary<string, ClassCellCode>();
    }

    /// <summary>
    /// 包含接口
    /// </summary>
    public bool IncludeInterface { get; set; }

    /// <summary>
    /// 包含实例
    /// </summary>
    public bool IncludeInstance { get; set; }

    /// <summary>
    /// 包含扩展
    /// </summary>
    public bool IncludeExtension { get; set; }

    /// <summary>
    /// 服务名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 生成代理时，额外的命名空间
    /// </summary>
    public List<string> Namespaces { get; } = new List<string>();

    /// <summary>
    /// 方法集合
    /// </summary>
    public Dictionary<string, MethodCellCode> Methods { get; set; }

    /// <summary>
    /// 类参数集合。
    /// </summary>
    public Dictionary<string, ClassCellCode> ClassCellCodes { get; set; }
}