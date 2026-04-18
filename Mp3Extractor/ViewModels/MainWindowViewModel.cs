using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Mp3Extractor.Core;
using Mp3Extractor.Tools;
using Mp3Extractor.Utilities;

namespace Mp3Extractor.ViewModels;

public class MainWindowViewModel : NotifyPropertyChangedBase
{
    private string _outputDirectory;
    private string _url = "";
    private string _title = "";
    private string _status = "Ready";
    private bool _isProcessing;
    private string _progressLog = "";
    private ToolPaths? _tools;

    public string OutputDirectory
    {
        get => _outputDirectory;
        set
        {
            if (SetProperty(ref _outputDirectory, value))
            {
                ValidateOutputDirectory();
            }
        }
    }

    public string Url
    {
        get => _url;
        set => SetProperty(ref _url, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        set => SetProperty(ref _isProcessing, value);
    }

    public string ProgressLog
    {
        get => _progressLog;
        set => SetProperty(ref _progressLog, value);
    }

    public ICommand BrowseCommand { get; }
    public ICommand DownloadCommand { get; }
    public ICommand GetTitleCommand { get; }
    public ICommand ClearLogCommand { get; }
    public ICommand PasteCommand { get; }

    public MainWindowViewModel()
    {
        _outputDirectory = Path.Combine("E:", "mp3");
        
        BrowseCommand = new RelayCommand(_ => BrowseForFolder());
        DownloadCommand = new AsyncRelayCommand(async _ => await DownloadAsync(), _ => !IsProcessing && !string.IsNullOrWhiteSpace(Url));
        GetTitleCommand = new AsyncRelayCommand(async _ => await GetTitleAsync(), _ => !IsProcessing && !string.IsNullOrWhiteSpace(Url));
        ClearLogCommand = new RelayCommand(_ => ProgressLog = "");
        PasteCommand = new RelayCommand(_ => PasteFromClipboard(), _ => !IsProcessing);

        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        try
        {
            Status = "Initializing tools...";
            _tools = await ToolBootstrapper.EnsureAsync();
            Status = "Ready";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
            AddLog($"ERROR: {ex.Message}");
        }
    }

    private void ValidateOutputDirectory()
    {
        if (!Directory.Exists(OutputDirectory))
        {
            try
            {
                Directory.CreateDirectory(OutputDirectory);
            }
            catch (Exception ex)
            {
                AddLog($"ERROR: Could not create directory: {ex.Message}");
            }
        }
    }

    private void BrowseForFolder()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select output directory",
            SelectedPath = OutputDirectory,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            OutputDirectory = dialog.SelectedPath;
        }
    }

    private async Task GetTitleAsync()
    {
        if (string.IsNullOrWhiteSpace(Url) || _tools == null)
            return;

        try
        {
            IsProcessing = true;
            Status = "Fetching title...";
            AddLog($"Fetching title for: {Url}");

            var url = Url.Split("&list=").FirstOrDefault() ?? Url;
            var title = await TitleService.GetTitleAsync(url);

            if (string.IsNullOrEmpty(title))
            {
                Status = "Could not fetch title";
                AddLog("ERROR: Could not fetch title from URL");
            }
            else
            {
                Title = title;
                Status = $"Title: {title}";
                AddLog($"Title fetched: {title}");
            }
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
            AddLog($"ERROR: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private async Task DownloadAsync()
    {
        if (string.IsNullOrWhiteSpace(Url) || _tools == null)
            return;

        try
        {
            IsProcessing = true;
            
            var url = Url.Split("&list=").FirstOrDefault() ?? Url;
            var title = string.IsNullOrWhiteSpace(Title) ? await TitleService.GetTitleAsync(url) : Title;

            if (string.IsNullOrEmpty(title))
            {
                Status = "Could not determine title for download";
                AddLog("ERROR: Could not determine title");
                return;
            }

            Title = title;
            var outputPath = Path.Combine(OutputDirectory, $"{title}.mp3");

            // Check if MP3 file already exists
            if (File.Exists(outputPath))
            {
                Status = "File already exists!";
                AddLog($"File already exists: {title}.mp3");
                Url = "";
                Title = "";
                return;
            }

            Status = "Downloading...";
            AddLog($"Starting download: {title}.mp3");

            var args = $"--ffmpeg-location \"{_tools.ToolsDir}\" -x --audio-format mp3 -o \"{outputPath}\" {url}";
            var exitCode = await ProcessRunner.RunAsync(_tools.YtDlpPath, args, AddLog);

            if (exitCode == 0)
            {
                Status = "Download completed successfully!";
                AddLog($"SUCCESS: {title}.mp3 downloaded!");
                Url = "";
                Title = "";
            }
            else
            {
                Status = $"Download failed with exit code {exitCode}";
                AddLog($"ERROR: Download failed with exit code {exitCode}");
            }
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
            AddLog($"ERROR: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private void AddLog(string message)
    {
        ProgressLog += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
    }

    private void PasteFromClipboard()
    {
        try
        {
            if (Clipboard.ContainsText())
            {
                var clipboardText = Clipboard.GetText().Trim();
                if (!string.IsNullOrEmpty(clipboardText))
                {
                    Url = clipboardText;
                    AddLog($"Pasted URL from clipboard");
                }
            }
        }
        catch (Exception ex)
        {
            AddLog($"ERROR: Failed to paste from clipboard: {ex.Message}");
        }
    }
}
