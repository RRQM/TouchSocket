//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 当正在配置Config时触发。
    /// </summary>
    //[Obsolete("此插件已被弃用，原因是在加载配置时，不应该构建插进管理器，也就不能通过插件管理器触发相关业务。同时该插件还会扰乱配置，故此弃用",true)]
    public interface ILoadingConfigPlugin<in TSender> : IPlugin where TSender : IConfigObject
    {
        /// <summary>
        /// 当载入配置时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        Task OnLoadingConfig(TSender sender, ConfigEventArgs e);
    }

    /// <summary>
    /// ILoadingConfigPlugin
    /// </summary>
    //[Obsolete("此插件已被弃用，原因是在加载配置时，不应该构建插进管理器，也就不能通过插件管理器触发相关业务。同时该插件还会扰乱配置，故此弃用", true)]
    public interface ILoadingConfigPlugin : ILoadingConfigPlugin<IConfigObject>
    {
    }
}