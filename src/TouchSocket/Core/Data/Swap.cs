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
namespace TouchSocket.Core
{
    /// <summary>
    /// 交换类。功能类似：a=1，b=2，交换后a=2，b=1。
    /// </summary>
    public static class Swap
    {
        /// <summary>
        /// 执行交换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void Execute<T>(ref T x, ref T y)
        {
#if NET45_OR_GREATER
            T temp = x;
            x = y;
            y = temp;
#else
            (y, x) = (x, y);
#endif
        }
    }
}