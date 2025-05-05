using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Lkeap.V20240522;
using TencentCloud.Lkeap.V20240522.Models;

namespace AiXmlSinify
{
    public class tencentDeepseek : IchatAI
    {
        LkeapClient client = null;
        private string[] args;
        public async Task<string> GetTranslationResults(string content)
        {
            ChatCompletionsRequest req = new ChatCompletionsRequest();
            req.Model = "deepseek-v3";
            req.Stream = false;
            Message message1 = new Message();
            message1.Role = "user";
            message1.Content = content;
            Message message2 = new Message();
            message2.Role = "system";
            message2.Content = args[2];
            req.Messages = new Message[] { message1 ,message2 };
            // 返回的resp是一个ChatCompletionsResponse的实例，与请求对象对应
            ChatCompletionsResponse resp = await client.ChatCompletions(req);

            return resp.Choices[0].Message.Content;

        }

        public void Init(params string[] _args)
        {
            this.args = _args;
            Credential cred = new Credential
            {
                SecretId = _args[0],
                SecretKey = _args[1]
            };
            // 实例化一个client选项，可选的，没有特殊需求可以跳过
            ClientProfile clientProfile = new ClientProfile();
            // 实例化一个http选项，可选的，没有特殊需求可以跳过
            HttpProfile httpProfile = new HttpProfile();
            httpProfile.Endpoint = ("lkeap.tencentcloudapi.com");
            clientProfile.HttpProfile = httpProfile;

            // 实例化要请求产品的client对象,clientProfile是可选的
            client = new LkeapClient(cred, "ap-guangzhou", clientProfile);
            // 实例化一个请求对象,每个接口都会对应一个request对象
           
        }
    }
}
