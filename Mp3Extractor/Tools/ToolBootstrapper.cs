using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;

namespace Mp3Extractor.Tools;

public sealed record ToolPaths(string YtDlpPath, string ToolsDir);

public static class ToolBootstrapper
{
    private static readonly string ToolsDir =
        Path.Combine(AppContext.BaseDirectory, "tools");

    public static async Task<ToolPaths> EnsureAsync()
    {
        Directory.CreateDirectory(ToolsDir);

        var ytDlpPath = Path.Combine(ToolsDir, "yt-dlp.exe");
        var ffmpegPath = Path.Combine(ToolsDir, "ffmpeg.exe");

        if (!File.Exists(ytDlpPath))
        {
            Console.WriteLine("Downloading yt-dlp...");
            await DownloadFile(
                "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe",
                ytDlpPath);
        }

        if (!File.Exists(ffmpegPath))
        {
            Console.WriteLine("Downloading ffmpeg...");
            await DownloadAndExtractFfmpeg();
        }

        return new ToolPaths(ytDlpPath, ToolsDir);
    }

    private static async Task DownloadAndExtractFfmpeg()
    {
        var zipUrl =
            "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip";

        var zipPath = Path.Combine(ToolsDir, "ffmpeg.zip");

        await DownloadFile(zipUrl, zipPath);

        ZipFile.ExtractToDirectory(zipPath, ToolsDir, true);

        File.Delete(zipPath);

        var bin = Directory.GetDirectories(ToolsDir)
                           .SelectMany(d => Directory.GetFiles(d, "ffmpeg.exe", SearchOption.AllDirectories))
                           .FirstOrDefault();

        if (bin != null)
        {
            File.Move(bin, Path.Combine(ToolsDir, "ffmpeg.exe"), true);
        }
    }

    private static async Task DownloadFile(string url, string path)
    {
        using var http = new HttpClient();
        var data = await http.GetByteArrayAsync(url);
        await File.WriteAllBytesAsync(path, data);
    }
}