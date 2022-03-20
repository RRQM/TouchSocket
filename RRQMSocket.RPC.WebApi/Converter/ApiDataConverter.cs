//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMSocket.Http;

namespace RRQMSocket.RPC.WebApi
{
    /// <summary>
    /// Api结果转化器
    /// </summary>
    public abstract class ApiDataConverter
    {
        /// <summary>
        /// 在调用完成时转换结果
        /// </summary>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public abstract HttpResponse OnResult(MethodInvoker methodInvoker, MethodInstance methodInstance);

        /// <summary>
        /// 在调用时
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        public abstract void OnPost(HttpRequest httpRequest, ref MethodInvoker methodInvoker, MethodInstance methodInstance);
    }
}