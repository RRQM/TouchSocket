using RRQMCore.Exceptions;
using RRQMSocket.RPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// Type扩展累类
    /// </summary>
    public static class TypeHelper
    {
        /// <summary>
        /// 获取类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetRefOutType(this Type type)
        {
            if (type.FullName.Contains("&"))
            {
                string typeName = type.FullName.Replace("&", string.Empty);
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    type = assembly.GetType(typeName);

                    if (type != null)
                    {
                        return type;
                    }
                }

                throw new RRQMException($"未能识别类型{typeName}");
            }
            else
            {
                return type;
            }
        }
    }
}
