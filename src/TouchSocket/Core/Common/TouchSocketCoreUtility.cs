//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections;

namespace TouchSocket.Core
{
    /// <summary>
    /// 常量
    /// </summary>
    public class TouchSocketCoreUtility
    {
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public static readonly Type stringType = typeof(string);
        public static readonly Type byteType = typeof(byte);
        public static readonly Type sbyteType = typeof(sbyte);
        public static readonly Type shortType = typeof(short);
        public static readonly Type ushortType = typeof(ushort);
        public static readonly Type intType = typeof(int);
        public static readonly Type uintType = typeof(uint);
        public static readonly Type boolType = typeof(bool);
        public static readonly Type charType = typeof(char);
        public static readonly Type longType = typeof(long);
        public static readonly Type ulongType = typeof(ulong);
        public static readonly Type floatType = typeof(float);
        public static readonly Type doubleType = typeof(double);
        public static readonly Type decimalType = typeof(decimal);
        public static readonly Type dateTimeType = typeof(DateTime);
        public static readonly Type bytesType = typeof(byte[]);
        public static readonly Type dicType = typeof(IDictionary);
        public static readonly Type iEnumerableType = typeof(IEnumerable);
        public static readonly Type arrayType = typeof(Array);
        public static readonly Type listType = typeof(IList);
        public static readonly Type nullableType = typeof(Nullable<>);
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
    }
}