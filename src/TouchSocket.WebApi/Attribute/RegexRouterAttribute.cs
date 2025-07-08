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

namespace TouchSocket.WebApi;

/// <summary>  
/// 表示一个正则表达式路由的特性。  
/// </summary>  
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public sealed class RegexRouterAttribute : Attribute
{
    /// <summary>  
    /// 获取路由模板的正则表达式。  
    /// </summary>  
    public string RegexTemple { get; }

    /// <summary>  
    /// 初始化 <see cref="RegexRouterAttribute"/> 类的新实例。  
    /// </summary>  
    /// <param name="regexTemple">路由模板的正则表达式。</param>  
    public RegexRouterAttribute(string regexTemple)
    {
        this.RegexTemple = regexTemple;
    }
}