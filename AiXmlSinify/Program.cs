
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Security;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace AiXmlSinify
{
    internal class Program
    {


        static async Task Main(string[] args)
        {


        //string[] tmpp = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "Cache/", "*.xml", SearchOption.AllDirectories);
        //foreach (string p in tmpp)
        //{
        //    string ff = await GetFileMd5Async(p);
        //    File.Move(p, AppDomain.CurrentDomain.BaseDirectory + "Cache/" + Path.GetFileNameWithoutExtension(p) + "#" + ff + ".xml");
        //}
        TOP:



            List<string> XmlFile = new List<string>();
            XmlDocument doc = new XmlDocument();
            string path = AppDomain.CurrentDomain.BaseDirectory + "config.json";
            int TasksNum = 0;
            ConcurrentDictionary<string, string> Cache = new ConcurrentDictionary<string, string>();
            ConcurrentDictionary<string, string> Cache2 = new ConcurrentDictionary<string, string>();
            IchatAI ichatAI=null ;
            //分别是 翻译错误重试次数，最终翻译错误不超过这个数就保存本地缓存
            int retryCount = 0, errorCount = 0 ;
            //创建汉化过的文件夹 这里将放汉化过的文件，文件名为汉化前的md5
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Cache"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Cache");
            }

            //整个if是为了在管理员模式前实现拖入功能，因为系统限制 管理员后无法拖入，并且需要管理员权限写出文件
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "pathFile.tmp"))
            {
                Cache2 = await LoadCache();
                XmlFile = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "pathFile.tmp").ToList();
                foreach (string fileUrl in XmlFile)
                {
                    doc.Load(fileUrl);
                    TasksNum += doc.SelectNodes("/doc/members/member").Count;
                }
                Console.WriteLine($"已读取 {XmlFile.Count} 个Xml文件,总共有 {TasksNum} 个任务");
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "pathFile.tmp");
            //初始化AI
            error3:
                JObject jsonObj = JObject.Parse(File.ReadAllText(path));
                string useAI = (string)jsonObj["useAI"] ;
                errorCount = (int)jsonObj["errorCount"];
                retryCount = (int)jsonObj["retryCount"];
                if (string.IsNullOrEmpty(useAI))
                {
                    Console.WriteLine("请在配置文件中useAI写入对应AI名字");
                    Console.ReadLine();
                    goto error3;
                }
                Console.WriteLine($"当前使用AI：{useAI}");
                //动态切换类
                ichatAI = (IchatAI)Activator.CreateInstance(Type.GetType("AiXmlSinify." + useAI));
                var AINode = jsonObj[useAI] as JObject;
                

                //初始化获取参数
                ichatAI.Init(AINode.Properties().Select(x =>
                {
                    //如果prompt为空 则替换成默认
                    string tmp = x.Value.ToString();
                    if (x.Name == "prompt" && string.IsNullOrEmpty(tmp))
                    {
                        return (string)jsonObj["defaultPormpt"];
                    }
                    else
                    {
                        return tmp;
                    }
                }).ToArray());

                //Console.WriteLine(await ichatAI.GetTranslationResults("你好呀"));

               // Console.WriteLine(await ichatAI.GetTranslationResults("<summary> Extension methods for configuring MVC view and data annotations localization services. </summary>"));
            }
            else
            {
                //检查创建配置文件
                CreateConfig(path);
                //启用拖入功能
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)==false)
                {
                    EnableDragDrop();
                }
                
                Console.WriteLine("此程序用于汉化Visual Studio的代码提示的Xml文件，可以汉化c#以及外部导入的包的代码提示的Xml文件");
                Console.WriteLine("一般情况c#自带的包代码提示文件在：C:\\Program Files\\dotnet\\packs");
                Console.WriteLine("外部的Nuget包语言文件地址：C:\\Users\\lu\\.nuget\\packages");
                Console.WriteLine("想要找到具体的文件位置可以使用Notepad++搜索此目录，关键词为你要汉化的英文");
                Console.WriteLine("请输入要翻译的c# XML后缀的路径(支持拖入整个文件夹和文件):");
                
            error1:
                //获取Xml汉化文件
               if(GetXmlFiles(XmlFile) ==0) 
                {

                    goto error1; 
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    goto TOP;
                }
            }





            //默认线程数
            int xianchengshu;
        error2:
            Console.Write("请输入线程数(默认300):");
            try
            {   
                
                xianchengshu = Int32.TryParse(Console.ReadLine(), out int result) ? result : 300;
            }
            catch (Exception e)
            {
                Console.WriteLine("线程输入错误 请重新输入");
                goto error2;
            }
            int Zcounter=0,counter=0,errorNum = 0;
          
            //主任务模块
            foreach (string fileUrl in XmlFile)
            {
                doc.Load(fileUrl);
                //这是xml的成员路径  批量翻译的关键
                var items = doc.SelectNodes("/doc/members/member");
                List<Task> tasks = new List<Task>();
                //计算md5
                string md5 = await GetFileMd5Async(fileUrl);
                //从本地Cache文件夹找是否有原文件md5对应名字的文件
                string[] filesPath = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "Cache/", "*.xml", SearchOption.AllDirectories);
                string CacheFilePath = filesPath.FirstOrDefault(x => x.Contains(md5));
                if (!string.IsNullOrEmpty(CacheFilePath)) {
                    try
                    {
                        File.Move(fileUrl, fileUrl + "_old", false);
                    }
                    catch (Exception)
                    {

                        
                    }
                    File.Copy(CacheFilePath, fileUrl,true);
                    Console.WriteLine("文件："+ Path.GetFileName(fileUrl)+" 命中缓存");
                    Interlocked.Add(ref Zcounter, items.Count);
                    continue;
                }


                StringBuilder errorXml = new StringBuilder(5000000);
                for (int t = 0; t < items.Count; t += xianchengshu)
                {
                    int end = Math.Min(t + xianchengshu, items.Count); // 计算当前批次的结束索引
                    for (int s = t; s < end; s++)
                    {
                        int k = s;
                        tasks.Add(Task.Run(async () =>
                        {
                            
                            int chongshi = 0;
                            string yuanwenben = items[k].InnerXml;
                            if (string.IsNullOrEmpty(yuanwenben))
                            {
                                Interlocked.Increment(ref counter);
                                Interlocked.Increment(ref Zcounter);
                                return; }
                            string fanyihou ="";
                        error1:

                            try
                            {
                                //缓存 如果这次的文本之前汉化过则直接写入不通过ai重复翻译
                                string huancun1 = null, huancun2 = null;
                                if (Cache.TryGetValue(yuanwenben, out huancun1) || Cache2.TryGetValue(items[k].Attributes["name"] == null ? "" : items[k].Attributes["name"].Value, out huancun2))
                                {
                                    Console.WriteLine($"[{k}]命中缓存");
                                    items[k].InnerXml = huancun1 ?? huancun2;
                                }
                                else
                                {
                                    //翻译
                                    fanyihou = await ichatAI.GetTranslationResults(yuanwenben);
                                    //校验xml格式
                                    items[k].InnerXml = fanyihou;
                                    //添加缓存
                                    Cache.TryAdd(yuanwenben, fanyihou);
                                }
                            }
                            catch (Exception e)
                            {
                                //翻译错误重试
                                if (chongshi <= retryCount) { chongshi++; Console.WriteLine($"[{k}]翻译错误正在重试第{chongshi}次"); goto error1; } else { Console.WriteLine($"[{k}]超出重试次数,翻译失败"); }
                                //输出错误内容
                                Console.WriteLine(fanyihou);
                                //添加错误日志
                                errorXml.AppendLine(yuanwenben + "\n==================翻译后：\n" + fanyihou + "\n==================xml校验错误提示：\n" + e.Message);
                                //恢复原来的xml
                                items[k].InnerXml = yuanwenben;
                                Interlocked.Increment(ref errorNum);
                            }
                            Interlocked.Increment(ref counter);
                            Interlocked.Increment(ref Zcounter);
                        Console.WriteLine($"-----------------------------------------------\n[{k}]当前任务文件名[{Path.GetFileName(fileUrl)}]\n当前任务进度 {counter}/{items.Count}\n总任务进度 {Zcounter}/{TasksNum}");
                        }));

                    }

                    await Task.WhenAll(tasks);
                    tasks.Clear();


                }
                
                try
                {
                    
                    counter = 0;
                    //保存旧文件
                    File.Move(fileUrl, fileUrl + "_old");
                    //写出因为翻译错误等报错未翻译的结构
                    if (!string.IsNullOrEmpty(errorXml.ToString())) { File.WriteAllText(fileUrl + ".error", errorXml.ToString()); }
                    //写出翻译的文件
                    doc.Save(fileUrl);
                    //如果错误数小于5个 则写出文件保存到cache文件夹中，如果下次遇到相同的md5文件 则会用这个直接替换
                    if (errorNum <= errorCount)
                    {
                        string NewMd5 = await GetFileMd5Async(fileUrl);
                        string FileName = AppDomain.CurrentDomain.BaseDirectory + "Cache/" + md5 + "#" + NewMd5+".xml";
                        doc.Save(FileName);
                    }
                }
                catch (Exception e)
                {

                    Console.WriteLine(e.Message);
                }
             


            }

            Console.WriteLine($"已完成！ 翻译错误数：{errorNum}");
            Console.WriteLine("结果已生成到原目录，并且保留旧文件，若有翻译错误 则会生成error文件");
            Console.ReadLine();

            //File.WriteAllText(@"e:\test.xml",str.ToString());

        }
        /// <summary>
        /// 加载本地缓存
        /// </summary>
        /// <returns></returns>
        private static ConcurrentDictionary<string, string> LoadCache2()
        {
            var  beforDT = System.DateTime.Now;
            Console.WriteLine("正在加载本地缓存，请稍等");
            ConcurrentDictionary<string, string> cache2 = new ConcurrentDictionary<string, string>();
            string[] filesPath = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "Cache/", "*.xml", SearchOption.AllDirectories);
            XmlDocument doc = new XmlDocument();
            List<Task> tasks = new List<Task>();
            foreach (var file in filesPath)
            {
                doc.Load(file);
                XmlNodeList list = doc.SelectNodes("/doc/members/member");
                
                for (int i = 0; i < list.Count; i++)
                {
                    cache2.TryAdd(list[i].Attributes["name"].Value, list[i].InnerXml);

                    
                }
               
            }
            var afterDT = System.DateTime.Now;
            
            Console.WriteLine($"已加载本地缓存 {cache2.Count} 个,耗时：{afterDT.Subtract(beforDT)}");
            return cache2;
        }


        private static async Task<ConcurrentDictionary<string, string>> LoadCache()
        {
            var beforDT = System.DateTime.Now;
            Console.WriteLine("正在加载本地缓存，请稍等");
            ConcurrentDictionary<string, string> cache2 = new ConcurrentDictionary<string, string>();
            string[] filesPath = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "Cache/", "*.xml", SearchOption.AllDirectories);

            List<Task> tasks = new List<Task>();
            foreach (var file in filesPath)
            {


                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        // 使用FileStream异步读取
                        using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);

                        var doc = new XmlDocument();

                        doc.Load(fileStream);
                        XmlNodeList list = doc.SelectNodes("/doc/members/member");
                        for (int i = 0; i < list.Count; i++)
                        {
                            cache2.TryAdd(list[i].Attributes["name"].Value, list[i].InnerXml);


                        }
                    }
                    catch (Exception e)
                    {

                        Console.WriteLine(e.Message);
                        throw;
                    }
                }));





            }
            await Task.WhenAll(tasks);
            var afterDT = System.DateTime.Now;
            Console.WriteLine($"已加载本地缓存 {cache2.Count} 个,耗时：{afterDT.Subtract(beforDT)}");
            return cache2;
        }
        /// <summary>
        /// 解析用户输入的地址递归查找所有的xml文件 查到的话就重启进入管理员模式
        /// </summary>
        /// <param name="XmlFile"></param>
        /// <returns></returns>
        private static int GetXmlFiles(List<string> XmlFile)
        {
            string[] tempFileUrls;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                tempFileUrls = new string[] { Console.ReadLine() };
            }
            else
            {
                 tempFileUrls = ProcessingPath(Console.ReadLine());
            }

           
                foreach (string fileUrl in tempFileUrls)
                {
                 Console.WriteLine($"测试：{Directory.Exists(fileUrl)}");
                    //如果是文件且是.xml后缀的话则添加到XmlFile
                    if (Path.GetExtension(fileUrl) == ".xml")
                    {
                        XmlFile.Add(fileUrl);
                    }
                    //如果是文件夹 枚举文件
                    if (Directory.Exists(fileUrl))
                    {
                        XmlFile.AddRange(Directory.GetFiles(fileUrl, "*.xml", SearchOption.AllDirectories));
                    }


                }
            if (XmlFile.Count == 0)
            {
                Console.WriteLine("无可翻译xml文件！请重新输入：");
                return 0;
            }
            File.WriteAllLines(AppDomain.CurrentDomain.BaseDirectory + "pathFile.tmp", XmlFile);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) == false)
            {
                //重启管理员模式
                RestartAsAdministrator();
            }

            return 1;
        }

        /// <summary>
        /// 创建配置文件
        /// </summary>
        /// <param name="path"></param>
        private static void CreateConfig(string path)
        {

            if (!File.Exists(path))
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ai翻译.config.json"))
                {

                    using (FileStream fileStream = new FileStream(path, FileMode.Create))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
                Console.WriteLine("第一次运行请打开软件运行目录下 config.json 进行配置");
                Console.ReadLine();
                
            }
        }

        // 导入 Windows API
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        const int STD_INPUT_HANDLE = -10;
        const uint ENABLE_EXTENDED_FLAGS = 0x0080;
        const uint ENABLE_QUICK_EDIT_MODE = 0x0040;
        /// <summary>
        /// 实现拖入文件
        /// </summary>
        static void EnableDragDrop()
        {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);
            GetConsoleMode(consoleHandle, out uint consoleMode);
            consoleMode |= ENABLE_EXTENDED_FLAGS;
            consoleMode &= ~ENABLE_QUICK_EDIT_MODE; // 禁用快速编辑模式（避免拖放冲突）
            SetConsoleMode(consoleHandle, consoleMode);
        }

        /// <summary>
        /// 重新进入程序
        /// </summary>
        static void RestartAsAdministrator()
        {
         error:
            var startInfo = new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                UseShellExecute = true,
                Verb = "runas" // 这是关键，表示以管理员权限运行
            };

            try
            {
                Process.Start(startInfo);
                Environment.Exit(0);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // 这里处理用户拒绝的情况
                Console.WriteLine("请不要拒绝管理员权限，否则可能将无法保存汉化文件！ 按回车重试");
                Console.ReadLine();
               goto error;
                
            }
        }
       
        public static string[] ProcessingPath(string content)
        {
            List<string> paths = new List<string>();

            // 正则表达式匹配：
            // 1. 引号内的路径 ("...")
            // 2. 或者以盘符开头的路径 (C:\...)
            var matches = Regex.Matches(content, @"""(.*?)""|([A-Za-z]:\\[^\""\s]*(?:\\[^\""\s]*)*)");

            foreach (Match match in matches)
            {
                if (!string.IsNullOrEmpty(match.Groups[1].Value))
                {
                    // 引号内的路径
                    paths.Add(match.Groups[1].Value);
                }
                else if (!string.IsNullOrEmpty(match.Groups[2].Value))
                {
                    // 无引号的路径
                    paths.Add(match.Groups[2].Value);
                }
            }

            return paths.ToArray();
        }
        public static async Task<string> GetFileMd5Async(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous))
                {
                    byte[] hashBytes = await md5.ComputeHashAsync(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
