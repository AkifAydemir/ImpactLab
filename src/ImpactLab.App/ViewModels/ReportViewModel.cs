using System.ComponentModel;
using ImpactLab.Core.Reporting;

namespace ImpactLab.App.ViewModels;

public sealed class ReportViewModel : INotifyPropertyChanged
{
    private EngineeringReport? _report;
    private string _markdown = "No report generated.";
    public EngineeringReport? Report
    {
        get => _report;
        private set
        {
            _report = value;
            PropertyChanged?.Invoke(this, new(nameof(Report)));
        }
    }
    public string Markdown
    {
        get => _markdown;
        private set
        {
            _markdown = value;
            PropertyChanged?.Invoke(this, new(nameof(Markdown)));
        }
    }

    public void Load(EngineeringReport report)
    {
        Report = report;
        Markdown = MarkdownReportWriter.Write(report);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
