using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Runtime.InteropServices;

namespace Demo;

/// <summary>
/// Helper class responsible for downloading libraries described in a Minecraft version JSON.
/// Public API is preserved: <see cref="DownloadLibrariesAsync(string)"/> remains unchanged.
/// </summary>
public class MinecraftLibraryDownloader
{
    private readonly HttpClient _http = new();

    public MinecraftLibraryDownloader()
    {
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        _http.Timeout = TimeSpan.FromMinutes(10);
    }

    /// <summary>
    /// Read the specified version JSON and ensure required libraries/natives are downloaded.
    /// This method keeps the original behavior: it swallows per-library exceptions so one failure
    /// does not abort the whole process.
    /// </summary>
    /// <param name="versionJsonPath">Path to a version JSON file produced by Mojang's manifests.</param>
    public async Task DownloadLibrariesAsync(string versionJsonPath)
    {
        if (string.IsNullOrWhiteSpace(versionJsonPath))
            throw new ArgumentException("versionJsonPath cannot be null or empty", nameof(versionJsonPath));

        string json = await File.ReadAllTextAsync(versionJsonPath);
        JsonNode root = JsonNode.Parse(json)!;

        string os = GetCurrentOS();
        string arch = GetCurrentArch();

        var libs = root["libraries"] as JsonArray;
        if (libs == null)
            return;

        foreach (JsonNode lib in libs)
        {
            try
            {
                if (!IsLibraryAllowed(lib, os, arch))
                    continue;

                await DownloadLibraryAsync(lib);
            }
            catch
            {
                // preserve original behavior: ignore single-library failures
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
                artifact["sha1"]?.ToString());
        }

        // natives
        if (lib["natives"] is JsonNode natives)
        {
            string os = GetCurrentOS();
            string? classifier = os switch
            {
                "windows" => natives["windows"]?.ToString(),
                "linux" => natives["linux"]?.ToString(),
                "osx" => natives["osx"]?.ToString(),
                _ => null
            };

            if (!string.IsNullOrEmpty(classifier)
                && lib["downloads"]?["classifiers"]?[classifier] is JsonNode native)
            {
                await SafeDownloadAsync(
                    native["url"]!.ToString(),
                    native["path"]!.ToString(),
                    native["sha1"]?.ToString());
            }
        }
    }

    private async Task SafeDownloadAsync(string url, string relativePath, string? expectedSha1)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string fullPath = Path.Combine(baseDir, "Minecraft", "VML", "libraries", relativePath);

        // already exists and checksum OK -> skip
        if (File.Exists(fullPath) && await VerifySha1Async(fullPath, expectedSha1))
            return;

        // try to delete any partial file
        TryDeleteFile(fullPath);

        string? dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        using (var resp = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
        {
            resp.EnsureSuccessStatusCode();
            await using var src = await resp.Content.ReadAsStreamAsync();
            await using var dst = File.Create(fullPath);
            await src.CopyToAsync(dst);
        }

        // small delay to reduce race with antivirus/OS
        await Task.Delay(300);

        if (!await VerifySha1Async(fullPath, expectedSha1))
        {
            TryDeleteFile(fullPath);
            throw new Exception("SHA1 verification failed");
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
            // best-effort, ignore errors
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
                return Convert.ToHexString(hash).ToLowerInvariant() == expected.ToLowerInvariant();
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
            string action = rule["action"]?.ToString() ?? string.Empty;

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

    private static string GetCurrentOS()
    {
        if (OperatingSystem.IsWindows()) return "windows";
        if (OperatingSystem.IsLinux()) return "linux";
        if (OperatingSystem.IsMacOS()) return "osx";
        return "unknown";
    }

    private static string GetCurrentArch()
    {
        return RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x86_64",
            Architecture.Arm64 => "arm64",
            Architecture.X86 => "x86",
            _ => "unknown",
        };
    }
}