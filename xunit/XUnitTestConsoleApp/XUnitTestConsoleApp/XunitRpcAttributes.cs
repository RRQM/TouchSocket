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
//------------------------------------------------------------------------------

using TouchSocket.JsonRpc;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.WebApi;
using TouchSocket.XmlRpc;

namespace XUnitTestConsoleApp
{
    internal class XunitJsonRpcAttribute : JsonRpcAttribute
    {
        public XunitJsonRpcAttribute()
        {
            //this.MethodName = "JsonRpc_{0}";//实现：如果是JsonRpc，则函数以JsonRpc开头.
        }
    }

    internal class XunitDmtpRpcAttribute : DmtpRpcAttribute
    {
        public XunitDmtpRpcAttribute()
        {
            //this.MethodName = "TouchRpc_{0}";//实现：如果是DmtpRpc，则函数以DmtpRpc开头.
        }
    }

    internal class XunitWebApiAttribute : WebApiAttribute
    {
        public XunitWebApiAttribute(HttpMethodType methodType) : base(methodType)
        {
            this.MethodName = Method + "_{0}";//实现：如果是GET，则函数以GET开头，如果是POST，则以POST开头
        }
    }

    internal class XunitXmlRpcAttribute : XmlRpcAttribute
    {
        public XunitXmlRpcAttribute()
        {
            //this.MethodName = "XmlRpc_{0}";//实现：如果是XmlRpc，则函数以XmlRpc开头.
        }
    }
}