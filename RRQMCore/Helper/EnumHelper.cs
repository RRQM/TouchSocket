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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RRQMCore.Helper
{
    /// <summary>
    /// 枚举
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// 获取自定义attribute 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumObj"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this Enum enumObj) where T : Attribute
        {
            Type type = enumObj.GetType();
            Attribute attr = null;
            string enumName = Enum.GetName(type, enumObj);  //获取对应的枚举名
            FieldInfo field = type.GetField(enumName);
            attr = field.GetCustomAttribute(typeof(T), false);
            return (T)attr;
        }
    }
}
