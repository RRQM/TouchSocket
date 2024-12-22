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

using System.Reflection;
using TouchSocket.Rpc;

namespace TouchSocket.WebApi
{
    internal class WebApiParameterInfo
    {
        public WebApiParameterInfo(RpcParameter parameter)
        {
            this.IsFromBody = parameter.ParameterInfo.IsDefined(typeof(FromBodyAttribute), false);

            if (parameter.ParameterInfo.GetCustomAttribute(typeof(FromQueryAttribute), false) is FromQueryAttribute fromQueryAttribute)
            {
                this.IsFromQuery = true;
                this.FromQueryName = fromQueryAttribute.Name ?? parameter.Name;
            }
            if (parameter.ParameterInfo.GetCustomAttribute(typeof(FromFormAttribute), false) is FromFormAttribute fromFormAttribute)
            {
                this.IsFromForm = true;
                this.FromFormName = fromFormAttribute.Name ?? parameter.Name;
            }

            if (parameter.ParameterInfo.GetCustomAttribute(typeof(FromHeaderAttribute), false) is FromHeaderAttribute fromHeaderAttribute)
            {
                this.IsFromHeader = true;
                this.FromHeaderName = fromHeaderAttribute.Name ?? parameter.Name;
            }

            this.Parameter = parameter;
        }

        public string FromFormName { get; }
        public string FromHeaderName { get; }
        public string FromQueryName { get; }
        public bool IsFromBody { get; }
        public bool IsFromForm { get; }
        public bool IsFromHeader { get; }
        public bool IsFromQuery { get; }
        public RpcParameter Parameter { get; }
    }
}
