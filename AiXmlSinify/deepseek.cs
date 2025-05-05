using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AiXmlSinify
{
    public static class deepseek
    {
        private static string  baseurl = "https://api.deepseek.com/v1/chat/completions";
        private static string prompt;
        private static  HttpClient client ;

        public static void InitInfo(string _apiKey, string _prompt)
        {
            prompt = _prompt;
            var handler = new HttpClientHandler
            {
                MaxConnectionsPerServer = 10000, // 每个服务器的最大连接数
                UseProxy = false             // 如果不使用代理可以禁用
            };
            client = new HttpClient(handler);
            // 在程序开始时设置全局连接限制
            ServicePointManager.DefaultConnectionLimit = 10000; // 提高到20个并发连接
            client.Timeout = TimeSpan.FromMinutes(5);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");//请求头密钥
        }
        public static async Task<string> GetTextAsync(string content)
        {


            var requestData = new
            {
                model = "deepseek-chat",
                messages = new[] { //消息结构
                    new {
                        role="system",//角色
                        content=prompt
                    },
                     new {
                        role="user",//角色
                        content=content //内容
                    }
                }
            };
            string jsonContent = JsonConvert.SerializeObject(requestData);
            var contents = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            string responseContent="";
            string jieguo = "";
            Debug.WriteLine(jsonContent);
            try
            {
                HttpResponseMessage response = await client.PostAsync(baseurl, contents);
                responseContent = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                jieguo =  result.choices[0].message.content;
                
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message );
            }

            return jieguo;
        }


    }
}
