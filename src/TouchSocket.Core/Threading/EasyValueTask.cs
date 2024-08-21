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

using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 定义了一个简化版本的ValueTask工具类。
    /// 该类提供了一些静态方法来创建ValueTask对象，旨在优化性能并简化异步编程。
    /// </summary>
    public class EasyValueTask
    {
        /// <summary>
        /// 类的静态构造函数，用于初始化静态字段。
        /// </summary>
        static EasyValueTask()
        {
            // 根据目标框架设置CompletedTask的值
            // 对于.NET 6.0及以上版本，直接使用ValueTask类的静态属性CompletedTask
            // 对于.NET 6.0以下版本，将CompletedTask设置为默认值
#if NET6_0_OR_GREATER
            CompletedTask = ValueTask.CompletedTask;
#else
            CompletedTask = default;
#endif
        }

        /// <summary>
        /// 获取一个表示已完成任务的静态属性。
        /// </summary>
        public static ValueTask CompletedTask { get; }

        /// <summary>
        /// 根据指定的结果创建一个ValueTask对象。
        /// </summary>
        /// <param name="result">作为ValueTask结果的值。</param>
        /// <typeparam name="TResult">ValueTask结果的类型。</typeparam>
        /// <returns>一个新的ValueTask对象，其结果被指定为传入的result参数。</returns>
        public static ValueTask<TResult> FromResult<TResult>(TResult result)
        {
            return new ValueTask<TResult>(result);
        }
    }
}