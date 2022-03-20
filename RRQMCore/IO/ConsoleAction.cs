//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace RRQMCore.IO
{
    /// <summary>
    /// 控制台行为
    /// </summary>
    public class ConsoleAction
    {
        private string helpOrder;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="helpOrder">帮助信息指令，如："h|help|?"</param>
        public ConsoleAction(string helpOrder)
        {
            this.helpOrder = helpOrder;

            this.Add(helpOrder, "帮助信息", () =>
            {
                List<string> s = new List<string>();
                foreach (var item in this.actions)
                {
                    if (!s.Contains(item.Value.FullOrder.ToLower()))
                    {
                        s.Add(item.Value.FullOrder.ToLower());
                        Console.WriteLine($"[{item.Value.FullOrder}]-------->{item.Value.Description}");
                    }
                }
            });
        }

        /// <summary>
        /// 帮助信息指令
        /// </summary>
        public string HelpOrder => this.helpOrder;

        private Dictionary<string, VAction> actions = new Dictionary<string, VAction>();

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="order">指令，多个指令用“|”分割</param>
        /// <param name="description">描述</param>
        /// <param name="action"></param>
        public void Add(string order, string description, Action action)
        {
            string[] orders = order.ToLower().Split('|');
            foreach (var item in orders)
            {
                this.actions.Add(item, new VAction(description, order, action));
            }
        }

        /// <summary>
        /// 执行异常
        /// </summary>
        public event Action<Exception> OnException;

        /// <summary>
        /// 执行，返回值仅表示是否有这个指令，异常获取请使用<see cref="OnException"/>
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool Run(string order)
        {
            if (this.actions.TryGetValue(order.ToLower(), out VAction vAction))
            {
                try
                {
                    vAction.Action.Invoke();
                }
                catch (Exception ex)
                {
                    this.OnException?.Invoke(ex);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    internal struct VAction
    {
        private Action action;

        public Action Action => this.action;

        private string fullOrder;

        public string FullOrder => this.fullOrder;

        private string description;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="action"></param>
        /// <param name="description"></param>
        /// <param name="fullOrder"></param>
        public VAction(string description, string fullOrder, Action action)
        {
            this.fullOrder = fullOrder;
            this.action = action ?? throw new ArgumentNullException(nameof(action));
            this.description = description ?? throw new ArgumentNullException(nameof(description));
        }

        public string Description => this.description;
    }
}