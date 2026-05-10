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

using System.Text;

namespace TouchSocket.Semi;

/// <summary>
/// 表示 SECS-II JIS8（2 字节字符）格式数据项，使用 UTF-8 编码。
/// </summary>
public class JIS8SecsItem : StringSecsItem
{
    /// <summary>
    /// 初始化 <see cref="JIS8SecsItem"/> 的新实例。
    /// </summary>
    public JIS8SecsItem() : base(Encoding.UTF8) { }

    /// <inheritdoc/>
    public override SecsFormat SecsFormat => SecsFormat.JIS8;
}
