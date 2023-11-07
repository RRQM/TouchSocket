using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 具有释放的对象，包含一个<see cref="DisposedValue"/>来标识是否该对象已被释放。
    /// </summary>
    public partial interface IDisposableObject: IDisposable
    {
        /// <summary>
        /// 标识该对象是否已被释放
        /// </summary>
        bool DisposedValue { get; }
    }

#if NET6_0_OR_GREATER
    public partial interface IDisposableObject : IAsyncDisposable
    {
    }
#endif
}
