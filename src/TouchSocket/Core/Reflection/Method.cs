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
using System.Collections.Specialized;
using System.Reflection;
using System.Threading.Tasks;

namespace TouchSocket.Core.Reflection
{
    /// <summary>
    /// Task类型
    /// </summary>
    public enum TaskType
    {
        /// <summary>
        /// 没有Task
        /// </summary>
        None,

        /// <summary>
        /// 仅返回Task
        /// </summary>
        Task,

        /// <summary>
        /// 返回Task的值
        /// </summary>
        TaskObject
    }

    /// <summary>
    /// 表示方法
    /// </summary>
    public class Method
    {
        /// <summary>
        /// 属性
        /// </summary>
        public NameValueCollection Properties { get; set; }=new NameValueCollection();

        private readonly MethodInfo m_info;

        private readonly bool m_isByRef;

        /// <summary>
        /// 方法
        /// </summary>
        /// <param name="method">方法信息</param>
        public Method(MethodInfo method)
        {
            this.m_info = method ?? throw new ArgumentNullException(nameof(method));
            this.Name = method.Name;

            foreach (var item in method.GetParameters())
            {
                if (item.ParameterType.IsByRef)
                {
                    this.m_isByRef = true;
                    break;
                }
            }

            if (method.ReturnType == typeof(Task))
            {
                this.HasReturn = false;
                this.TaskType = TaskType.Task;
            }
            else if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                this.HasReturn = true;
                this.ReturnType = method.ReturnType.GetGenericArguments()[0];
                this.TaskType = TaskType.TaskObject;
            }
            else if (method.ReturnType == typeof(void))
            {
                this.HasReturn = false;
                this.TaskType = TaskType.None;
            }
            else
            {
                this.HasReturn = true;
                this.TaskType = TaskType.None;
                this.ReturnType = method.ReturnType;
            }
        }

        /// <summary>
        /// 是否具有返回值
        /// </summary>
        public bool HasReturn { get; private set; }

        /// <summary>
        /// 方法信息
        /// </summary>
        public MethodInfo Info => this.m_info;

        /// <summary>
        /// 是否有引用类型
        /// </summary>
        public bool IsByRef => this.m_isByRef;

        /// <summary>
        /// 获取方法名
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// 返回值类型。
        /// <para>当方法为void或task时，为null</para>
        /// <para>当方法为task泛型时，为泛型元素类型</para>
        /// </summary>
        public Type ReturnType { get; private set; }

        /// <summary>
        /// 返回值的Task类型。
        /// </summary>
        public TaskType TaskType { get; private set; }

        /// <summary>
        /// 执行方法。
        /// <para>当方法为void或task时，会返回null</para>
        /// <para>当方法为task泛型时，会wait后的值</para>
        /// <para>注意：当调用方为UI主线程时，调用异步方法，则极有可能发生死锁。</para>
        /// </summary>
        /// <param name="instance">实例</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public object Invoke(object instance, params object[] parameters)
        {
            switch (this.TaskType)
            {
                case TaskType.None:
                    {
                        object re;
                        re = this.m_info.Invoke(instance, parameters);
                        return re;
                    }
                case TaskType.Task:
                    {
                        object re;
                        re = this.m_info.Invoke(instance, parameters);
                        Task task = (Task)re;
                        task.Wait();
                        return default;
                    }
                case TaskType.TaskObject:
                    {
                        object re;
                        re = this.m_info.Invoke(instance, parameters);
                        Task task = (Task)re;
                        task.Wait();
                        return task.GetType().GetProperty("Result").GetValue(task);
                    }
                default:
                    return default;
            }
        }

        /// <summary>
        /// 调用异步结果
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<object> InvokeObjectAsync(object instance, params object[] parameters)
        {
            switch (this.TaskType)
            {
                case TaskType.None:
                    {
                        object re;
                        re = this.m_info.Invoke(instance, parameters);
                        return re;
                    }
                case TaskType.Task:
                    {
                        object re;
                        re = this.m_info.Invoke(instance, parameters);
                        Task task = (Task)re;
                        await task;
                        return default;
                    }
                case TaskType.TaskObject:
                    {
                        object re;
                        re = this.m_info.Invoke(instance, parameters);
                        Task task = (Task)re;
                        await task;
                        return task.GetType().GetProperty("Result").GetValue(task);
                    }
                default:
                    return default;
            }
        }

        /// <summary>
        /// 异步调用
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Task InvokeAsync(object instance, params object[] parameters)
        {
            switch (this.TaskType)
            {
                case TaskType.None:
                    {
                        throw new Exception("该方法不包含Task。");
                    }
                case TaskType.Task:
                    {
                        object re;
                        re = this.m_info.Invoke(instance, parameters);
                        return (Task)re;
                    }
                case TaskType.TaskObject:
                    {
                        object re;
                        re = this.m_info.Invoke(instance, parameters);
                        return (Task)re;
                    }
                default:
                    return default;
            }
        }
    }
}