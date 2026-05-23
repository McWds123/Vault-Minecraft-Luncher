using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Runtime.InteropServices;

namespace Demo;

public class MinecraftLibraryDownloader
{
    private readonly HttpClient _http = new();

    public MinecraftLibraryDownloader()
    {
        _http.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        _http.Timeout = TimeSpan.FromMinutes(10);
    }

    public async Task DownloadLibrariesAsync(string versionJsonPath)
    {
        string json = await File.ReadAllTextAsync(versionJsonPath);
        JsonNode root = JsonNode.Parse(json)!;

        string os = GetCurrentOS();
        string arch = GetCurrentArch();

        foreach (JsonNode lib in root["libraries"]!.AsArray())
        {
            try
            {
                if (!IsLibraryAllowed(lib, os, arch))
                    continue;

                await DownloadLibraryAsync(lib);
            }
            catch
            {
                // ✅ 单个库失败不影响整体
            }
        }
    }

    private async Task DownloadLibraryAsync(JsonNode lib)
    {
        // artifact
        if (lib["downloads"]?["artifact"] is JsonNode artifact)
        {
            await SafeDownloadAsync(
                artifact["url"]!.ToString(),
                artifact["path"]!.ToString(),
                artifact["sha1"]?.ToString()
            );
        }

        // natives
        if (lib["natives"] is JsonNode natives)
        {
            string os = GetCurrentOS();
            string classifier = os switch
            {
                "windows" => natives["windows"]?.ToString(),
                "linux" => natives["linux"]?.ToString(),
                "osx" => natives["osx"]?.ToString(),
                _ => null
            };

            if (classifier != null &&
                lib["downloads"]?["classifiers"]?[classifier] is JsonNode native)
            {
                await SafeDownloadAsync(
                    native["url"]!.ToString(),
                    native["path"]!.ToString(),
                    native["sha1"]?.ToString()
                );
            }
        }
    }

    private async Task SafeDownloadAsync(string url, string relativePath, string? expectedSha1)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string fullPath = Path.Combine(baseDir, "Minecraft", "VML", "libraries", relativePath);

        // ✅ 已存在且校验通过，直接跳过
        if (File.Exists(fullPath) && await VerifySha1Async(fullPath, expectedSha1))
            return;

        // ✅ 强制释放占用
        TryDeleteFile(fullPath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        // ✅ 下载
        using (var resp = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
        {
            resp.EnsureSuccessStatusCode();
            await using var src = await resp.Content.ReadAsStreamAsync();
            await using var dst = File.Create(fullPath);
            await src.CopyToAsync(dst);
        }

        // ✅ 关键：让杀毒软件和 Windows 喘口气
        await Task.Delay(300);

        if (!await VerifySha1Async(fullPath, expectedSha1))
        {
            TryDeleteFile(fullPath);
            throw new Exception("SHA1 校验失败");
        }
    }

    private void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // ignore
        }
    }

    private async Task<bool> VerifySha1Async(string path, string? expected)
    {
        if (string.IsNullOrEmpty(expected))
            return true;

        for (int i = 0; i < 3; i++)
        {
            try
            {
                using var sha1 = SHA1.Create();
                await using var fs = File.OpenRead(path);
                byte[] hash = await sha1.ComputeHashAsync(fs);
                return Convert.ToHexString(hash).ToLower() == expected.ToLower();
            }
            catch
            {
                await Task.Delay(300);
            }
        }
        return false;
    }

    private bool IsLibraryAllowed(JsonNode lib, string os, string arch)
    {
        if (lib["rules"] is not JsonArray rules)
            return true;

        bool allow = false;

        foreach (JsonNode rule in rules)
        {
            string action = rule["action"]!.ToString();

            if (rule["os"] is JsonNode osRule)
            {
                if (osRule["name"]?.ToString() == os)
                {
                    if (osRule["arch"] == null || osRule["arch"]!.ToString() == arch)
                        allow = action == "allow";
                }
            }
            else
            {
                allow = action == "allow";
            }
        }

        return allow;
    }

    private string GetCurrentOS()
    {
        if (OperatingSystem.IsWindows()) return "windows";
        if (OperatingSystem.IsLinux()) return "linux";
        if (OperatingSystem.IsMacOS()) return "osx";
        return "unknown";
    }

    private string GetCurrentArch()
    {
        return RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x86_64",
            Architecture.Arm64 => "arm64",
            Architecture.X86 => "x86",
            _ => "unknown"
        };
    }
}