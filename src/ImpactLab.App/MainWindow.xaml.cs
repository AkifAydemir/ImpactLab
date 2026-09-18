using System.Windows;
using ImpactLab.App.ViewModels;
using Microsoft.Win32;

namespace ImpactLab.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new ProductShellViewModel();
    }

    private void OpenScenario_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ProductShellViewModel shell)
            return;

        var dialog = new OpenFileDialog
        {
            Title = "Open ImpactLab scenario",
            Filter = "ImpactLab scenario (*.json)|*.json|All files (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false,
        };
        if (dialog.ShowDialog(this) == true)
            shell.OpenScenario(dialog.FileName);
    }
}
