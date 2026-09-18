using System.ComponentModel;

namespace ImpactLab.App.Visualization;

public sealed class TimelineCursorService : INotifyPropertyChanged
{
    private int _frame;
    public int Frame
    {
        get => _frame;
        set
        {
            if (_frame == value)
                return;
            _frame = value;
            PropertyChanged?.Invoke(this, new(nameof(Frame)));
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
}
