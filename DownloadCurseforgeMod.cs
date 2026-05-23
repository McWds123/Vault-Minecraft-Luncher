using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Web;
using System.Threading.Tasks;

namespace Demo;

/// <summary>
/// 统一模组实体
/// </summary>
public class ModrinthModInfo
{
    /// <summary>模组名称</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>简介</summary>
    public string Summary { get; set; } = string.Empty;
    /// <summary>作者</summary>
    public string AuthorName { get; set; } = string.Empty;
    /// <summary>总下载量</summary>
    public long DownloadCount { get; set; }
    /// <summary>模组唯一标识</summary>
    public string Slug { get; set; } = string.Empty;
    /// <summary>模组主页地址</summary>
    public string HomeUrl => $"https://modrinth.com/mod/{Slug}";
}

/// <summary>
/// 搜索配置模型（面向对象配置）
/// </summary>
public class ModSearchOption
{
    /// <summary>MC游戏版本</summary>
    public string GameVersion { get; set; } = "1.20.1";
    /// <summary>加载器</summary>
    public string Loader { get; set; } = "fabric";
    /// <summary>每页数量</summary>
    public int PageSize { get; set; } = 30;
    /// <summary>排序字段</summary>
    public string SortField { get; set; } = "downloads";
    /// <summary>是否倒序(热度从高到低)</summary>
    public bool IsDesc { get; set; } = true;
}

/// <summary>
/// 核心爬虫服务类
/// </summary>
public class ModrinthModService
{
    private readonly HttpClient _httpClient;

    public ModrinthModService()
    {
        _httpClient = new HttpClient();
        // 统一请求头
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("DotNetModTool/2.0");
    }

    /// <summary>根据配置获取模组列表</summary>
    public async Task<List<ModrinthModInfo>> GetModListAsync(ModSearchOption option)
    {
        if (option == null) throw new ArgumentNullException(nameof(option));

        // 拼接筛选条件
        var facetArr = new List<string>
        {
            $"versions:{option.GameVersion}",
            $"loaders:{option.Loader}",
            "project_type:mod"
        };
        string facets = $"[[{string.Join(",", facetArr.Select(x => $"\"{x}\""))}]]";
        string encodeFacet = HttpUtility.UrlEncode(facets);

        string order = option.IsDesc ? "desc" : "asc";
        string apiUrl = $"https://api.modrinth.com/v2/search?limit={option.PageSize}&facets={encodeFacet}&sort={option.SortField}&order={order}";

        using var resp = await _httpClient.GetAsync(apiUrl);
        resp.EnsureSuccessStatusCode();
        string responseJson = await resp.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ModApiRoot>(responseJson);

        List<ModrinthModInfo> modList = new();
        if (result == null || result.Hits == null || result.Hits.Count == 0) return modList;

        foreach (var item in result.Hits)
        {
            modList.Add(new ModrinthModInfo
            {
                Name = item.title,
                Summary = item.description,
                AuthorName = item.author,
                DownloadCount = item.downloads,
                Slug = item.slug
            });
        }
        return modList;
    }

    // 释放资源
    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

// API返回根实体
internal class ModApiRoot
{
    public List<ModRawItem> Hits { get; set; } = new();
}

internal class ModRawItem
{
    public string title { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public string author { get; set; } = string.Empty;
    public long downloads { get; set; }
    public string slug { get; set; } = string.Empty;
}

// 你原本的业务调用类
public class DownloadCurseforgeMod
{
    private readonly ModrinthModService _modService;
    private readonly ModSearchOption _searchOption;

    // 构造函数依赖注入初始化
    public DownloadCurseforgeMod()
    {
        _modService = new ModrinthModService();
        // 初始化默认筛选：Fabric + 1.20.1 热度排序
        _searchOption = new ModSearchOption
        {
            GameVersion = "1.20.1",
            Loader = "fabric",
            SortField = "downloads",
            IsDesc = true
        };
    }

    /// <summary>获取并控制台输出模组列表</summary>
    public async void getModList()
    {
        try
        {
            List<ModrinthModInfo> modData = await _modService.GetModListAsync(_searchOption);
            Console.WriteLine("========== Fabric热门模组列表 ==========");
            foreach (var mod in modData)
            {
                Console.WriteLine($"模组名称：{mod.Name}");
                Console.WriteLine($"作者：{mod.AuthorName}");
                Console.WriteLine($"简介：{mod.Summary}");
                Console.WriteLine($"下载量：{mod.DownloadCount:N0}");
                Console.WriteLine($"模组地址：{mod.HomeUrl}");
                Console.WriteLine("--------------------------------------");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取模组失败：{ex.Message}");
        }
    }

    // 修改筛选版本
    public void SetGameVersion(string version)
    {
        _searchOption.GameVersion = version;
    }

    // 切换加载器
    public void SetLoader(string loader)
    {
        _searchOption.Loader = loader;
    }
}