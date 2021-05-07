using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        protected override HttpResponse OnResultConverter(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
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
