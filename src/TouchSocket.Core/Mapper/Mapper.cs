//------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
//交流QQ群：234762506
// 感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace TouchSocket.Core;

/// <summary>
/// 映射数据
/// </summary>
public static class Mapper
{
    private const DynamicallyAccessedMemberTypes RequiredMemberTypes =
       DynamicallyAccessedMemberTypes.PublicConstructors |
       DynamicallyAccessedMemberTypes.PublicProperties |
       DynamicallyAccessedMemberTypes.PublicFields;

    /// <summary>
    /// 泛型静态缓存类 - 每个类型组合都有独立的静态字段
    /// 这样完全避免了字典和键的概念
    /// </summary>
    private static class MapperCache<[DynamicallyAccessedMembers(RequiredMemberTypes)] TIn,
        [DynamicallyAccessedMembers(RequiredMemberTypes)] TOut>
        where TIn : class
        where TOut : class, new()
    {
        // 每个类型组合都有一个独立的静态字段，JIT会为每个组合生成独立的代码
        public static readonly Func<TIn, TOut> Mapper = CreateMapper();

        // 配置映射缓存 - 使用配置哈希作为键
        private static readonly Dictionary<int, Func<TIn, TOut>> ConfiguredMappers = new();

        public static Func<TIn, TOut> GetMapper(MappingConfig config = null)
        {
            if (config == null)
            {
                return Mapper;
            }

            var configHash = GetConfigHash(config);
            if (ConfiguredMappers.TryGetValue(configHash, out var cachedMapper))
            {
                return cachedMapper;
            }

            lock (ConfiguredMappers)
            {
                if (ConfiguredMappers.TryGetValue(configHash, out cachedMapper))
                {
                    return cachedMapper;
                }

                var newMapper = CreateMapper(config);
                ConfiguredMappers[configHash] = newMapper;
                return newMapper;
            }
        }

        private static int GetConfigHash(MappingConfig config)
        {
            unchecked
            {
                var hash = 17;

                // 计算忽略属性的哈希
                foreach (var ignored in config.IgnoredProperties.OrderBy(x => x))
                {
                    hash = hash * 31 + (ignored?.GetHashCode() ?? 0);
                }

                // 计算属性映射的哈希
                foreach (var mapping in config.PropertyMappings.OrderBy(x => x.Key))
                {
                    hash = hash * 31 + (mapping.Key?.GetHashCode() ?? 0);
                    hash = hash * 31 + (mapping.Value?.GetHashCode() ?? 0);
                }

                return hash;
            }
        }

        private static Func<TIn, TOut> CreateMapper(MappingConfig config = null)
        {
            var sourceParam = Expression.Parameter(typeof(TIn), "source");
            var memberBindings = new List<MemberBinding>();

            // 映射属性
            var sourceProperties = typeof(TIn).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var targetProperties = typeof(TOut).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToArray();

            foreach (var targetProp in targetProperties)
            {
                // 检查是否被忽略
                if (config?.IgnoredProperties.Contains(targetProp.Name) == true)
                {
                    continue;
                }

                // 查找源属性名（考虑映射配置）
                var sourcePropertyName = GetSourcePropertyName(targetProp.Name, config);

                var sourceProp = Array.Find(sourceProperties, p =>
                    string.Equals(p.Name, sourcePropertyName, StringComparison.Ordinal) &&
                    p.CanRead);

                if (sourceProp is not null && IsAssignableFrom(targetProp.PropertyType, sourceProp.PropertyType))
                {
                    var sourcePropertyAccess = Expression.Property(sourceParam, sourceProp);
                    var binding = Expression.Bind(targetProp, ConvertIfNeeded(sourcePropertyAccess, targetProp.PropertyType));
                    memberBindings.Add(binding);
                }
            }

            // 映射字段
            var sourceFields = typeof(TIn).GetFields(BindingFlags.Public | BindingFlags.Instance);
            var targetFields = typeof(TOut).GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => !f.IsInitOnly)
                .ToArray();

            foreach (var targetField in targetFields)
            {
                // 检查是否被忽略
                if (config?.IgnoredProperties.Contains(targetField.Name) == true)
                {
                    continue;
                }

                // 查找源字段名（考虑映射配置）
                var sourceFieldName = GetSourcePropertyName(targetField.Name, config);

                var sourceField = Array.Find(sourceFields, f =>
                    string.Equals(f.Name, sourceFieldName, StringComparison.Ordinal));

                if (sourceField is not null && IsAssignableFrom(targetField.FieldType, sourceField.FieldType))
                {
                    var sourceFieldAccess = Expression.Field(sourceParam, sourceField);
                    var binding = Expression.Bind(targetField, ConvertIfNeeded(sourceFieldAccess, targetField.FieldType));
                    memberBindings.Add(binding);
                }
            }

            var newExpression = Expression.New(typeof(TOut));
            var memberInitExpression = Expression.MemberInit(newExpression, memberBindings);
            var lambda = Expression.Lambda<Func<TIn, TOut>>(memberInitExpression, sourceParam);
            return lambda.Compile();
        }

