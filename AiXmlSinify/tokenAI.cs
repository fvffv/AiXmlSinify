
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiXmlSinify
{

    public class tokenAI: IchatAI
    {
        private  Kernel kernel ;
        private  string prompt;
        private  IChatCompletionService chat;

        public void Init(params string[] args) {
            prompt = args[2];
            var hc = new HttpClient(new OpenAIHttpClientHandler("https://api.token-ai.cn/") { MaxConnectionsPerServer = 100000 });
            hc.Timeout = TimeSpan.FromSeconds(60);
            System.Net.ServicePointManager.DefaultConnectionLimit = 512;
            kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
            modelId: args[1],
            apiKey: args[0],
            httpClient: hc).Build();
            chat = kernel.GetRequiredService<IChatCompletionService>();
        }
        /// <summary>
        /// 翻译
        /// </summary>
        /// <param name="Content">源文本</param>
        /// <returns></returns>
        public async Task<string> GetTranslationResults(string Content)
        {
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(prompt);
            chatHistory.AddUserMessage(Content);
            return (await chat.GetChatMessageContentsAsync(chatHistory)).First().Content;

        }
    }
}
