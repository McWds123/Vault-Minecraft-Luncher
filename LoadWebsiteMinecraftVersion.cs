using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace Demo;
using System;
using System.Net.Http;
using System.Threading.Tasks;

public class LoadWebsiteMinecraftVersion
{
    // 正确：异步方法获取 Minecraft 版本清单
    public async Task<string> LoadVersionManifest()
    {
        using HttpClient client = new HttpClient();

        try
        {
            string url = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
            string json = await client.GetStringAsync(url);
            return json;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"请求失败：{ex.Message}");
            return null; // 出错返回空
        }
    }
    public async Task<string> getLastestVerison(string jsonContent)
    {
        string json = jsonContent;
        AllTheVersionMainfestStructure root = JsonSerializer.Deserialize<AllTheVersionMainfestStructure>(json);
        return root.latest.release;
    }
    public async Task<List<string>> getVersionList(string jsonContent)
    {
        // 用来存储所有 id 的列表
        List<string> idList = new List<string>();

        JsonNode root = JsonNode.Parse(jsonContent)!;

        // 获取 versions 数组
        JsonArray array = root["versions"].AsArray();

        // 遍历所有版本对象
        foreach (JsonObject item in array)
        {
            if (item.TryGetPropertyValue("id", out JsonNode idNode))
            {
                string idValue = idNode.ToString();
                idList.Add(idValue);
            }
        }
        return idList;
    }
    public async Task<string> getTheVersionID(string versionName)
    {
        string json = await LoadVersionManifest();
        List<string> allIds = await getVersionList(json);
        
        string findId = allIds.FirstOrDefault(id => id == versionName);

        if (findId == null)
        {
            Console.WriteLine("未找到该版本！");
            return null;
        }

        return findId;
    }

    private async Task<string> getTheVerisonInformation(string jsonContent, string versionID, string type)
    {
        JsonNode root = JsonNode.Parse(jsonContent)!;
        JsonArray array = root["versions"].AsArray();

        foreach (JsonObject item in array)
        {
            // 匹配传入的版本ID
            if (item["id"]?.ToString() == versionID)
            {
                string targetUrl = item[type].ToString();
                return targetUrl;
            }
        }
        Console.WriteLine($"未找到版本：{versionID}");
        return null;
    }
    public async Task<string> getTheVerisonDownloadUrl(string jsonContent, string versionID)
    {
        JsonNode root = JsonNode.Parse(jsonContent)!;
        JsonArray array = root["versions"].AsArray();

        foreach (JsonObject item in array)
        {
            // 匹配传入的版本ID
            if (item["id"]?.ToString() == versionID)
            {
                string targetUrl = item["url"].ToString();
                return targetUrl; // 找到直接返回
            }
        }
        Console.WriteLine($"未找到版本：{versionID}");
        return null;
    }
    public async Task<string> getTheVersionType(string jsonContent, string versionID)
    {
        string TypeSting =  await getTheVerisonInformation(jsonContent, versionID, "type");
        return TypeSting;
    }
    public async Task<string> getTheVerisonTime(string jsonContent, string versionID)
    {
        string VersionTime = await getTheVerisonInformation(jsonContent,versionID,"time");
        return VersionTime;
    }
    public async Task<string> getTheVersionReleaseTime(string jsonContent, string verisonID)
    {
        string VerusibReleaseTime = await getTheVerisonInformation(jsonContent, verisonID, "releaseTime");
        return VerusibReleaseTime;
    }
}