using RRQMSocket.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

/// <summary>
/// HTTP上下文事件委托
/// </summary>
/// <typeparam name="TClient"></typeparam>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate void HttpContextEventHandler<TClient>(TClient client, HttpContextEventArgs e) where TClient : IHttpClientBase;
