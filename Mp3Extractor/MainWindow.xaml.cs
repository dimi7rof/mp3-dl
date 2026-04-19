using System.Windows;
using System.Windows.Controls;
using Mp3Extractor.ViewModels;

namespace Mp3Extractor;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        
        // Enable auto-scroll for logs
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainWindowViewModel.ProgressLog))
                {
                    Dispatcher.Invoke(() => LogTextBox.ScrollToEnd());
                }
            };
        }
    }
}
