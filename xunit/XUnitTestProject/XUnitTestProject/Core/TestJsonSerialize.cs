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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TouchSocket.Core;
using TouchSocket.JsonRpc;

namespace XUnitTestProject.Core
{
    
    public class TestJsonSerialize
    {
        [Fact]
        public void JobjectShouldBeSerialize()
        {
            var jobject = new JObject();
            jobject.Add("a", "张三");
            jobject.Add("b", 10);
            jobject.Add("c", 10f);
        }

        [Fact]
        public void JobjectModelShouldBeSerialize()
        {
            var user = new UserInfoModel();
            user.ExpAdd = $"5%";
            user.ExplodeAdd = $"6%";
            user.LLAdd = $"6%";
            user.MoneyAdd = $"6%";
            user.WGAdd = $"6%";
            user.WFAdd = $"6%";
            user.HCAdd = $"6%";
            var jobject = new JObject();
            jobject.Add("Result", "OK");
            jobject.Add("Data", JToken.FromObject(user));
        }

        [Fact]
        public void NewtonsoftJsonShouldBeOk()
        {
            var response = new JsonRpcSuccessResponse()
            {
                Id = 10,
                Result = new UserInfoModel()
                {
                    ExpAdd = "1",
                    ExplodeAdd = "2"
                }
            };
            var ss = SerializeConvert.ToJsonString(response);
        }

        [Fact]
        public void Json()
        {
            //string str = "{\"jsonrpc\": \"2.0\", \"method\": \"update\", \"params\": [1,2,3,4,5]}";
            //string str = "{\"jsonrpc\": \"2.0\", \"method\": \"subtract\", \"params\": {\"subtrahend\": 23, \"minuend\": {\"a\":1}}, \"id\": 3}";
            //string str = "{\"jsonrpc\":\"2.0\",\"result\":{\"P1\":10,\"P2\":{\"P1\":100,\"P2\":{\"P1\":1000}}},\"id\":\"1\"}";
            var str = "{\"jsonrpc\":\"2.0\",\"result\":{\"ExpAdd\":\"1\",\"MoneyAdd\":null,\"ExplodeAdd\":\"2\",\"LLAdd\":null,\"WGAdd\":null,\"WFAdd\":null,\"HCAdd\":null},\"id\":\"10\"}";

            var response = new JsonRpcSuccessResponse()
            {
                Id = 10,
                Result = new UserInfoModel()
                {
                    ExpAdd = "1",
                    ExplodeAdd = "2"
                }
            };

            var ss = JsonConvert.SerializeObject(response);

            var pp = JsonConvert.DeserializeObject<JsonRpcResponseContext>(str);
            var vv = str.FromJsonString<JsonRpcResponseContext>();

            var s1 = vv.Result.ToJsonString();
            var u = s1.FromJsonString<UserInfoModel>();
        }
    }

    public class JsonRpcPackage
    {
        public string jsonrpc;
        public string method;
        public object @params;
        public string id;
        public bool needResponse;
    }

    public class UserInfoModel
    {
        /// <summary>
        /// 经验加成
        /// </summary>
        public string ExpAdd { get; set; }

        /// <summary>
        /// 金币加成
        /// </summary>
        public string MoneyAdd { get; set; }

        /// <summary>
        /// 爆率加成
        /// </summary>
        public string ExplodeAdd { get; set; }

        /// <summary>
        /// 历练
        /// </summary>
        public string LLAdd { get; set; }

        /// <summary>
        /// 武功
        /// </summary>
        public string WGAdd { get; set; }

        /// <summary>
        /// 物防
        /// </summary>
        public string WFAdd { get; set; }

        /// <summary>
        /// 合成强化概率
        /// </summary>
        public string HCAdd { get; set; }
    }
}