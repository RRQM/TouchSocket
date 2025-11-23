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
using TouchSocket.Rpc;

namespace WebApiServerApp;

public class CustomResponseAttribute : RpcActionFilterAttribute
{
    public override async Task<InvokeResult> ExecutedAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult, Exception exception)
    {
        if (invokeResult.Status == InvokeStatus.Success)
        {
            //正常情况，直接返回数据
            return invokeResult;
        }
        else
        {
            //非正常情况，可以获取到错误信息
            var errorMsg = invokeResult.Message;
            //和异常
            var errorException = exception;

            return new InvokeResult()
            {
                Status = InvokeStatus.Success,
                Result = exception?.Message ?? "自定义结果"
            };
        }


        //if (callContext is IWebApiCallContext webApiCallContext)
        //{
        //    var response = webApiCallContext.HttpContext.Response;
        //    if (!response.Responsed)
        //    {
        //        response.SetStatus(500, "自定义状态码");
        //        await response.AnswerAsync();
        //    }

        //}

        //return invokeResult;
    }
}
