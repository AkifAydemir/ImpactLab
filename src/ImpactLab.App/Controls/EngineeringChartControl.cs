using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ImpactLab.Core.Visualization;

namespace ImpactLab.App.Controls;

public sealed class EngineeringChartControl : FrameworkElement
{
    public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register(
        nameof(Document),
        typeof(ChartDocument),
        typeof(EngineeringChartControl),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.AffectsRender,
            OnDocument
        )
    );
    public ChartDocument? Document
    {
        get => (ChartDocument?)GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }
    public ChartInteractionState Interaction { get; } = new();
    private Point? _panStart;
    private ChartViewport? _panViewport;

    public EngineeringChartControl()
    {
        ClipToBounds = true;
        Focusable = true;
        MouseWheel += Wheel;
        MouseLeftButtonDown += Down;
        MouseLeftButtonUp += Up;
        MouseMove += Move;
        Interaction.Changed += (_, _) => InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        dc.DrawRectangle(
            new SolidColorBrush(Color.FromRgb(18, 22, 28)),
            null,
            new Rect(RenderSize)
        );
        if (Document is null)
            return;
        var view = Interaction.Viewport ?? ChartViewport.Auto(Document);
        var plot = new Rect(58, 22, Math.Max(1, ActualWidth - 78), Math.Max(1, ActualHeight - 62));
        dc.DrawRectangle(null, new Pen(new SolidColorBrush(Color.FromRgb(70, 82, 98)), 1), plot);
        var palette = new[]
        {
            Colors.DeepSkyBlue,
            Colors.OrangeRed,
            Colors.LimeGreen,
            Colors.MediumPurple,
            Colors.Gold,
        };
        for (var si = 0; si < Document.Series.Count; si++)
        {
            var s = Document.Series[si];
            if (s.Points.Count < 2)
                continue;
            var geo = new StreamGeometry();
            using (var g = geo.Open())
            {
                for (var i = 0; i < s.Points.Count; i++)
                {
                    var p = Map(s.Points[i], view, plot);
                    if (i == 0)
                        g.BeginFigure(p, false, false);
                    else
                        g.LineTo(p, true, false);
                }
            }
            geo.Freeze();
            dc.DrawGeometry(
                null,
                new Pen(new SolidColorBrush(palette[si % palette.Length]), 1.4),
                geo
            );
        }
        if (Interaction.CursorX is double cx)
        {
            var x = plot.Left + (cx - view.XMin) / view.XSpan * plot.Width;
            dc.DrawLine(
                new Pen(Brushes.White, 1) { DashStyle = DashStyles.Dash },
                new Point(x, plot.Top),
                new Point(x, plot.Bottom)
            );
        }
    }

    private static Point Map(ChartPoint p, ChartViewport v, Rect r) =>
        new(
            r.Left + (p.X - v.XMin) / v.XSpan * r.Width,
            r.Bottom - (p.Y - v.YMin) / v.YSpan * r.Height
        );

    private static void OnDocument(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var c = (EngineeringChartControl)d;
        if (e.NewValue is ChartDocument doc)
            c.Interaction.Fit(doc);
    }

    private void Wheel(object s, MouseWheelEventArgs e)
    {
        if (Document is null)
            return;
        var v = Interaction.Viewport ?? ChartViewport.Auto(Document);
        var p = e.GetPosition(this);
        var plot = new Rect(58, 22, Math.Max(1, ActualWidth - 78), Math.Max(1, ActualHeight - 62));
        var x = v.XMin + (p.X - plot.Left) / plot.Width * v.XSpan;
        var y = v.YMax - (p.Y - plot.Top) / plot.Height * v.YSpan;
        Interaction.SetViewport(v.Zoom(x, y, e.Delta > 0 ? 0.8 : 1.25));
    }

    private void Down(object s, MouseButtonEventArgs e)
    {
        CaptureMouse();
        _panStart = e.GetPosition(this);
        _panViewport = Interaction.Viewport;
        if (e.ClickCount == 2 && Document is not null)
            Interaction.Fit(Document);
    }

    private void Up(object s, MouseButtonEventArgs e)
    {
        ReleaseMouseCapture();
        _panStart = null;
        _panViewport = null;
    }

    private void Move(object s, MouseEventArgs e)
    {
        if (_panStart is null || _panViewport is null || e.LeftButton != MouseButtonState.Pressed)
            return;
        var p = e.GetPosition(this);
        var dx = -(p.X - _panStart.Value.X) / Math.Max(ActualWidth, 1) * _panViewport.XSpan;
        var dy = (p.Y - _panStart.Value.Y) / Math.Max(ActualHeight, 1) * _panViewport.YSpan;
        Interaction.SetViewport(_panViewport.Pan(dx, dy));
    }
}
