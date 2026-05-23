using System.IO;
using System.Text.Json;

namespace Demo.Lib;
public class LoadStyle
{   
    public dynamic LoadStyles()
    {
        List<dynamic> returnList = new List<dynamic>();
        string fullPath = Path.Join("data", "style");
        string[] files = Directory.GetFiles(fullPath);
        foreach (var file in files) 
        {
            string name = Path.GetFileNameWithoutExtension(file);
            returnList.Add(name);
            Console.WriteLine(name);
        }
        return returnList;
    }

    public dynamic LoadUsingStyles()
    {
        string json = File.ReadAllText(Path.Join("data","Config","usingHtml.json"));
        usingHtmlJson UsingHtmlJson = JsonSerializer.Deserialize<usingHtmlJson>(json);
        Console.WriteLine("正在使用的html:" + Path.Join("data","style",UsingHtmlJson.usingHtml));
        return Path.GetFullPath(Path.Join("data","style",UsingHtmlJson.usingHtml));
    }
    private class usingHtmlJson
    {
        public string usingHtml { get; set; }
    }
}