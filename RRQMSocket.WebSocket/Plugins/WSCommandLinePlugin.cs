using RRQMCore.Converter;
using RRQMCore.Reflection;
using RRQMSocket.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.WebSocket.Plugins
{
    /// <summary>
    /// WS命令行插件。
    /// </summary>
    public abstract class WSCommandLinePlugin : WebSocketPluginBase
    {
        Dictionary<string, Method> pairs = new Dictionary<string, Method>();

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
        public WSCommandLinePlugin NoReturnException()
        {
            this.ReturnException = false;
            return this;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public WSCommandLinePlugin()
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
        protected override void OnHandleWSDataFrame(ITcpClientBase client, WSDataFrameEventArgs e)
        {
            if (e.DataFrame.Opcode== WSDataType.Text)
            {
                try
                {
                    string[] strs = e.DataFrame.ToText().Split(' ');
                    if (strs.Length > 0 && this.pairs.TryGetValue(strs[0], out Method method))
                    {
                        var ps = method.Info.GetParameters();
                        if (ps.Length == strs.Length - 1)
                        {
                            object[] os = new object[ps.Length];
                            for (int i = 0; i < ps.Length; i++)
                            {
                                os[i] = this.Converter.ConvertFrom(strs[i + 1], ps[i].ParameterType);
                            }
                            e.Handled = true;

                            try
                            {
                                object result = method.Invoke(this, os);
                                if (method.HasReturn)
                                {
                                    if (client is HttpClient httpClient)
                                    {
                                        httpClient.SendWithWS(this.Converter.ConvertTo(result));
                                    }
                                    else if (client is HttpSocketClient httpSocketClient)
                                    {
                                        httpSocketClient.SendWithWS(this.Converter.ConvertTo(result));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                if (this.ReturnException)
                                {
                                    if (client is HttpClient httpClient)
                                    {
                                        httpClient.SendWithWS(this.Converter.ConvertTo(ex.Message));
                                    }
                                    else if (client is HttpSocketClient httpSocketClient)
                                    {
                                        httpSocketClient.SendWithWS(this.Converter.ConvertTo(ex.Message));
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.Debug(RRQMCore.Log.LogType.Error, this, ex.Message, ex);
                }
            }
            
            base.OnHandleWSDataFrame(client, e);
        }
    }
}
