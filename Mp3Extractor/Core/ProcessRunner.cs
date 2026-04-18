using System.Diagnostics;
using static Mp3Extractor.Utilities.Printer;

namespace Mp3Extractor.Core;

public static class ProcessRunner
{
    public static void Run(string file, string args)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = file,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // While the process is running, print a dot every second to show progress.
        while (!process.WaitForExit(1000))
        {
            Print(".", ConsoleColor.DarkGray);
        }

        // Ensure asynchronous handlers have finished processing.
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            PrintLine($"Error: {file} exited with code {process.ExitCode}", ConsoleColor.Red);
        }
    }

    public static async Task<int> RunAsync(string file, string args, Action<string>? onOutput = null)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = file,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.OutputDataReceived += (sender, args) => 
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                onOutput?.Invoke(args.Data);
            }
        };

        process.ErrorDataReceived += (sender, args) => 
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                onOutput?.Invoke($"ERROR: {args.Data}");
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return process.ExitCode;
    }
}