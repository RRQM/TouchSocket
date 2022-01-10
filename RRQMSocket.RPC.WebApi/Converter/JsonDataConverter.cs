//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Helper;
using RRQMCore.XREF.Newtonsoft.Json;
using RRQMSocket.Http;

namespace RRQMSocket.RPC.WebApi
{
    /// <summary>
    /// Json数据转换器
    /// </summary>
    public class JsonDataConverter : ApiDataConverter
    {
        /// <summary>
        /// OnPost
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        public override void OnPost(HttpRequest httpRequest, ref MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            switch (httpRequest.Content_Type)
            {
                case "application/x-www-form-urlencoded":
                    {
                        if (httpRequest.Params != null)
                        {
                            for (int i = 0; i < methodInstance.Parameters.Length; i++)
                            {
                                if (httpRequest.Params.TryGetValue(methodInstance.ParameterNames[i], out string value))
                                {
                                    methodInvoker.Parameters[i] = value.ParseToType(methodInstance.ParameterTypes[i]);
                                }
                                else
                                {
                                    methodInvoker.Parameters[i] = methodInstance.ParameterTypes[i].GetDefault();
                                }
                            }
                        }
                        break;
                    }
                case "application/json":
                    {
                        if (methodInstance.Parameters.Length > 0)
                        {
                            for (int i = 0; i < methodInstance.Parameters.Length; i++)
                            {
                                if (i == 0)
                                {
                                    methodInvoker.Parameters[i] = JsonConvert.DeserializeObject(httpRequest.Body, methodInstance.ParameterTypes[0]);
                                }
                                else
                                {
                                    methodInvoker.Parameters[i] = methodInstance.ParameterTypes[i].GetDefault();
                                }
                            }
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// 在调用完成时转换结果
        /// </summary>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public override HttpResponse OnResult(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            HttpRequest httpRequest = (HttpRequest)methodInvoker.Flag;
            HttpResponse httpResponse = new HttpResponse();
            switch (methodInvoker.Status)
            {
                case InvokeStatus.Success:
                    {
                        if (methodInvoker.ReturnParameter != null)
                        {
                            httpResponse.FromJson(JsonConvert.SerializeObject(methodInvoker.ReturnParameter));
                            break;
                        }
                        else
                        {
                            httpResponse.FromText(string.Empty);
                        }

                        break;
                    }
                case InvokeStatus.UnFound:
                    {
                        string jsonString = JsonConvert.SerializeObject(new ActionResult() { Status = methodInvoker.Status, Message = methodInvoker.StatusMessage });
                        httpResponse.FromJson(jsonString, "404");
                        break;
                    }
                case InvokeStatus.UnEnable:
                    {
                        string jsonString = JsonConvert.SerializeObject(new ActionResult() { Status = methodInvoker.Status, Message = methodInvoker.StatusMessage });
                        httpResponse.FromJson(jsonString, "405");
                        break;
                    }
                case InvokeStatus.Abort:
                    {
                        string jsonString = JsonConvert.SerializeObject(new ActionResult() { Status = methodInvoker.Status, Message = methodInvoker.StatusMessage });
                        httpResponse.FromJson(jsonString, "403");
                        break;
                    }
                case InvokeStatus.InvocationException:
                case InvokeStatus.Exception:
                    {
                        string jsonString = JsonConvert.SerializeObject(new ActionResult() { Status = methodInvoker.Status, Message = methodInvoker.StatusMessage });
                        httpResponse.FromJson(jsonString, "422");
                        break;
                    }
            }

            return httpResponse;
        }
    }
}