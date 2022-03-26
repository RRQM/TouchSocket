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
using RRQMCore.Converter;
using RRQMCore.Reflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.Plugins
{
    /// <summary>
    /// TCP命令行插件。
    /// </summary>
    public abstract class TcpCommandLinePlugin : TcpPluginBase
    {
        Dictionary<string, Method> pairs = new Dictionary<string, RRQMCore.Reflection.Method>();

        /// <summary>
        /// 字符串转换器，默认支持基础类型和Json。可以自定义。
        /// </summary>
        public StringConverter Converter { get; }

        /// <summary>
        /// 是否返回执行异常。
        /// </summary>
        public bool ReturnException { get; set; } = true;

        /// <summary>
        /// 当有执行异常时，不返回异常。
        /// </summary>
        /// <returns></returns>
        public TcpCommandLinePlugin NoReturnException()
        {
            this.ReturnException = false;
            return this;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpCommandLinePlugin()
        {
            this.Converter = new StringConverter();
            var ms = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(a => a.Name.EndsWith("Command"));
            foreach (var item in ms)
            {
                pairs.Add(item.Name.Replace("Command", string.Empty), new Method(item));
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnReceivedData(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            try
            {
                string[] strs = e.ByteBlock.ToString().Split(' ');
                if (strs.Length > 0 && this.pairs.TryGetValue(strs[0], out Method method))
                {
                    var ps = method.Info.GetParameters();
                    if (ps.Length == strs.Length - 1)
                    {
                        object[] os = new object[ps.Length];
                        for (int i = 0; i < ps.Length; i++)
                        {
                            os[i] = this.Converter.ConvertFrom(strs[i+1], ps[i].ParameterType);
                        }
                        e.Handled = true;

                        try
                        {
                            object result = method.Invoke(this, os);
                            if (method.HasReturn)
                            {
                                client.Send(this.Converter.ConvertTo(result));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (this.ReturnException)
                            {
                                client.Send(ex.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.Debug(RRQMCore.Log.LogType.Error, this, ex.Message, ex);
            }
            base.OnReceivedData(client, e);
        }
    }
}
