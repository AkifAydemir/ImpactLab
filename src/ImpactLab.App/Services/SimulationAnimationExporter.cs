using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using ImpactLab.App.Rendering;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Rigid;
using ImpactLab.Core.Simulation;

namespace ImpactLab.App.Services;

public static class SimulationAnimationExporter
{
    private static readonly JsonSerializerOptions TimelineJson = new() { WriteIndented = true };

    public static void ExportGif(
        string path,
        SimulationRunBundle run,
        HeatmapMode heatmap = HeatmapMode.Damage,
        int width = 960,
        int height = 540,
        int framesPerSecond = 20
    )
    {
        if (run.Result.Frames.Count == 0)
            throw new InvalidOperationException("The simulation contains no frames to animate.");
        if (width < 320 || height < 240)
            throw new ArgumentOutOfRangeException(nameof(width));

        var encoder = new GifBitmapEncoder();
        var displayMaximum = GlobalMaximum(run, heatmap);
        foreach (var frame in run.Result.Frames)
        {
            var bitmap = RenderFrame(
                run.Compiled.Mesh.Nodes.Select(x => x.RestPosition).ToArray(),
                run.Compiled.RigidBodies,
                run,
                frame,
                heatmap,
                width,
                height,
                displayMaximum
            );
            var metadata = new BitmapMetadata("gif");
            metadata.SetQuery("/grctlext/Delay", (ushort)Math.Max(1, 100 / framesPerSecond));
            metadata.SetQuery("/grctlext/Disposal", (byte)2);
            encoder.Frames.Add(BitmapFrame.Create(bitmap, null, metadata, null));
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using (var stream = File.Create(path))
            encoder.Save(stream);

        WriteTimeline(path, run, heatmap, width, height, framesPerSecond);
    }

    public static void ExportMp4(
        string path,
        SimulationRunBundle run,
        HeatmapMode heatmap = HeatmapMode.Damage,
        int width = 960,
        int height = 540,
        int framesPerSecond = 30
    )
    {
        Validate(run, width, height);
        var ffmpeg = ResolveFfmpeg();
        var temporary = Path.Combine(Path.GetTempPath(), $"ImpactLab-video-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporary);
        try
        {
            var restPositions = run.Compiled.Mesh.Nodes.Select(x => x.RestPosition).ToArray();
            var displayMaximum = GlobalMaximum(run, heatmap);
            for (var i = 0; i < run.Result.Frames.Count; i++)
            {
                var bitmap = RenderFrame(
                    restPositions,
                    run.Compiled.RigidBodies,
                    run,
                    run.Result.Frames[i],
                    heatmap,
                    width,
                    height,
                    displayMaximum
                );
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using var stream = File.Create(Path.Combine(temporary, $"frame-{i:D6}.png"));
                encoder.Save(stream);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            var start = new ProcessStartInfo
            {
                FileName = ffmpeg,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
            };
            foreach (
                var argument in new[]
                {
                    "-y",
                    "-framerate",
                    framesPerSecond.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    "-i",
                    Path.Combine(temporary, "frame-%06d.png"),
                    "-c:v",
                    "libx264",
                    "-pix_fmt",
                    "yuv420p",
                    "-movflags",
                    "+faststart",
                    path,
                }
            )
                start.ArgumentList.Add(argument);
            using var process =
                Process.Start(start)
                ?? throw new InvalidOperationException("FFmpeg could not be started.");
            var diagnostics = process.StandardError.ReadToEndAsync();
            process.WaitForExit();
            var diagnosticText = diagnostics.GetAwaiter().GetResult();
            if (process.ExitCode != 0 || !File.Exists(path))
                throw new InvalidOperationException(
                    $"FFmpeg MP4 export failed (exit {process.ExitCode}). {Tail(diagnosticText)}"
                );
            WriteTimeline(path, run, heatmap, width, height, framesPerSecond);
        }
        finally
        {
            if (Directory.Exists(temporary))
                Directory.Delete(temporary, true);
        }
    }

    public static void ExportFramePng(
        string path,
        SimulationRunBundle run,
        int frameIndex = -1,
        HeatmapMode heatmap = HeatmapMode.Displacement,
        int width = 960,
        int height = 540
    )
    {
        Validate(run, width, height);
        var index = frameIndex < 0 ? run.Result.Frames.Count - 1 : frameIndex;
        index = Math.Clamp(index, 0, run.Result.Frames.Count - 1);
        var displayMaximum = GlobalMaximum(run, heatmap);
        var bitmap = RenderFrame(
            run.Compiled.Mesh.Nodes.Select(x => x.RestPosition).ToArray(),
            run.Compiled.RigidBodies,
            run,
            run.Result.Frames[index],
            heatmap,
            width,
            height,
            displayMaximum
        );
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var stream = File.Create(path);
        encoder.Save(stream);
    }

    private static void WriteTimeline(
        string path,
        SimulationRunBundle run,
        HeatmapMode heatmap,
        int width,
        int height,
        int framesPerSecond
    )
    {
        var timelinePath = Path.ChangeExtension(path, ".timeline.json");
        File.WriteAllText(
            timelinePath,
            JsonSerializer.Serialize(
                new
                {
                    format = "ImpactLab.AnimationTimeline",
                    heatmap = heatmap.ToString(),
                    width,
                    height,
                    framesPerSecond,
                    frameCount = run.Result.Frames.Count,
                    timesSeconds = run.Result.Frames.Select(x => x.TimeSeconds).ToArray(),
                },
                TimelineJson
            )
        );
    }

    private static void Validate(SimulationRunBundle run, int width, int height)
    {
        if (run.Result.Frames.Count == 0)
            throw new InvalidOperationException("The simulation contains no frames to animate.");
        if (width < 320 || height < 240)
            throw new ArgumentOutOfRangeException(nameof(width));
    }

    private static string ResolveFfmpeg()
    {
        var configured = Environment.GetEnvironmentVariable("IMPACTLAB_FFMPEG");
        if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
            return configured;
        var bundled = Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe");
        if (File.Exists(bundled))
            return bundled;
        try
        {
            using var probe = Process.Start(
                new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-version",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            );
            probe?.WaitForExit();
            if (probe is { ExitCode: 0 })
                return "ffmpeg";
        }
        catch (Win32Exception) { }
        throw new InvalidOperationException(
            "MP4 export requires FFmpeg. Place ffmpeg.exe next to ImpactLab.App.exe, "
                + "add it to PATH, or set IMPACTLAB_FFMPEG. Animated GIF export does not require FFmpeg."
        );
    }

    private static string Tail(string value) => value.Length <= 600 ? value : value[^600..];

    private static double GlobalMaximum(SimulationRunBundle run, HeatmapMode heatmap) =>
        heatmap == HeatmapMode.Damage
            ? 1.0
            : Math.Max(
                run.Result.Frames.Max(frame =>
                    MeshSceneBuilder.MaximumValue(run.Compiled.Mesh, frame, heatmap)
                ),
                1e-12
            );

    private static RenderTargetBitmap RenderFrame(
        IReadOnlyList<Vec3> restPositions,
        IReadOnlyList<RigidBodyDefinition> rigidBodies,
        SimulationRunBundle run,
        SimulationFrame frame,
        HeatmapMode heatmap,
        int width,
        int height,
        double displayMaximum
    )
    {
        var viewport = new Viewport3D();
        var bounds = Bounds(restPositions);
        var span = Math.Max(bounds.Size.X, Math.Max(bounds.Size.Y, bounds.Size.Z));
        span = Math.Max(span, run.Compiled.Mesh.CellSize * 8);
        var center = bounds.Center;
        viewport.Camera = new PerspectiveCamera(
            new Point3D(center.X + span * 1.5, center.Y - span * 2.2, center.Z + span * 1.3),
            new Vector3D(-span * 1.5, span * 2.2, -span * 1.1),
            new Vector3D(0, 0, 1),
            42
        );

        var group = new Model3DGroup();
        group.Children.Add(new AmbientLight(Color.FromRgb(105, 112, 126)));
        group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, 1, -2)));
        group.Children.Add(
            MeshSceneBuilder.BuildDetailed(run.Compiled.Mesh, frame, heatmap, displayMaximum).Scene
        );
        for (var i = 0; i < frame.RigidBodies.Count && i < rigidBodies.Count; i++)
            group.Children.Add(RigidBodySceneBuilder.Build(rigidBodies[i], frame.RigidBodies[i]));
        viewport.Children.Add(new ModelVisual3D { Content = group });

        var canvas = new Grid
        {
            Width = width,
            Height = height,
            Background = new SolidColorBrush(Color.FromRgb(17, 21, 27)),
        };
        canvas.Children.Add(viewport);
        canvas.Children.Add(Legend(heatmap, displayMaximum, frame.TimeSeconds));
        canvas.Measure(new Size(width, height));
        canvas.Arrange(new Rect(0, 0, width, height));
        canvas.UpdateLayout();
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(canvas);
        bitmap.Freeze();
        return bitmap;
    }

    private static Border Legend(HeatmapMode heatmap, double maximum, double timeSeconds)
    {
        var gradient = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0.5),
            EndPoint = new Point(1, 0.5),
        };
        gradient.GradientStops.Add(new(Color.FromRgb(20, 61, 115), 0));
        gradient.GradientStops.Add(new(Colors.DodgerBlue, 0.25));
        gradient.GradientStops.Add(new(Color.FromRgb(52, 184, 197), 0.5));
        gradient.GradientStops.Add(new(Color.FromRgb(242, 193, 78), 0.7));
        gradient.GradientStops.Add(new(Color.FromRgb(179, 38, 30), 1));
        var scale = new Grid { Width = 260 };
        scale.RowDefinitions.Add(new() { Height = GridLength.Auto });
        scale.RowDefinitions.Add(new() { Height = new GridLength(10) });
        scale.RowDefinitions.Add(new() { Height = GridLength.Auto });
        scale.Children.Add(
            new TextBlock
            {
                Text =
                    $"{HeatmapFieldCatalog.Get(heatmap).DisplayName} · t = {timeSeconds * 1000:F3} ms",
                Foreground = Brushes.White,
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 5),
            }
        );
        var bar = new Border { Background = gradient, CornerRadius = new CornerRadius(2) };
        Grid.SetRow(bar, 1);
        scale.Children.Add(bar);
        var labels = new Grid { Margin = new Thickness(0, 4, 0, 0) };
        for (var i = 0; i < 5; i++)
            labels.ColumnDefinitions.Add(new());
        labels.Children.Add(
            new TextBlock
            {
                Text = "0",
                Foreground = Brushes.White,
                FontSize = 11,
            }
        );
        foreach (var (text, column) in new[] { ("25%", 1), ("50%", 2), ("75%", 3) })
        {
            var tick = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontSize = 9,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            Grid.SetColumn(tick, column);
            labels.Children.Add(tick);
        }
        var maximumLabel = new TextBlock
        {
            Text = FormatMaximum(heatmap, maximum),
            Foreground = Brushes.White,
            FontSize = 11,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        Grid.SetColumn(maximumLabel, 3);
        Grid.SetColumnSpan(maximumLabel, 2);
        labels.Children.Add(maximumLabel);
        Grid.SetRow(labels, 2);
        scale.Children.Add(labels);
        return new Border
        {
            Child = scale,
            Background = new SolidColorBrush(Color.FromArgb(220, 24, 28, 34)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(70, 78, 90)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Padding = new Thickness(10, 8, 10, 7),
            Margin = new Thickness(0, 0, 16, 16),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
        };
    }

    private static string FormatMaximum(HeatmapMode heatmap, double maximum) =>
        heatmap switch
        {
            HeatmapMode.EquivalentStress => $"{maximum / 1e6:F2} MPa",
            HeatmapMode.Displacement => $"{maximum * 1000:F3} mm",
            HeatmapMode.Speed => $"{maximum:F2} m/s",
            HeatmapMode.EquivalentStrain => $"{maximum:F4}",
            HeatmapMode.YieldUtilization => $"{maximum:F3}",
            HeatmapMode.Damage => "1.0",
            _ => maximum.ToString("G4", System.Globalization.CultureInfo.InvariantCulture),
        };

    private static (Vec3 Center, Vec3 Size) Bounds(IReadOnlyList<Vec3> points)
    {
        if (points.Count == 0)
            return (Vec3.Zero, new Vec3(1, 1, 1));
        var min = new Vec3(points.Min(x => x.X), points.Min(x => x.Y), points.Min(x => x.Z));
        var max = new Vec3(points.Max(x => x.X), points.Max(x => x.Y), points.Max(x => x.Z));
        return ((min + max) * 0.5, max - min);
    }
}
