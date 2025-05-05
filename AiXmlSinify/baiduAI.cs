using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Newtonsoft.Json;
using RestSharp;
using TencentCloud.Emr.V20190103.Models;
using TencentCloud.Tbp.V20190311.Models;

namespace AiXmlSinify
{
    public class baiduAI : IchatAI
    {
        private RestRequest request =null;
        private RestClient client = new RestClient($"https://qianfan.baidubce.com/v2/chat/completions");
        private string[] args;
        public async Task<string> GetTranslationResults(string content)
        {
            var body = new
            {
                model = "deepseek-v3",
                messages = new[] { //消息结构
                    new {
                        role="system",//角色
                        content=args[1]
                    },
                     new {
                        role="user",//角色
                        content=content //内容
                    }
                }
            };
            request.AddOrUpdateParameter("application/json", JsonConvert.SerializeObject(body), ParameterType.RequestBody);
            IRestResponse response = await client.ExecuteAsync(request);
            //Console.WriteLine(response.Content);
            return JsonConvert.DeserializeObject<dynamic>(response.Content).choices[0].message.content;
        }

        public void Init(params string[] _args)
        {
            this.args = _args;
            client.Timeout = -1;
            request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", $"Bearer {args[0]}");

        }
    }
}
