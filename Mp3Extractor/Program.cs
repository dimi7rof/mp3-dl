using Mp3Extractor.Tools;
using static Mp3Extractor.Utilities.Printer;
using static Mp3Extractor.Core.ProcessRunner;
using static Mp3Extractor.Utilities.TitleService;

class Program
{
    static async Task Main(string[] args)
    {
        Print("Enter output directory: ", ConsoleColor.Magenta);

        var outputDir = Console.ReadLine() ?? Path.Combine("E:", "mp3");
        if (!Directory.Exists(outputDir))
        {
            PrintLine($"Directory '{outputDir}' does not exist. Creating it...");
            Directory.CreateDirectory(outputDir);
        }

        while (true)
        {
            try
            {
                Print("Enter url: ", ConsoleColor.Magenta);
                var url = Console.ReadLine();
                url = url!.Split("&list=").FirstOrDefault();

                var tools = await ToolBootstrapper.EnsureAsync();

                var title = await GetTitleAsync(url!);                

                var outputPath = Path.Combine(outputDir, $"{title}.mp3");

                Print($"{title}.mp3", ConsoleColor.Yellow);

                Run(tools.YtDlpPath, $"--ffmpeg-location \"{tools.ToolsDir}\" -x --audio-format mp3 -o \"{outputPath}\" {url}");

                PrintLine(" Downloaded!", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                PrintLine(ex.Message, ConsoleColor.Red);
            }
        }
    }
}