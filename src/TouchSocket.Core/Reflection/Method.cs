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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;

namespace TouchSocket.Core;

/// <summary>
/// 动态方法调用器。
/// </summary>
public class Method
{
    private const string GeneratorTypeNamespace = "TouchSocket.Core.__Internals";
    private readonly IDynamicMethodInfo m_dynamicMethodInfo;
    private readonly MethodInfo m_info;

    /// <summary>
    /// 使用指定的方法信息和动态方法信息初始化<see cref="Method"/>类的新实例。
    /// </summary>
    /// <param name="method">关于要表示的方法的元数据信息。不可能 <see langword="null"/>.</param>
    /// <param name="dynamicMethodInfo">与该方法相关联的动态方法信息。不可能 <see langword="null"/>.</param>
    public Method(MethodInfo method, IDynamicMethodInfo dynamicMethodInfo)
    {
        ThrowHelper.ThrowArgumentNullExceptionIf(dynamicMethodInfo, nameof(dynamicMethodInfo));
        this.m_info = ThrowHelper.ThrowArgumentNullExceptionIf(method, nameof(method));
        this.m_dynamicMethodInfo = dynamicMethodInfo;
    }

    /// <summary>
    /// 构造方法，初始化 Method 实例。
    /// </summary>
    /// <param name="method">目标方法信息。</param>
    /// <param name="dynamicBuilderType">指定动态构建类型。</param>
    public Method(MethodInfo method, DynamicBuilderType? dynamicBuilderType = default)
    {
        this.m_info = ThrowHelper.ThrowArgumentNullExceptionIf(method, nameof(method));

        if (dynamicBuilderType.HasValue)
        {
            switch (dynamicBuilderType.Value)
            {
                case DynamicBuilderType.IL:
                    {
                        if (!GlobalEnvironment.IsDynamicCodeSupported)
                        {
                            ThrowHelper.ThrowNotSupportedException($"当前环境不支持{dynamicBuilderType.Value}");
                        }
                        this.m_dynamicMethodInfo = new ILDynamicMethodInfo(method);
                        break;
                    }
                case DynamicBuilderType.Expression:
                    this.m_dynamicMethodInfo = new ExpressionDynamicMethodInfo(method);
                    break;

                case DynamicBuilderType.Reflect:
                    this.m_dynamicMethodInfo = new ReflectDynamicMethodInfo(method);
                    break;

                case DynamicBuilderType.SourceGenerator:
                    this.m_dynamicMethodInfo = this.CreateDynamicMethodInfoFromSG();
                    break;

                default:
                    break;
            }

            this.DynamicBuilderType = dynamicBuilderType.Value;
            return;
        }

        this.m_dynamicMethodInfo = this.CreateDynamicMethodInfoFromSG();
        if (this.m_dynamicMethodInfo != null)
        {
            this.DynamicBuilderType = DynamicBuilderType.SourceGenerator;
            return;
        }

        try
        {
            this.m_dynamicMethodInfo = new ExpressionDynamicMethodInfo(method);
            this.DynamicBuilderType = DynamicBuilderType.Expression;
            return;
        }
        catch
        {
        }
        this.m_dynamicMethodInfo = new ReflectDynamicMethodInfo(method);
        this.DynamicBuilderType = DynamicBuilderType.Reflect;
    }

