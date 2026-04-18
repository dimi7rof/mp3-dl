// Program.cs
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        while (true)
        {
            Console.Write("Enter url: ");
            var url = Console.ReadLine();

            var mp3dir = Path.Combine("E:", "mp3");
            var tools = Path.Combine("E:", "Tools");
            var ytdlp = Path.Combine(tools, "yt-dlp.exe");
            var ffmpeg = Path.Combine(tools, "ffmpeg.exe");

            RunProcess(ytdlp,
                $"--ffmpeg-location \"{ffmpeg}\" " +
                "-x --audio-format mp3" +
                " --restrict-filenames " +
                $"-o \"{mp3dir}\\%(upload_date)s - %(title)s.%(ext)s\" {url}");

            Console.WriteLine("Done!");
        }
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
            Console.WriteLine($"{fileName} failed with exit code {process.ExitCode}");
        }
    }
}