using System.Windows;
using Mp3Extractor.ViewModels;

namespace Mp3Extractor;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}
