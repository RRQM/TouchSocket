//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
using RRQMCore.Serialization;
using RRQMSocket.Http;

namespace RRQMSocket.RPC.WebApi
{
    /// <summary>
    /// Xml结果转换器
    /// </summary>
    public class XmlResultConverter : ResultConverter
    {
        /// <summary>
        /// 在调用完成时转换结果
        /// </summary>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public override HttpResponse OnResultConverter(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            HttpRequest httpRequest = (HttpRequest)methodInvoker.Flag;
            HttpResponse httpResponse = new HttpResponse();
            switch (methodInvoker.Status)
            {
                case InvokeStatus.Success:
                    {
                        if (methodInvoker.ReturnParameter != null)
                        {
                            httpResponse.FromXML(SerializeConvert.XmlSerializeToString(methodInvoker.ReturnParameter));
                            break;
                        }
                        else
                        {
                            httpResponse.FromText(string.Empty);
                        }

                        break;
                    }
                case InvokeStatus.UnFound:
                case InvokeStatus.UnEnable:
                    {
                        httpResponse.FromText(methodInvoker.Status.ToString());
                        break;
                    }
                case InvokeStatus.Abort:
                    {
                        httpResponse.FromText(methodInvoker.Status.ToString());
                        break;
                    }
                case InvokeStatus.InvocationException:
                case InvokeStatus.Exception:
                    {
                        httpResponse.FromText(methodInvoker.Status.ToString());
                        break;
                    }
            }

            return httpResponse;
        }
    }
}