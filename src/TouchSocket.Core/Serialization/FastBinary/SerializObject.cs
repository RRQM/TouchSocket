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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TouchSocket.Core;

/// <summary>
/// 可序列化对象
/// </summary>
public sealed class SerializObject
{
    internal Method AddMethod;
    internal Type[] ArgTypes;
    // internal Type ArrayType;
    internal bool EnableIndex;
    internal Dictionary<byte, FastMemberInfo> FastMemberInfoDicForIndex;
    internal Dictionary<string, FastMemberInfo> FastMemberInfoDicForName;
    internal InstanceType InstanceType;
    internal bool IsStruct;
    internal MemberAccessor MemberAccessor;
    internal FastMemberInfo[] MemberInfos;

    /// <summary>
    /// 从转换器初始化
    /// </summary>
    /// <param name="type"></param>
    /// <param name="converter"></param>
    public SerializObject(Type type, IFastBinaryConverter converter)
    {
        this.Type = type;
        this.Converter = converter;
    }

    /// <summary>
    /// 从类型创建序列化器
    /// </summary>
    /// <param name="type"></param>
    /// <exception cref="Exception"></exception>
    public SerializObject(Type type)
    {
        this.Type = type;
        if (type.IsArray)//数组
        {
            this.InstanceType = InstanceType.Array;
            this.ArgTypes = new Type[] { type.GetElementType() };
        }
        else if (type.IsClass || type.IsStruct())
        {
            if (type.IsNullableType())
            {
                type = type.GetGenericArguments()[0];
            }
            this.Type = type;
            if (type.IsList())
            {
                var genericType = type;
                while (true)
                {
                    if (genericType.IsGenericType)
                    {
                        break;
                    }
                    genericType = genericType.BaseType;
                    if (genericType == TouchSocketCoreUtility.objType)
                    {
                        break;
                    }
                }
                this.ArgTypes = genericType.GetGenericArguments();
                this.AddMethod = new Method(type.GetMethod("Add"));
                this.InstanceType = InstanceType.List;
            }
            else if (type.IsDictionary())
            {
                var genericType = type;
                while (true)
                {
                    if (genericType.IsGenericType)
                    {
                        break;
                    }
                    genericType = genericType.BaseType;
                    if (genericType == TouchSocketCoreUtility.objType)
                    {
                        break;
                    }
                }
                this.ArgTypes = genericType.GetGenericArguments();
                this.AddMethod = new Method(type.GetMethod("Add"));
                this.InstanceType = InstanceType.Dictionary;
            }
            else
            {
                this.InstanceType = InstanceType.Class;
                this.MemberAccessor = new MemberAccessor(type)
                {
                    OnGetFieldInfes = GetFieldInfos,
                    OnGetProperties = GetProperties
                };
                this.MemberAccessor.Build();
            }

            if (type.GetCustomAttribute(typeof(FastConverterAttribute), false) is FastConverterAttribute attribute)
            {
                this.Converter = (IFastBinaryConverter)Activator.CreateInstance(attribute.Type);
            }

            if (type.GetCustomAttribute(typeof(FastSerializedAttribute), false) is FastSerializedAttribute fastSerializedAttribute)
            {
                this.EnableIndex = fastSerializedAttribute.EnableIndex;
            }

            var list = new List<MemberInfo>();
            list.AddRange(GetProperties(type));
            list.AddRange(GetFieldInfos(type));

            this.FastMemberInfoDicForIndex = new Dictionary<byte, FastMemberInfo>();
            this.FastMemberInfoDicForName = new Dictionary<string, FastMemberInfo>();

            if (this.EnableIndex)
            {
                foreach (var memberInfo in list)
                {
                    var fastMemberInfo = new FastMemberInfo(memberInfo, true);
                    if (this.FastMemberInfoDicForIndex.ContainsKey(fastMemberInfo.Index))
                    {
                        throw new Exception($"类型：{type}中的成员{memberInfo.Name}，在标识{nameof(FastMemberAttribute)}特性时Index重复。");
                    }
                    this.FastMemberInfoDicForIndex.Add(fastMemberInfo.Index, fastMemberInfo);
                }
            }
            else
            {
                foreach (var memberInfo in list)
                {
                    var fastMemberInfo = new FastMemberInfo(memberInfo, false);
                    this.FastMemberInfoDicForName.Add(fastMemberInfo.Name, fastMemberInfo);
                }
            }

            var infos = new List<FastMemberInfo>();
            infos.AddRange(this.FastMemberInfoDicForIndex.Values);
            infos.AddRange(this.FastMemberInfoDicForName.Values);
            this.MemberInfos = infos.ToArray();

            if (type.IsGenericType)
            {
                this.ArgTypes = type.GetGenericArguments();
            }
        }
        this.IsStruct = type.IsStruct();
    }

    /// <summary>
    /// 转化器
    /// </summary>
    public IFastBinaryConverter Converter { get; private set; }

    /// <summary>
    /// 类型
    /// </summary>
    public Type Type { get; private set; }

    private static FieldInfo[] GetFieldInfos(Type type)
    {
        return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Default)
            .Where(p =>
            {
                return !p.IsInitOnly && (!p.IsDefined(typeof(FastNonSerializedAttribute), false));
            })
            .ToArray();
    }

    private static PropertyInfo[] GetProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Default)
                      .Where(p =>
                      {
                          if (p.IsDefined(typeof(FastSerializedAttribute), false))
                          {
                              return true;
                          }

                          if (p.IsDefined(typeof(FastNonSerializedAttribute), false))
                          {
                              return false;
                          }

                          if (p.CanPublicRead() && p.CanPublicWrite())
                          {
                              return true;
                          }
                          return false;
                      })
                      .ToArray();
    }
}