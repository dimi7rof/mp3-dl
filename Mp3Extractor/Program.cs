using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

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

                var tools = Path.Combine("E:", "Tools");
                var ytdlp = Path.Combine(tools, "yt-dlp.exe");
                var ffmpeg = Path.Combine(tools, "ffmpeg.exe");

                var title = await GetTitleAsync(url!);

                var safeTitle = string.Concat(title!.Split(Path.GetInvalidFileNameChars()));

                var latinTitle = CyrillicToLatin(safeTitle);

                var outputPath = Path.Combine(outputDir, $"{latinTitle}.mp3");

                Print($"{latinTitle}.mp3", ConsoleColor.Yellow);

                RunProcess(ytdlp,
                    $"--ffmpeg-location \"{ffmpeg}\" " +
                    "-x --audio-format mp3 " +
                    $"-o \"{outputPath}\" {url}");

                PrintLine(" Downloaded!", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                PrintLine(ex.Message, ConsoleColor.Red);
            }
        }
    }

    static string CyrillicToLatin(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var map = new Dictionary<char, string>
        {
            // Uppercase
            ['А'] = "A",
            ['Б'] = "B",
            ['В'] = "V",
            ['Г'] = "G",
            ['Д'] = "D",
            ['Е'] = "E",
            ['Ж'] = "Zh",
            ['З'] = "Z",
            ['И'] = "I",
            ['Й'] = "Y",
            ['К'] = "K",
            ['Л'] = "L",
            ['М'] = "M",
            ['Н'] = "N",
            ['О'] = "O",
            ['П'] = "P",
            ['Р'] = "R",
            ['С'] = "S",
            ['Т'] = "T",
            ['У'] = "U",
            ['Ф'] = "F",
            ['Х'] = "H",
            ['Ц'] = "Ts",
            ['Ч'] = "Ch",
            ['Ш'] = "Sh",
            ['Щ'] = "Sht",
            ['Ъ'] = "A",
            ['Ь'] = "",
            ['Ю'] = "Yu",
            ['Я'] = "Ya",

            // Lowercase
            ['а'] = "a",
            ['б'] = "b",
            ['в'] = "v",
            ['г'] = "g",
            ['д'] = "d",
            ['е'] = "e",
            ['ж'] = "zh",
            ['з'] = "z",
            ['и'] = "i",
            ['й'] = "y",
            ['к'] = "k",
            ['л'] = "l",
            ['м'] = "m",
            ['н'] = "n",
            ['о'] = "o",
            ['п'] = "p",
            ['р'] = "r",
            ['с'] = "s",
            ['т'] = "t",
            ['у'] = "u",
            ['ф'] = "f",
            ['х'] = "h",
            ['ц'] = "ts",
            ['ч'] = "ch",
            ['ш'] = "sh",
            ['щ'] = "sht",
            ['ъ'] = "a",
            ['ь'] = "",
            ['ю'] = "yu",
            ['я'] = "ya"
        };

        var result = new StringBuilder();

        foreach (var ch in text)
        {
            if (map.TryGetValue(ch, out var latin))
                result.Append(latin);
            else
                result.Append(ch); // keep symbols, numbers, spaces
        }

        return result.ToString();
    }

    static void Print(string text, ConsoleColor? color = null)
    {
        if (color.HasValue)
            Console.ForegroundColor = color.Value;

        Console.Write(text);

        if (color.HasValue)
            Console.ResetColor();
    }

    static void PrintLine(string text, ConsoleColor? color = null)
    {
        if (color.HasValue)
            Console.ForegroundColor = color.Value;

        Console.WriteLine(text);

        if (color.HasValue)
            Console.ResetColor();
    }

    static async Task<string?> GetTitleAsync(string url)
    {
        using var http = new HttpClient();

        // Some sites (like YouTube) require a User-Agent
        http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

        var html = await http.GetStringAsync(url);

        var match = Regex.Match(html, @"<title>\s*(.+?)\s*</title>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (!match.Success)
            return null;

        var title = match.Groups[1].Value;

        // YouTube titles often end with " - YouTube"
        title = title.Replace(" - YouTube", "").Trim();

        return System.Net.WebUtility.HtmlDecode(title);
    }

    static void RunProcess(string fileName, string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            PrintLine($"Error: {fileName} exited with code {process.ExitCode}", ConsoleColor.Red);
        }
    }
}