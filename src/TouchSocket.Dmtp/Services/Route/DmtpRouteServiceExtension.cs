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
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// DmtpRouteServiceExtension
    /// </summary>
    public static class DmtpRouteServiceExtension
    {
        /// <summary>
        /// 添加Dmtp路由服务。
        /// </summary>
        /// <param name="registrator"></param>
        public static void AddDmtpRouteService(this IRegistrator registrator)
        {
            registrator.RegisterSingleton<IDmtpRouteService, DmtpRouteService>();
        }

        /// <summary>
        /// 添加基于设定委托的Dmtp路由服务。
        /// </summary>
        /// <param name="registrator"></param>
        /// <param name="func"></param>
        public static void AddDmtpRouteService(this IRegistrator registrator, Func<string, Task<IDmtpActor>> func)
        {
            registrator.RegisterSingleton<IDmtpRouteService>(new DmtpRouteService()
            {
                FindDmtpActor = func
            });
        }

        /// <summary>
        /// 添加基于设定委托的Dmtp路由服务。
        /// </summary>
        /// <param name="registrator"></param>
        /// <param name="action"></param>
        public static void AddDmtpRouteService(this IRegistrator registrator, Func<string, IDmtpActor> action)
        {
            AddDmtpRouteService(registrator, async (id) =>
            {
                await EasyTask.CompletedTask;
                return action.Invoke(id);
            });
        }
    }
}