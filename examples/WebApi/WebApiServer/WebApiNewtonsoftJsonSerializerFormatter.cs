using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using TouchSocket.Core;
using TouchSocket.Http;

namespace WebApiServerApp
{
    #region WebApi定义NewtonsoftJson序列化器
    internal class WebApiNewtonsoftJsonSerializerFormatter: JsonStringToClassSerializerFormatter<HttpContext>
    {
        public override bool TrySerialize<TTarget>(HttpContext state, in TTarget target, out string source)
        {
            switch (state.Request.Accept)
            {
                case "application/xml":
                case "text/xml":
                    {
                        source = default;
                        return false;
                    }
                case "application/json":
                case "text/json":
                case "text/plain":
                default:
                    return base.TrySerialize(state, target, out source);
            }
        }
    }

    public class JsonStringToClassSerializerFormatter<TState> : ISerializerFormatter<string, TState>
    {
        /// <inheritdoc/>
        public int Order { get; set; }

        /// <summary>
        /// JsonSettings
        /// </summary>
        public JsonSerializerSettings JsonSettings { get; set; } = new JsonSerializerSettings();

        /// <inheritdoc/>
        public virtual bool TryDeserialize(TState state, in string source, [DynamicallyAccessedMembers(AOT.SerializerFormatterMemberType)] Type targetType, out object target)
        {
            try
            {
                target = JsonConvert.DeserializeObject(source, targetType, this.JsonSettings);
                return true;
            }
            catch
            {
                target = default;
                return false;
            }
        }

        /// <inheritdoc/>
        public virtual bool TrySerialize<TTarget>(TState state, in TTarget target, out string source)
        {
            try
            {
                source = JsonConvert.SerializeObject(target, this.JsonSettings);
                return true;
            }
            catch
            {
                source = null;
                return false;
            }
        }
    }
    #endregion
}
