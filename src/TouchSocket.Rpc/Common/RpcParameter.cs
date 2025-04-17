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

using System;
using System.ComponentModel;
using System.Reflection;
using TouchSocket.Core;

namespace TouchSocket.Rpc;

/// <summary>
/// Rpc参数
/// </summary>
public class RpcParameter
{
    /// <summary>
    /// Rpc参数
    /// </summary>
    public RpcParameter(ParameterInfo parameterInfo)
    {
        this.ParameterInfo = parameterInfo;
        this.Type = parameterInfo.ParameterType.GetRefOutType();
        this.IsCallContext = typeof(ICallContext).IsAssignableFrom(this.Type);
        this.IsFromServices = parameterInfo.IsDefined(typeof(FromServicesAttribute));
    }

    /// <summary>
    /// 参数信息
    /// </summary>
    public ParameterInfo ParameterInfo { get; }

    /// <summary>
    /// 参数描述
    /// </summary>
    public string ParameterDesc => this.ParameterInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;
    
    /// <summary>
    /// 参数名称
    /// </summary>
    public string Name => this.ParameterInfo.Name;

    /// <summary>
    /// 参数类型，已处理out或者ref
    /// </summary>
    public Type Type { get; private set; }

    /// <summary>
    /// 是否为调用上下文
    /// </summary>
    public bool IsCallContext { get; private set; }

    /// <summary>
    /// 标识参数是否应该来自于服务
    /// </summary>
    public bool IsFromServices { get; private set; }

    /// <summary>
    /// 包含Out或者Ref
    /// </summary>
    public bool IsByRef => this.ParameterInfo.ParameterType.IsByRef;
}