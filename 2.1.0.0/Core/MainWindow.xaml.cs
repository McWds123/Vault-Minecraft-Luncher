using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Demo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadHtml();
            LoadIcon();
        }

        private void LoadHtml()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "index.html");
            string html = File.ReadAllText(path);

            webBrowser.NavigateToString(html);
            webBrowser.ObjectForScripting = new ScriptAPI(); // <-- 绑定C#
        }
        private void LoadIcon()
        {
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.png");
                if (File.Exists(iconPath))
                {
                    BitmapImage icon = new BitmapImage();
                    icon.BeginInit();
                    icon.UriSource = new Uri(iconPath, UriKind.Absolute);
                    icon.CacheOption = BitmapCacheOption.OnLoad;
                    icon.EndInit();
                    this.Icon = icon;
                }
            }
            catch { }
        }
    }

    [System.Runtime.InteropServices.ComVisible(true)]
    public class ScriptAPI
    {

        public void RunGameButton()
        {
            if (GetSelectedText() == "请选择版本")
            {
                MessageBox.Show("请选择版本","警告", (MessageBoxButton.OK),MessageBoxImage.Warning);
            }
            else
            {
                RunGame(GetSelectedText());   
            }
        }
        public void RunGame(string VersionName)
        {
            System.Diagnostics.Process.Start($@"Minecraft\VML\Script\{VersionName}\Run.bat");
        }
        public void ChoiceVersion()
        {
            try
            {
                Console.WriteLine("[Log] Content: ChoiceVersion function run start");

                // 获取主窗口
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow == null)
                {
                    Console.WriteLine("[Error] Content: no find main window");
                    return;
                }

                Console.WriteLine("[Debug] Content: find webBrowser control success");

                var webBrowser = mainWindow.webBrowser;
                if (webBrowser == null)
                {
                    Console.WriteLine("[Error] Content: no find webBrowser control");
                    return;
                }

                // 获取网页文档
                dynamic doc = webBrowser.Document;
                if (doc == null)
                {
                    Console.WriteLine("[Error] Content: HTML page no load finish");
                    return;
                }

                Console.WriteLine("[Debug] Content: get HTML DOM success");

                // 获取下拉框
                dynamic select = doc.getElementById("mySelect");
                if (select == null)
                {
                    Console.WriteLine("[Error] Content: no find mySelect select box");
                    return;
                }

                Console.WriteLine("[Log] Content: find mySelect box success");

                // 拼接路径
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                Console.WriteLine("[Debug] Content: current program path is " + basePath);

                string targetPath = Path.Combine(basePath, "Minecraft", "VML", "Script");
                Console.WriteLine("[Log] Content: now read folder " + targetPath);

                // 读取文件夹
                string[] folderPaths = Directory.GetDirectories(targetPath);
                Console.WriteLine("[Log] Content: find total " + folderPaths.Length + " version");

                // 清空原有选项
                select.options.length = 0;
                AddOption(select, "", "请选择版本");

                // 遍历添加
                foreach (string path in folderPaths)
                {
                    string folderName = Path.GetFileName(path);
                    AddOption(select, folderName, folderName);
                    Console.WriteLine("[Log] Content: add version item -> " + folderName + " success");
                }

                Console.WriteLine("[Log] Content: all version load finish");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Error] Content: load version fail, reason: " + ex.Message);
            }
        }
        private void AddOption(dynamic select, string value, string text)
        {
            try
            {
                dynamic option = select.ownerDocument.createElement("option");
                option.value = value;
                option.text = text;
                select.options.add(option);
            }
            catch
            {
                Console.WriteLine("[Error] Content: add option item fail -> " + text);
            }
        }
        public string GetSelectedText()
        {
            try
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow == null) return null;

                // 获取文档
                dynamic doc = mainWindow.webBrowser.Document;
                object select = doc.getElementById("mySelect");

                // 关键：用 InvokeMember 读取属性，避开 dynamic 坑
                int selectedIndex = (int)select.GetType().InvokeMember(
                    "selectedIndex",
                    System.Reflection.BindingFlags.GetProperty,
                    null,
                    select,
                    null);

                object options = select.GetType().InvokeMember(
                    "options",
                    System.Reflection.BindingFlags.GetProperty,
                    null,
                    select,
                    null);

                object option = options.GetType().InvokeMember(
                    "item",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null,
                    options,
                    new object[] { selectedIndex });

                // 拿到显示文本！
                string text = (string)option.GetType().InvokeMember(
                    "text",
                    System.Reflection.BindingFlags.GetProperty,
                    null,
                    option,
                    null);

                return text;
            }
            catch
            {
                return null;
            }
        }
    }
}