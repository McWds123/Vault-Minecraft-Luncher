
using System.IO;
using System.Windows;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using Demo.Lib;

namespace Demo
{
    public partial class MainWindow : Window
    {
        private ScriptAPI _scriptAPI = new ScriptAPI();

        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();
            
        }

        private static LoadStyle _loadStyle = new LoadStyle();
        public dynamic htmlPath = _loadStyle.LoadUsingStyles();

        
        private class usingHtmlConfigJsonData
        {
            public string usingHtml { get; set; }
        }
        
        
        private async void InitializeAsync()
        {
            try
            {
        Console.WriteLine("[INFO] 开始初始化 WebView2");
        
        await webView2.EnsureCoreWebView2Async();
        Console.WriteLine("[INFO] WebView2 初始化完成");
        
        // 设置开发工具
        webView2.CoreWebView2.Settings.AreDevToolsEnabled = true;
        
        // 绑定JavaScript到C#
        webView2.CoreWebView2.Settings.AreDevToolsEnabled = true;
        webView2.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
        webView2.CoreWebView2.Settings.IsWebMessageEnabled = true;
        
        webView2.CoreWebView2.AddHostObjectToScript("scriptAPI", _scriptAPI);
        Console.WriteLine("[INFO] ScriptAPI 绑定完成");
        
        Console.WriteLine($"[INFO] 检查HTML文件: {Path.GetFileName(htmlPath)}");
        
        if (File.Exists(htmlPath))
        {
            try
            {
                FileInfo fileInfo = new FileInfo(htmlPath);
                Console.WriteLine($"[INFO] HTML文件大小: {fileInfo.Length} 字节");
                
                // 读取HTML但不输出内容
                string html = File.ReadAllText(htmlPath);
                Console.WriteLine("[INFO] HTML文件读取成功");
                
                // 直接加载HTML
                //webView2.NavigateToString(html);
                string fileUrl = new Uri(htmlPath).AbsoluteUri;
                webView2.CoreWebView2.Navigate(fileUrl);
                Console.WriteLine("[INFO] 正在加载HTML页面...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] 加载HTML失败: {ex.Message}");
                string errorHtml = @"
                    <!DOCTYPE html>
                    <html>
                    <head><meta charset='utf-8'><style>body{background:#2C3E50;color:white;padding:20px;}</style></head>
                    <body>
                        <h1>加载错误</h1>
                        <p>无法加载页面内容</p>
                    </body>
                    </html>
                ";
                webView2.NavigateToString(errorHtml);
            }
        }
        else
        {
            Console.WriteLine($"[WARNING] HTML文件不存在: {htmlPath}");
            
            // 显示文件不存在页面
            string notFoundHtml = @"
                <!DOCTYPE html>
                <html>
                <head><meta charset='utf-8'><style>body{background:#2C3E50;color:white;padding:20px;}</style></head>
                <body>
                    <h1>文件不存在</h1>
                    <p>找不到文件</p>
                </body>
                </html>
            ";
            webView2.NavigateToString(notFoundHtml);
        }
        
        LoadIcon();
        
        // 导航事件
        webView2.NavigationCompleted += async (s, e) =>
        {
            if (e.IsSuccess)
            {
                Console.WriteLine("[INFO] 页面加载完成");
                await WebView2_NavigationCompleted();
            }
            else
            {
                Console.WriteLine($"[ERROR] 页面加载失败: {e.WebErrorStatus}");
            }
        };
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[FATAL] 初始化失败: {ex.Message}");
        MessageBox.Show($"初始化失败: {ex.Message}");
    }
        }
        
        private async Task WebView2_NavigationCompleted()
        {
            try
            {
                webView2.CoreWebView2.AddHostObjectToScript("scriptAPI", _scriptAPI);
                await webView2.ExecuteScriptAsync(@"
            window.external = {
                SaveDataOfUsingHtml: function() {
                    return chrome.webview.hostObjects.scriptAPI.SaveDataOfUsingHtml();
                },
                RunGameButton: function() {
                    return chrome.webview.hostObjects.scriptAPI.RunGameButton();
                },
                ChoiceVersion: function() {
                    return chrome.webview.hostObjects.scriptAPI.ChoiceVersion();
                },
                DownloadClientJar: function(version) {
                    return chrome.webview.hostObjects.scriptAPI.DownloadClientJar(version);
                },
                styleList: function() {
                    return chrome.webview.hostObjects.scriptAPI.styleList();
                },
                GetSelectedText: function() {
                    return chrome.webview.hostObjects.scriptAPI.GetSelectedText();
                }
            };
        ");

                await LoadVersionButtons();
                _scriptAPI.AutomaticInvocation();
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载失败：" + ex.Message);
            }
        }
        private async Task LoadVersionButtons()
{
    try
    {
        Console.WriteLine("[INFO] 开始加载版本按钮");
        
        var loadWebsiteMinecraftVersion = new LoadWebsiteMinecraftVersion();
        string HtmlJson = await loadWebsiteMinecraftVersion.LoadVersionManifest();
        
        if (string.IsNullOrEmpty(HtmlJson))
        {
            Console.WriteLine("[ERROR] 无法获取版本清单");
            return;
        }
        
        Console.WriteLine($"[INFO] 获取到版本清单，长度: {HtmlJson.Length}");
        
        List<string> versionList = await loadWebsiteMinecraftVersion.getVersionList(HtmlJson);
        
        if (versionList == null || versionList.Count == 0)
        {
            Console.WriteLine("[WARNING] 版本列表为空");
            return;
        }
        
        Console.WriteLine($"[INFO] 原始版本数量: {versionList.Count}");
        
        var cleanVersions = versionList
            .Where(ver => !ver.Any(c => char.IsLetter(c)))
            .ToList();
            
        Console.WriteLine($"[INFO] 清理后版本数量: {cleanVersions.Count}");
        
        if (cleanVersions.Count == 0)
        {
            Console.WriteLine("[WARNING] 清理后版本列表为空");
            return;
        }
        
        // 显示前几个版本
        Console.WriteLine($"[INFO] 前5个版本: {string.Join(", ", cleanVersions.Take(5))}");
        
        // 构建JavaScript
        string script = @"
                console.log('开始注入版本按钮');
                var content2 = document.getElementById('content2');
                console.log('找到content2元素:', content2);
                
                var versions = " + System.Text.Json.JsonSerializer.Serialize(cleanVersions) + @";
                
                console.log('版本数据:', versions);
                
                for (var i = 0; i < versions.length; i++) {
                    var version = versions[i];
                    var btn = document.createElement('button');
                    btn.className = 'MinecraftVersionButton animate item' + ((i % 4) + 1);
                
              
                    var img = document.createElement('img');
                    img.src = 'icon/minecraft.png';
                    img.style.cssText = 'width:45px;height:45px;margin-right:50px;vertical-align:middle;';
                
                    btn.appendChild(img);
                    btn.appendChild(document.createTextNode(version));
             
                
                    btn.onclick = function () {
                        var version = this.innerText;
                        console.log('Minecraft版本按钮点击:', version);
                        showModal(version);
                        return false;
                    };
                
                    if (content2) {
                        content2.appendChild(btn);
                    } else {
                        console.error('找不到content2元素');
                    }
                }
                
                console.log('版本按钮注入完成');
        ";
        
        Console.WriteLine("[INFO] 执行JavaScript脚本");
        string result = await webView2.ExecuteScriptAsync(script);
        Console.WriteLine($"[INFO] JavaScript执行结果: {result}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] LoadVersionButtons失败: {ex.Message}");
        Console.WriteLine($"[ERROR] 堆栈: {ex.StackTrace}");
    }
}
        [ClassInterface(ClassInterfaceType.AutoDual)]
        [ComVisible(true)]
        public class ScriptAPI {
            public async Task RunGameButton()
            {
                string selectedVersion = await GetSelectedText();
                if (selectedVersion == "请选择版本")
                {
                    MessageBox.Show("请选择版本", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    RunGame(selectedVersion);
                }
            }
            public void RunGame(string VersionName)
            {
                System.Diagnostics.Process.Start($@"Minecraft\VML\Script\{VersionName}\Run.bat");
            }
            public async Task ChoiceVersion()
            {
                try
                {
                    Console.WriteLine("[INFO] ChoiceVersion函数开始执行");
        
                    string basePath = AppDomain.CurrentDomain.BaseDirectory;
                    string targetPath = Path.Combine(basePath, "Minecraft", "VML", "Script");
        
                    if (!Directory.Exists(targetPath))
                    {
                        Console.WriteLine("[ERROR] 目录不存在: " + targetPath);
                        return;
                    }
        
                    string[] folderPaths = Directory.GetDirectories(targetPath);
                    Console.WriteLine($"[INFO] 找到 {folderPaths.Length} 个版本");
                    
                    var options = new List<string> { "<option value=''>请选择版本</option>" };
                    foreach (string path in folderPaths)
                    {
                        string folderName = Path.GetFileName(path);
                        options.Add($"<option value='{folderName}'>{folderName}</option>");
                    }
        
                    // ✅ 修正：直接通过 Application.Current.MainWindow 获取
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null && mainWindow.webView2 != null)
                    {
                        await mainWindow.Dispatcher.InvokeAsync(async () =>
                        {
                            try
                            {
                                string script = $@"
                                    var select = document.getElementById('mySelect');
                                    if (select) {{
                                        select.innerHTML = `{string.Join("", options)}`;
                                    }}
                                ";
                                await mainWindow.webView2.ExecuteScriptAsync(script);
                                Console.WriteLine("[INFO] 版本列表已更新到下拉框");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[ERROR] 更新下拉框失败: {ex.Message}");
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] ChoiceVersion失败: {ex.Message}");
                }
            }
            public async Task AddOption(string selectId, string value, string text)
            {
                try
                {
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow == null || mainWindow.webView2 == null) 
                        return;

                    // 拼接 JS 代码：创建 option 并添加到下拉框
                    string script = $@"
            try {{
                let select = document.getElementById('{selectId}');
                if (select) {{
                    let option = document.createElement('option');
                    option.value = '{value.Replace("'", "\\'")}';
                    option.text = '{text.Replace("'", "\\'")}';
                    select.add(option);
                }}
            }} catch(e) {{
                console.error('添加选项失败', e);
            }}
        ";

                    await mainWindow.webView2.ExecuteScriptAsync(script);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] AddOption 失败: {ex.Message}");
                }
            }
            public async void styleList()
            {
                try
                {
                    int LoopTime = 0;
                    LoadStyle loadStyle = new LoadStyle();
                    List<dynamic> StyleFiles = loadStyle.LoadStyles(); 

                    foreach (var item in StyleFiles)
                    {
                        LoopTime++;
                        await AddOption("StyleSelect", LoopTime.ToString(), item);
                    }
                    Console.WriteLine("[INFO] 样式列表加载完成");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] styleList 失败: {ex.Message}");
                }

            }
            
            public async Task<string> GetSelectedText()
            {
                try
                {
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow == null) return null;
                    
                    // 通过JS获取选中的文本
                    string result = await mainWindow.webView2.ExecuteScriptAsync(@"
                        var select = document.getElementById('mySelect');
                        select ? select.options[select.selectedIndex].text : '请选择版本';
                    ");
                    
                    // 移除JSON引号
                    return result?.Trim('"') ?? "请选择版本";
                }
                catch
                {
                    return "请选择版本";
                }
            }
            public async Task<string> GetSelectedText(string SelectedText)
            {
                try
                {
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow == null) return null;
     
                    string result = await mainWindow.webView2.ExecuteScriptAsync(@"
                        var select = document.getElementById('"+SelectedText+@"');
                        select ? select.options[select.selectedIndex].text : '请选择界面样式';
                    ");
                    
                    return result?.Trim('"') ?? "error";
                }
                catch
                {
                    return "error";
                }
            }
            public async Task<string> GetSelectedText(string SelectedText,string Exclude)
                        {
                            try
                            {
                                var mainWindow = Application.Current.MainWindow as MainWindow;
                                if (mainWindow == null) return null;
                 
                                string result = await mainWindow.webView2.ExecuteScriptAsync(@"
                                    var select = document.getElementById('"+SelectedText+@"');
                                    select ? select.options[select.selectedIndex].text : '"+Exclude+@"';
                                ");
                                
                                return result?.Trim('"') ?? "error";
                            }
                            catch
                            {
                                return "error";
                            }
                        }

            public async Task SaveDataOfUsingHtml()
            { 
                string selectedText = await GetSelectedText("StyleSelect", "请选择界面样式");
                if (selectedText == "请选择界面样式")
                    return;
                string usingHtmlConfigJsonPath =
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "Config", "usingHtml.json");
                Directory.CreateDirectory(Path.GetDirectoryName(usingHtmlConfigJsonPath)!);
                var config = new usingHtmlConfigJsonData
                {
                    usingHtml = selectedText+".html"
                };
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(usingHtmlConfigJsonPath, json);

                Console.WriteLine("[INFO] 已保存界面样式: " + selectedText+".html");
                MessageBox.Show("请退出重进即可刷新", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                
                
            }
            
            public async Task ChangeLastedVersionContext()
            {
                try
                {
                    var loadWebsiteMinecraftVersion = new LoadWebsiteMinecraftVersion();
                    string HtmlJson = await loadWebsiteMinecraftVersion.LoadVersionManifest();
                    string lastedVersionString = await loadWebsiteMinecraftVersion.getLastestVerison(HtmlJson);

                    // ✅ 注意：这里不再需要获取MainWindow，因为已经在ScriptAPI内部
                    // 但我们需要访问webView2控件
        
                    // 由于ScriptAPI是独立类，我们需要一个方式来访问webView2
                    // 有几种解决方案：
        
                    // 方案1：传递MainWindow引用（推荐）
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow == null) return;
        
                    // 通过WebView2的ExecuteScriptAsync更新HTML
                    await mainWindow.Dispatcher.InvokeAsync(async () =>
                    {
                        try
                        {
                            await mainWindow.webView2.ExecuteScriptAsync(
                                $"document.getElementById('lastedVersion').innerText = '最新版:{lastedVersionString}';"
                            );
                        }
                        catch
                        {
                            // 忽略错误
                        }
                    });
                }
                catch
                {
                    // 忽略所有异常
                }
            }

            public void AutomaticInvocation()
            {
                ChangeLastedVersionContext();
                ChoiceVersion();
                styleList();
            }
             
            public async Task DownloadClientJar(string versionId)
            {
                try
                {
                    // 创建独立的下载进度窗口
                    var downloadWindow = new DownloadProgressWindow(versionId);
                    downloadWindow.Show();
                    
                    // 在后台线程中执行下载任务
                    await Task.Run(async () =>
                    {
                        try
                        {
                            var allTheVersion = new LoadWebsiteMinecraftVersion();
                            string manifestJson = await allTheVersion.LoadVersionManifest();
                            if (!string.IsNullOrWhiteSpace(manifestJson) && manifestJson.StartsWith("{"))
                            {
                                downloadWindow.UpdateStatus("版本清单验证成功");
                            }
                            downloadWindow.UpdateStatus("正在获取版本信息...");
                            string versionJsonUrl = await allTheVersion.getTheVerisonDownloadUrl(manifestJson, versionId);
                            
                            var versionInfo = new LoadWebsiteVersionInformation();
                            downloadWindow.UpdateStatus("正在获取JSON文件...");
                            string versionJson = await versionInfo.getVersionInformationJson(versionJsonUrl);
                            
                            downloadWindow.UpdateStatus("正在获取JAR下载地址...");
                            string jarUrl = await versionInfo.getClientVersionDownloadUrl(versionJson);
                            downloadWindow.UpdateStatus($"找到下载地址: {Path.GetFileName(jarUrl)}");
                            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                            string rootPath = Path.Combine(baseDir, "Minecraft", "VML", "Script");
                            string firstDir = Path.Combine(rootPath, versionId);
                            string secondDir = Path.Combine(firstDir, versionId);
                            string jsonDir = Path.Combine(secondDir, "libraries");
                            Directory.CreateDirectory(secondDir);
                            downloadWindow.UpdateStatus($"创建目录: {secondDir}");
                            string jarPath = Path.Combine(secondDir, $"{versionId}.jar");
                            string jsonPath = Path.Combine(secondDir, $"{versionId}.json");
                            string modsPath = Path.Combine(secondDir, "mods");
                            try
                            {
                                // 下载JAR文件并显示进度
                                downloadWindow.UpdateStatus("开始下载客户端文件...");
                                await DownloadFileWithProgressToWindow(jarUrl, jarPath, downloadWindow);
                                // 保存版本JSON
                                downloadWindow.UpdateStatus("保存版本信息文件...");
                                await File.WriteAllTextAsync(jsonPath, versionJson);
                                // 创建mods目录
                                Directory.CreateDirectory(modsPath);
                                
                                downloadWindow.UpdateStatus("正在下载 libraries...");
                                var libDownloader = new MinecraftLibraryDownloader();
                                await libDownloader.DownloadLibrariesAsync(jsonPath);
                                
                                downloadWindow.UpdateStatus("下载完成!", true);
                                
                                // 延迟2秒后关闭窗口
                                await Task.Delay(2000);
                                downloadWindow.Dispatcher.Invoke(() => downloadWindow.Close());
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"下载错误: {ex.Message}");
                                downloadWindow.UpdateStatus($"下载错误: {ex.Message}", false, true);
                                downloadWindow.SetErrorState();
                            }
                        }
                        catch (Exception ex)
                        {
                            downloadWindow.UpdateStatus($"下载过程出错: {ex.Message}", false, true);
                            downloadWindow.SetErrorState();
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FATAL ERROR] {ex.Message}");
                    Console.WriteLine($"[STACK TRACE] {ex.StackTrace}");
                }
            }

            private async Task DownloadFileWithProgressToWindow(string url, string outputPath, DownloadProgressWindow window)
{
    using (HttpClient client = new HttpClient())
    using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
    using (Stream stream = await response.Content.ReadAsStreamAsync())
    using (FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
    {
        long totalBytes = response.Content.Headers.ContentLength ?? -1;
        long downloadedBytes = 0;
        byte[] buffer = new byte[8192];
        int read;
        
        if (totalBytes != -1)
        {
            window.SetTotalSize(totalBytes);
        }

        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, read);
            downloadedBytes += read;

            if (totalBytes != -1)
            {
                double progress = (double)downloadedBytes / totalBytes;
                int percent = (int)(progress * 100);
                
                // 更新进度条
                window.UpdateProgress(percent, downloadedBytes, totalBytes);
                
                // 计算下载速度
                window.UpdateDownloadSpeed(downloadedBytes);
            }
        }
    }
}

            
            private async Task DownloadFileWithProgress(string url, string outputPath)
            {
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                using (Stream stream = await response.Content.ReadAsStreamAsync())
                using (FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    long totalBytes = response.Content.Headers.ContentLength ?? -1;
                    long downloadedBytes = 0;
                    byte[] buffer = new byte[8192];
                    int read;

                    Console.WriteLine($"[INFO] Total size: {totalBytes / 1024 / 1024:F2} MB");

                    while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, read);
                        downloadedBytes += read;

                        if (totalBytes != -1)
                        {
                            double progress = (double)downloadedBytes / totalBytes;
                            int percent = (int)(progress * 100);

                            // Simple progress bar display
                            Console.Write($"\r[PROGRESS] {percent}%  [");
                            Console.Write(new string('■', percent / 2));
                            Console.Write(new string(' ', 50 - percent / 2));
                            Console.Write("]");
                        }
                    }
                }
            }
            public void PrintLog(string Strings)
            {
                Console.WriteLine(Strings);
            }
        }
        
        private void LoadIcon()
        {
        }
    }
}