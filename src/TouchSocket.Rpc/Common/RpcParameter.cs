using System;
using System.Collections.Generic;
using System.Reflection;
using TouchSocket.Core;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// Rpc参数
    /// </summary>
    public class RpcParameter
    {
        /// <summary>
        /// Rpc参数
        /// </summary>
        public RpcParameter(ParameterInfo parameterInfo)
        {
            this.ParameterInfo = parameterInfo;
            this.Type = parameterInfo.ParameterType.GetRefOutType();
            this.IsCallContext = typeof(ICallContext).IsAssignableFrom(this.Type);
            this.IsFromServices = parameterInfo.IsDefined(typeof(FromServicesAttribute));
        }

        /// <summary>
        /// 参数信息
        /// </summary>
        public ParameterInfo ParameterInfo { get; }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name =>this.ParameterInfo.Name;

        /// <summary>
        /// 参数类型，已处理out或者ref
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// 是否为调用上下文
        /// </summary>
        public bool IsCallContext { get;private set; }

        /// <summary>
        /// 标识参数是否应该来自于服务
        /// </summary>
        public bool IsFromServices { get;private set; }

        /// <summary>
        /// 包含Out或者Ref
        /// </summary>
        public bool IsByRef =>this.ParameterInfo.ParameterType.IsByRef;
    }
}
