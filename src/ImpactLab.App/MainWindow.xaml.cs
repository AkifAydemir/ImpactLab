using System.Windows;
using ImpactLab.App.ViewModels;
using Microsoft.Win32;

namespace ImpactLab.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var workArea = SystemParameters.WorkArea;
        Width = Math.Min(Width, workArea.Width);
        Height = Math.Min(Height, workArea.Height);
        MaxHeight = workArea.Height;
        DataContext = new ProductShellViewModel();
    }

    private void OpenScenario_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ProductShellViewModel shell)
            return;

        var dialog = new OpenFileDialog
        {
            Title = "Open ImpactLab scenario",
            Filter =
                "ImpactLab project (*.impactlab)|*.impactlab|ImpactLab scenario (*.json)|*.json|All files (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false,
        };
        if (dialog.ShowDialog(this) == true)
            shell.OpenScenario(dialog.FileName);
    }

    private void SaveProject_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ProductShellViewModel shell)
            return;
        var dialog = new SaveFileDialog
        {
            Title = "Save portable ImpactLab project",
            Filter = "ImpactLab project (*.impactlab)|*.impactlab",
            DefaultExt = ".impactlab",
            AddExtension = true,
        };
        if (dialog.ShowDialog(this) != true)
            return;
        try
        {
            shell.SaveProject(dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Project save failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void ExportResultsCsv_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ProductShellViewModel shell)
            return;
        var dialog = new SaveFileDialog
        {
            Title = "Export detailed simulation results",
            Filter = "CSV data (*.csv)|*.csv",
            DefaultExt = ".csv",
            AddExtension = true,
        };
        if (dialog.ShowDialog(this) != true)
            return;
        try
        {
            shell.ExportResultsCsv(dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "CSV export failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void ExportReport_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ProductShellViewModel shell)
            return;
        var dialog = new SaveFileDialog
        {
            Title = "Export engineering report",
            Filter =
                "HTML report (*.html)|*.html|Markdown report (*.md)|*.md|JSON report (*.json)|*.json",
            DefaultExt = ".html",
            AddExtension = true,
        };
        if (dialog.ShowDialog(this) != true)
            return;
        try
        {
            shell.ExportEngineeringReport(dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Report export failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void ExportAnimation_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ProductShellViewModel shell)
            return;
        var dialog = new SaveFileDialog
        {
            Title = "Export simulation animation",
            Filter = "MP4 video (*.mp4)|*.mp4|Animated GIF (*.gif)|*.gif",
            DefaultExt = ".mp4",
            AddExtension = true,
        };
        if (dialog.ShowDialog(this) != true)
            return;
        try
        {
            shell.ExportSimulationAnimation(dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Animation export failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void ImportMesh_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ProductShellViewModel shell)
            return;
        var dialog = new OpenFileDialog
        {
            Title = "Import mesh",
            Filter = "Mesh files (*.stl;*.obj)|*.stl;*.obj|All files (*.*)|*.*",
        };
        if (dialog.ShowDialog(this) != true)
            return;
        shell.Import.Path = dialog.FileName;
        try
        {
            shell.Import.Import();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Mesh import failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void AddAssemblyParts_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ProductShellViewModel shell)
            return;
        var dialog = new OpenFileDialog
        {
            Title = "Add impact assembly parts",
            Filter = "Mesh files (*.stl;*.obj)|*.stl;*.obj|All files (*.*)|*.*",
            Multiselect = true,
        };
        if (dialog.ShowDialog(this) != true)
            return;
        try
        {
            shell.Assembly.AddFiles(dialog.FileNames);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Assembly import failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void RemoveAssemblyPart_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProductShellViewModel shell)
            shell.Assembly.RemoveSelected();
    }

    private void MarkAssemblyImpactor_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProductShellViewModel shell)
            shell.Assembly.MarkSelectedAsImpactor();
    }

    private void BuildAssemblyScenario_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ProductShellViewModel shell)
            return;
        try
        {
            shell.UseAssemblyAsScenario();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Assembly setup failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void SplitStl_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ProductShellViewModel shell)
            return;
        var dialog = new OpenFolderDialog { Title = "Choose the STL layer output directory" };
        if (dialog.ShowDialog(this) != true)
            return;
        try
        {
            shell.Import.ExportLayers(dialog.FolderName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "STL layer export failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void UseImportedTarget_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ProductShellViewModel shell)
            return;
        try
        {
            shell.UseImportedMeshAsImpactTarget();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Target setup failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void ExportMobilityDataset_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ProductShellViewModel shell)
            return;
        var dialog = new SaveFileDialog
        {
            Title = "Export TensorFlow-ready traversability dataset",
            Filter = "CSV dataset (*.csv)|*.csv",
            DefaultExt = ".csv",
            AddExtension = true,
        };
        if (dialog.ShowDialog(this) != true)
            return;
        try
        {
            shell.Mobility.ExportTensorFlowDataset(dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Dataset export failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }
}
