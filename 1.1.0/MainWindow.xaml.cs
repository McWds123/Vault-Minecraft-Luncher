using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Automation;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace demo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>

public class ScriptConfig
{
    // 特性可选：指定与 JSON 中的字段名对应（若属性名与 JSON 字段名一致，可省略）
    [JsonPropertyName("usingJson")]
    public string UsingJson { get; set; } // 对应 JSON 中的 "usingJson" 字段

    // 注意：添加无参数构造函数（默认自动生成，若手动添加有参构造函数，需显式定义无参构造函数）
    public ScriptConfig()
    {
        // 初始化默认值，避免空引用异常
        UsingJson = string.Empty;
    }
}


public partial class MainWindow : Window
{
    public MainWindow()
    {
        Console.WriteLine("VML 1.1 is runs");
        InitializeComponent();
        Console.WriteLine("Debug:GUI finishes for init;");
        string minecraftFile = "script\\.minecraft";
        string scriptFile = "script";
        if (!Directory.Exists(scriptFile))
        {
            Console.WriteLine("Debug:There is no 'script' file [type=folder];(function = init)");
            Directory.CreateDirectory(scriptFile);
            using(File.Create(scriptFile + "\\script.bat")){ }
            using(File.Create(scriptFile + "\\script.cmd")){ }
            using(File.Create(scriptFile + "\\script.json")){ }
            using(File.Create(scriptFile+"\\READ ME.txt")){ }
            File.WriteAllText(scriptFile+"\\READ ME.txt","因为技术问题，请使用exe来当启动脚本，后续会开发bat,cmd的版本");
            File.WriteAllText(scriptFile+"\\script.json","{\n    \"usingJson\": \"script.exe\"\n}",Encoding.UTF8);
            Console.WriteLine("Debug:Create the 'script' is finishes [type=folder];(function = init)");
        }
        else
        {
            Console.WriteLine("Warning:The '.minecraft' and 'script' file is early creates [type=folder];(function = init)");
        }
        if (!Directory.Exists(minecraftFile))
        {
            Console.WriteLine("Debug:There is no '.minecraft' file [type=folder];(function = init)");
            Directory.CreateDirectory(minecraftFile);
            Console.WriteLine("Debug:Create the '.minecraft' is finishes [type=folder];(function = init)");
        }
        else
        {
            Console.WriteLine("Warning:The '.minecraft' and 'script' file is early creates [type=folder];(function = init)");
        }
    }
    private void RunGame(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("Debug: user clicks 'runGame' button [type = widget]; (function = RunGame)");
        Console.WriteLine("Debug: Reading json [type = json file];(function = RunGame)");
        string jsonFilePath = "script\\script.json";
        string jsonString = File.ReadAllText(jsonFilePath, Encoding.UTF8);
        var jsonOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        ScriptConfig scriptConfig = JsonSerializer.Deserialize<ScriptConfig>(jsonString, jsonOptions)!;
        string left = "{";
        string right = "}";
        Console.WriteLine($"Debug: Finish to read json [type = json file] {left}return content = {scriptConfig.UsingJson}{right} ");
        Console.WriteLine("Debug: running game! [type = window];(function = RunGame)");
        RunGameButton.Visibility = Visibility.Collapsed;
        Process.Start($"script\\{scriptConfig.UsingJson}");
    }

    private void DownloadMods_OnClick(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("Debug: User is clicks the downloadMods Button");
        RunGameButton.Visibility = Visibility.Collapsed;
        BackButton.Visibility = Visibility.Visible;
        DownloadModButton.Visibility = Visibility.Visible;
        ToBeOpenedLater.Visibility = Visibility.Visible;
    }

    private void BackScript(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("Debug: User is clicks the back button");
        RunGameButton.Visibility = Visibility.Visible;
        BackButton.Visibility = Visibility.Collapsed;
        DownloadModButton.Visibility = Visibility.Collapsed;
        ToBeOpenedLater.Visibility = Visibility.Collapsed;
    }

    private void DownloadModScript(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("Debug: User is clicks the download mod button");
    }
}