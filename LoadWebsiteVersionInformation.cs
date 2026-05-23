
using System.IO;
using System.Text.Json.Nodes;
using System.Net;

namespace Demo;
using System;
using System.Net.Http;
using System.Threading.Tasks;
public class LoadWebsiteVersionInformation
{
    public async Task<string> getVersionInformationJson(string url)
    {
        using HttpClient client = new HttpClient();

        try
        {
            string json = await client.GetStringAsync(url);
            return json;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"请求失败：{ex.Message}");
            return null; // 出错返回空
        }
    }
    private async Task<string> getVersionInFormation(string jsonContent, string type)
    {
        JsonNode root = JsonNode.Parse(jsonContent)!;

        // 2. 直接定位：downloads → client → url
        string InFrommation = root["downloads"]["client"][type].ToString();
        return InFrommation;
    }

    public async Task<string> getClientVersionDownloadUrl(string jsonContent)
    {
        string VersionUrl = await getVersionInFormation(jsonContent, "url");
        return VersionUrl;
    }
    public async Task<string> getClientVersionSha1(string jsonContent)
    {
        string VersionSha1 = await  getVersionInFormation(jsonContent, "sha1");
        return VersionSha1;
    }
    public async Task<string> getClientVersionSize(string jsonContent)
    {
        string VersionSha1 = await  getVersionInFormation(jsonContent, "size");
        return VersionSha1;
    }
    // 🔥 异步下载（必须用 async，否则界面卡死）
    public async Task DownloadJar(string filePath, string url)
    {
        try
        {
            // 确保目录存在
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Console.WriteLine("Is downloadling" + url);

            // 使用官方推荐的 HttpClient（.NET Core/.NET 5+ 标准）
            using (HttpClient client = new HttpClient())
            {
                // 模拟浏览器，防止被 Mojang 拦截
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

                // 下载文件
                using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    // 确保请求成功
                    response.EnsureSuccessStatusCode();

                    // 写入文件
                    using (Stream stream = await response.Content.ReadAsStreamAsync())
                    using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await stream.CopyToAsync(fs);
                    }
                }
            }

            Console.WriteLine("DownloadOK!" + filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ 下载失败：" + ex.Message);
        }
    }
}