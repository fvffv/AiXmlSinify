# AI 翻译 C# 鼠标悬停文档 | AiXmlSinify

## 简介
此软件用于汉化 Visual Studio 的 **IntelliSense** 代码悬浮提示。官方提供的本地化 **.NET IntelliSense** 文件仅支持到 5.0.X 版本，详情参见：[.NET IntelliSense 下载](https://dotnet.microsoft.com/zh-cn/download/intellisense)。  
**同时支持批量汉化第三方 NuGet 包。**  
经调试，DeepSeek 在中文汉化方面表现最佳，因此软件默认使用 DeepSeek 进行 AI 汉化，同时也支持自定义 AI 接口。

---

## 调用 DeepSeek 的 API
软件支持调用多个平台的 DeepSeek 模型，包括：
- 腾讯
- 百度
- DeepSeek 官方
- TokenAI  

**推荐使用 TokenAI**：[注册链接](https://api.token-ai.cn/register?inviteCode=460a9518-e024-4caf-9654-5693432eee23)  
注册赠送免费额度，后续最低充值 3.5 元，支持多线程加速且无频率限制，适合大规模文本汉化。

> **缓存机制**：软件会自动缓存已汉化的内容，避免重复调用 API 消耗 Token。发布时会自带预汉化缓存文件，用户下载后可直接使用。

---

## 使用教程

### 1. 下载软件
- **GitHub Releases**：右侧下载  
- **国内蓝奏云**：[下载地址](https://wwos.lanzoub.com/iIVQK2wezdgd)  

### 2. 解压
**务必保留 `Cache` 文件夹**，否则无法使用预汉化缓存，将直接调用 API 消耗 Token。

### 3. 配置 Token（以 TokenAI 为例）
1. 注册后进入 **API Key 管理** → **创建 Token**  
   ![a1](https://github.com/fvffv/AiXmlSinify/blob/master/img/a1.png)  
2. 填写配置并复制 Key  
   ![a2](https://github.com/fvffv/AiXmlSinify/blob/master/img/a2.png)  
   ![a3](https://github.com/fvffv/AiXmlSinify/blob/master/img/a3.png)  
3. 将 Key 填入 `config.json` 的 `tokenAPI.apikey`  
   ![a4](https://github.com/fvffv/AiXmlSinify/blob/master/img/a4.png)  
4. 建议充值 3.5 元备用  
   ![a5](https://github.com/fvffv/AiXmlSinify/blob/master/img/a5.png)  

### 4. 运行软件
1. **输入汉化路径**：  
   - .NET Core 类注解路径：`C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref`  
   - NuGet 包路径：`C:\Users\<用户名>\.nuget\packages`  
   - 或通过 Visual Studio 右键依赖项 → **在资源管理器中打开文件夹** → 找到 `lib` 内的 `.xml` 文件  
     ![a6](https://github.com/fvffv/AiXmlSinify/blob/master/img/a6.png)  
   - 支持拖放文件/文件夹，自动识别 `.xml` 文件。  

2. **设置线程数**：默认 300，可根据文件量调整（仅 TokenAI 无频率限制）。  

3. **开始汉化**：  
   - 生成文件：汉化后文件、原文件备份（`.old`）、错误日志（`.err`）。  
   - 错误重试：超时 30 秒自动重试，次数由 `config.json` 的 `retryCount` 控制。  

---

## 配置文件详解
```json
{
  "useAI": "当前使用的AI名称",
  "defaultPrompt": "默认提示词",
  "errorCount": "节点翻译错误上限",
  "retryCount": "翻译错误重试次数",
  "tokenAI": "自定义AI参数（需与useAI匹配）",
  "deepseek": "...",
  "tencentDeepseek": "...",
  "baiduAI": "..."
}