        private static string GetSourcePropertyName(string targetPropertyName, MappingConfig config)
        {
            if (config?.PropertyMappings != null)
            {
                // 查找反向映射：如果目标属性名在映射值中，返回对应的源属性名
                var reverseMapping = config.PropertyMappings.FirstOrDefault(x => x.Value == targetPropertyName);
                if (!string.IsNullOrEmpty(reverseMapping.Key))
                {
                    return reverseMapping.Key;
                }
            }

            return targetPropertyName;
        }

        private static bool IsAssignableFrom(Type targetType, Type sourceType)
        {
            return targetType.IsAssignableFrom(sourceType) ||
                   (targetType == sourceType) ||
                 (Nullable.GetUnderlyingType(targetType) == sourceType) ||
                (Nullable.GetUnderlyingType(sourceType) == targetType);
        }

        private static Expression ConvertIfNeeded(Expression sourceExpression, Type targetType)
        {
            if (sourceExpression.Type == targetType)
            {
                return sourceExpression;
            }

            var sourceUnderlyingType = Nullable.GetUnderlyingType(sourceExpression.Type);
            var targetUnderlyingType = Nullable.GetUnderlyingType(targetType);

            if (sourceUnderlyingType is not null && targetUnderlyingType is null && sourceUnderlyingType == targetType)
            {
                return Expression.Convert(sourceExpression, targetType);
            }

            if (sourceUnderlyingType is null && targetUnderlyingType is not null && sourceExpression.Type == targetUnderlyingType)
            {
                return Expression.Convert(sourceExpression, targetType);
            }

            if (targetType.IsAssignableFrom(sourceExpression.Type))
            {
                return Expression.Convert(sourceExpression, targetType);
            }

            return sourceExpression;
        }
    }

    /// <summary>
    /// 映射方法 - 直接使用泛型静态缓存（无配置）
    /// </summary>
    public static TOut Trans<[DynamicallyAccessedMembers(RequiredMemberTypes)] TIn,
            [DynamicallyAccessedMembers(RequiredMemberTypes)] TOut>(TIn source)
          where TIn : class
            where TOut : class, new()
    {
        ThrowHelper.ThrowIfNull(source, nameof(source));
        return MapperCache<TIn, TOut>.Mapper(source);
    }

    /// <summary>
    /// 映射方法 - 支持配置忽略属性和重新映射
    /// </summary>
    public static TOut Trans<[DynamicallyAccessedMembers(RequiredMemberTypes)] TIn,
            [DynamicallyAccessedMembers(RequiredMemberTypes)] TOut>(TIn source, MappingConfig config)
          where TIn : class
            where TOut : class, new()
    {
        ThrowHelper.ThrowIfNull(source, nameof(source));
        ThrowHelper.ThrowIfNull(config, nameof(config));
        return MapperCache<TIn, TOut>.GetMapper(config)(source);
    }

    /// <summary>
    /// 映射方法 - 支持通过委托配置
    /// </summary>
    public static TOut Trans<[DynamicallyAccessedMembers(RequiredMemberTypes)] TIn,
            [DynamicallyAccessedMembers(RequiredMemberTypes)] TOut>(TIn source, Action<MappingConfig> configAction)
          where TIn : class
            where TOut : class, new()
    {
        ThrowHelper.ThrowIfNull(source, nameof(source));

        ThrowHelper.ThrowIfNull(configAction, nameof(configAction));
        var config = new MappingConfig();
        configAction(config);

        return MapperCache<TIn, TOut>.GetMapper(config)(source);
    }
}