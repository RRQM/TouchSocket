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

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

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
        ThrowHelper.ThrowIfNull(dynamicMethodInfo, nameof(dynamicMethodInfo));
        ThrowHelper.ThrowIfNull(method, nameof(method));
        this.m_info = method;
        this.m_dynamicMethodInfo = dynamicMethodInfo;
    }

    /// <summary>
    /// 构造方法，初始化 Method 实例。
    /// </summary>
    /// <param name="method">目标方法信息。</param>
    [UnconditionalSuppressMessage("AOT", "IL2026", Justification = "Method方法在AOT确定")]
    public Method(MethodInfo method)
    {
        ThrowHelper.ThrowIfNull(method, nameof(method));
        this.m_info = method;
        this.m_dynamicMethodInfo = this.CreateDynamicMethodInfoFromSG();
        if (this.m_dynamicMethodInfo != null)
        {
            return;
        }

        try
        {
            this.m_dynamicMethodInfo = new ExpressionDynamicMethodInfo(method);
            return;
        }
        catch
        {
        }
        this.m_dynamicMethodInfo = new ReflectDynamicMethodInfo(method);
    }

    /// <summary>
    /// 初始化一个动态调用方法。
    /// </summary>
    /// <param name="targetType">目标类型。</param>
    /// <param name="methodName">目标方法名。</param>
    public Method([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type targetType, string methodName)
        : this(targetType.GetMethod(methodName))
    {
    }

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
    /// <para>当方法为void或task时，为<see langword="null"/>。</para>
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
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "源生成器生成的代码在AOT环境中是安全的")]
    [UnconditionalSuppressMessage("Trimming", "IL2075:'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The parameter of method does not have matching annotations.", Justification = "源生成器确保相关成员在编译时存在")]
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
        return property == null ? default : (IDynamicMethodInfo)property.GetValue(null);
    }
}