    /// <summary>
    /// 初始化一个动态调用方法。
    /// </summary>
    /// <param name="targetType">目标类型。</param>
    /// <param name="methodName">目标方法名。</param>
    /// <param name="dynamicBuilderType">指定构建的类型。</param>
    public Method([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type targetType, string methodName, DynamicBuilderType? dynamicBuilderType = default)
        : this(targetType.GetMethod(methodName), dynamicBuilderType)
    {
    }

    /// <summary>
    /// 获取调用器的构建类型。
    /// </summary>
    public DynamicBuilderType DynamicBuilderType { get; private set; }

    /// <summary>
    /// 是否具有返回值。当返回值为Task时，也会认为没有返回值。
    /// </summary>
    public bool HasReturn => this.RealReturnType != null;

    /// <summary>
    /// 方法信息。
    /// </summary>
    public MethodInfo Info => this.m_info;

    /// <summary>
    /// 获取一个值，该值指示该方法的返回类型是否支持等待。
    /// </summary>
    public bool IsAwaitable => this.ReturnKind == MethodReturnKind.Awaitable || this.ReturnKind == MethodReturnKind.AwaitableObject;

    /// <summary>
    /// 获取方法名。
    /// </summary>
    public string Name => this.m_info.Name;

    /// <summary>
    /// 真实返回值类型。
    /// <para>当方法为void或task时，为null。</para>
    /// <para>当方法为task泛型时，为泛型元素类型。</para>
    /// </summary>
    public Type RealReturnType => this.m_dynamicMethodInfo.RealReturnType;

    /// <summary>
    /// 返回值的Task类型。
    /// </summary>
    public MethodReturnKind ReturnKind => this.m_dynamicMethodInfo.ReturnKind;

    #region Invoke

    /// <summary>
    /// 同步调用方法。
    /// </summary>
    /// <param name="instance">实例对象。</param>
    /// <param name="parameters">参数数组。</param>
    /// <returns>方法返回值。</returns>
    public object Invoke(object instance, params object[] parameters)
    {
        return this.m_dynamicMethodInfo.Invoke(instance, parameters);
    }

    /// <summary>
    /// 异步调用方法，返回指定类型结果。
    /// </summary>
    /// <typeparam name="TResult">结果类型。</typeparam>
    /// <param name="instance">实例对象。</param>
    /// <param name="parameters">参数数组。</param>
    /// <returns>异步任务，包含方法返回值。</returns>
    public async Task<TResult> InvokeAsync<TResult>(object instance, params object[] parameters)
    {
        return (TResult)await this.InvokeAsync(instance, parameters).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 异步调用方法。
    /// </summary>
    /// <param name="instance">实例对象。</param>
    /// <param name="parameters">参数数组。</param>
    /// <returns>异步任务，包含方法返回值。</returns>
    public async Task<object> InvokeAsync(object instance, params object[] parameters)
    {
        switch (this.ReturnKind)
        {
            case MethodReturnKind.Void:
                this.Invoke(instance, parameters);
                return default;

            case MethodReturnKind.Object:
                return this.Invoke(instance, parameters);

            case MethodReturnKind.Awaitable:
                {
                    var rawResult = this.Invoke(instance, parameters);
                    await this.m_dynamicMethodInfo.GetResultAsync(rawResult).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    return default;
                }
            case MethodReturnKind.AwaitableObject:
                {
                    var rawResult = this.Invoke(instance, parameters);
                    return (await this.m_dynamicMethodInfo.GetResultAsync(rawResult).ConfigureAwait(EasyTask.ContinueOnCapturedContext));
                }
            default:
                ThrowHelper.ThrowInvalidEnumArgumentException(this.ReturnKind);
                return null;
        }
    }

    #endregion Invoke

    /// <summary>
    /// 通过源生成器创建动态方法信息。
    /// </summary>
    /// <returns>动态方法信息接口。</returns>
    private IDynamicMethodInfo CreateDynamicMethodInfoFromSG()
    {
        var typeName = $"{GeneratorTypeNamespace}.__{StringExtension.MakeIdentifier(this.Info.DeclaringType.FullName)}MethodExtension";

        var type = this.Info.DeclaringType.Assembly.GetType(typeName);
        if (type == null)
        {
            return default;
        }

        var methodName = $"{this.Info.GetDeterminantName()}ClassProperty";
        var property = type.GetProperty(methodName, BindingFlags.Public | BindingFlags.Static);
        if (property == null)
        {
            return default;
        }

        return (IDynamicMethodInfo)property.GetValue(null);
    }
}