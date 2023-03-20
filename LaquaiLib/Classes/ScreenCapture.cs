using System.IO;
using System.Runtime.InteropServices;

using Timer = System.Threading.Timer;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace LaquaiLib.ScreenCapture;

/// <summary>
/// Wraps some screenshot functionality from <see cref="Bitmap"/> and <see cref="Graphics"/>.
/// </summary>
public partial class ScreenCapture
{
    private partial class MonitorEnum
    {
        [LibraryImport("shcore.dll")]
        private static partial nint GetScaleFactorForMonitor(nint hmonitor, out nint deviceScaleFactor);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref Rect pRect, int dwData);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool EnumDisplayMonitors(nint hdc, nint lpRect, MonitorEnumProc callback, int dwData);

        public static double[] EnumerateScales()
        {
            List<double> scales = new();
            bool Callback(IntPtr hDesktop, IntPtr hdc, ref Rect pRect, int dwData)
            {
                GetScaleFactorForMonitor(hDesktop, out nint scale);
                scales.Add(Math.Round(scale / 100d / 0.25) * 0.25);
                // scales.Add(scale / 100d);

                return true;
            }

            EnumDisplayMonitors(nint.Zero, nint.Zero, Callback, 0);
            return scales.ToArray();
        }
    }

    private static readonly double[] _resolutionScales = MonitorEnum.EnumerateScales();

    /// <summary>
    /// The resolution scales set in Windows Settings for each monitor. They are used whenever the capture region is automatically set.
    /// </summary>
    public static double[] ResolutionScales => _resolutionScales;

    /// <summary>
    /// <para>Captures a region of the screen.</para>
    /// <para>Passed coordinates are not corrected using <see cref="ResolutionScales"/>.</para>
    /// </summary>
    /// <param name="x1">The x-coordinate of the top-left point of the region.</param>
    /// <param name="y1">The y-coordinate of the top-left point of the region.</param>
    /// <param name="x2">The x-coordinate of the bottom-right point of the region.</param>
    /// <param name="y2">The y-coordinate of the bottom-right point of the region.</param>
    /// <returns>The <see cref="Bitmap"/> created by capturing the region.</returns>
    public static Bitmap Capture(int x1, int y1, int x2, int y2)
    {
        Rectangle region = new(x1, y1, x2 - x1, y2 - y1);
        Bitmap capture = new(region.Width, region.Height);
        Graphics cg = Graphics.FromImage(capture);
        cg.CopyFromScreen(region.Left, region.Top, 0, 0, region.Size);
        return capture;
    }

    /// <summary>
    /// <para>Captures a region of the screen.</para>
    /// <para>Position coordinates of the passed <see cref="Rectangle"/> are not corrected using <see cref="ResolutionScales"/>.</para>
    /// </summary>
    /// <param name="region">The region to capture.</param>
    /// <returns>A <see cref="Bitmap"/> object containing the capture created from the given region.</returns>
    public static Bitmap Capture(Rectangle region) => Capture(region.Left, region.Top, region.Right, region.Bottom);

    /// <summary>
    /// Captures the entire primary screen.
    /// </summary>
    /// <returns>A <see cref="Bitmap"/> object containing the capture created from the primary screen.</returns>
    public static Bitmap Capture()
    {
        Rectangle primary = Screen.PrimaryScreen!.Bounds;
        return Capture(primary.Left, primary.Top, (int)(primary.Right * ResolutionScales[0]), (int)(primary.Bottom * ResolutionScales[0]));
    }

    /// <summary>
    /// Creates a test image to show which region the passed coordinates would capture. If <paramref name="extract"/> is <c>false</c>, a red rectangle is painted onto the created capture <see cref="Bitmap"/> to show this region. Otherwise, only the pixels inside the region are written to the output file. A singular blue pixel shows the center of this region.
    /// </summary>
    /// <remarks>
    /// <para>The blue center pixel may be off-center if the chosen region has even width and/or height.</para>
    /// </remarks>
    /// <param name="x1">The x-coordinate of the top-left point of the region.</param>
    /// <param name="y1">The y-coordinate of the top-left point of the region.</param>
    /// <param name="x2">The x-coordinate of the bottom-right point of the region.</param>
    /// <param name="y2">The y-coordinate of the bottom-right point of the region.</param>
    /// <param name="extract">Whether to only write the pixels inside the region to the output image.</param>
    /// <returns>The path to saved <see cref="Bitmap"/>.</returns>
    public static string TestRegion(int x1, int y1, int x2, int y2, bool extract = false)
    {
        string path = Path.Combine(Path.GetTempPath(), $"testregion_{x1}_{y1}_{x2}_{y2}_{new Random().Next(10000)}.bmp");
        Bitmap desktop = Capture();
        if (extract)
        {
            desktop = desktop.Clone(new(x1, y1, x2 - x1, y2 - y1), desktop.PixelFormat);

            desktop.SetPixel(desktop.Width / 2, desktop.Height / 2, Color.FromArgb(0x00, 0x00, 0xFF));
        }
        else
        {
            Color highlight = Color.FromArgb(0xFF, 0x00, 0x00);
            for (int y = y1; y <= y2; y++)
            {
                desktop.SetPixel(x1, y, highlight);
                desktop.SetPixel(x2, y, highlight);
            }
            for (int x = x1; x <= x2; x++)
            {
                desktop.SetPixel(x, y1, highlight);
                desktop.SetPixel(x, y2, highlight);
            }

            desktop.SetPixel((x1 + x2) / 2, (y1 + y2) / 2, Color.FromArgb(0x00, 0x00, 0xFF));
        }

        desktop.Save(path, System.Drawing.Imaging.ImageFormat.Bmp);

        return path;
    }

    /// <summary>
    /// Scales the passed coordinates using the <see cref="ResolutionScales"/> to monitor coordinates.
    /// </summary>
    /// <param name="monitor">The monitor to scale the coordinates for.</param>
    /// <param name="scaleDown">Whether to scale down (<c>true</c>) or up (<c>false</c>).</param>
    /// <param name="x">The <c>x</c>-coordinate to scale.</param>
    /// <param name="y">The <c>y</c>-coordinate to scale.</param>
    public static void ScaleCoordinates(int monitor, bool scaleDown, ref int x, ref int y)
    {
        x = scaleDown ? (int)(x / ResolutionScales[monitor]) : (int)(x * ResolutionScales[monitor]);
        y = scaleDown ? (int)(y / ResolutionScales[monitor]) : (int)(y * ResolutionScales[monitor]);
    }

    /// <summary>
    /// Scales the passed coordinates using the <see cref="ResolutionScales"/> to monitor coordinates.
    /// </summary>
    /// <param name="monitor">The monitor to scale the coordinates for.</param>
    /// <param name="scaleDown">Whether to scale down (<c>true</c>) or up (<c>false</c>).</param>
    /// <param name="x1">The <c>x</c>-coordinate of the top-left point to scale.</param>
    /// <param name="y1">The <c>y</c>-coordinate of the top-left point to scale.</param>
    /// <param name="x2">The <c>x</c>-coordinate of the bottom-right point to scale.</param>
    /// <param name="y2">The <c>y</c>-coordinate of the bottom-right point to scale.</param>
    public static void ScaleCoordinates(int monitor, bool scaleDown, ref int x1, ref int y1, ref int x2, ref int y2)
    {
        x1 = scaleDown ? (int)(x1 / ResolutionScales[monitor]) : (int)(x1 * ResolutionScales[monitor]);
        y1 = scaleDown ? (int)(y1 / ResolutionScales[monitor]) : (int)(y1 * ResolutionScales[monitor]);
        x2 = scaleDown ? (int)(x2 / ResolutionScales[monitor]) : (int)(x2 * ResolutionScales[monitor]);
        y2 = scaleDown ? (int)(y2 / ResolutionScales[monitor]) : (int)(y2 * ResolutionScales[monitor]);
    }

    /// <summary>
    /// Scales the passed <see cref="Rectangle"/> using the <see cref="ResolutionScales"/> to monitor coordinates.
    /// </summary>
    /// <param name="monitor">The monitor to scale the coordinates for.</param>
    /// <param name="scaleDown">Whether to scale down (<c>true</c>) or up (<c>false</c>).</param>
    /// <param name="rect">The <see cref="Rectangle"/> to scale.</param>
    public static void ScaleCoordinates(int monitor, bool scaleDown, ref Rectangle rect)
    {
        rect.X = scaleDown ? (int)(rect.X / ResolutionScales[monitor]) : (int)(rect.X * ResolutionScales[monitor]);
        rect.Y = scaleDown ? (int)(rect.Y / ResolutionScales[monitor]) : (int)(rect.Y * ResolutionScales[monitor]);
        rect.Width = scaleDown ? (int)(rect.Width / ResolutionScales[monitor]) : (int)(rect.Width * ResolutionScales[monitor]);
        rect.Height = scaleDown ? (int)(rect.Height / ResolutionScales[monitor]) : (int)(rect.Height * ResolutionScales[monitor]);
    }

    /// <summary>
    /// Occurs when this <see cref="ScreenCapture"/> captures the configured region, but only if <see cref="Predicate"/> is satisfied.
    /// </summary>
    public event EventHandler<ScreenCaptureEventArgs>? Captured;

    /// <summary>
    /// The predicate that is checked whenever a capture would occur. If this returns <c>false</c>, the capture is discarded.
    /// </summary>
    public Func<bool> Predicate { get; set; }

    /// <summary>
    /// The <see cref="System.Threading.Timer"/> that controls when captures are made.
    /// </summary>
    private Timer Timer;

    /// <summary>
    /// Whether this <see cref="ScreenCapture"/> is currently creating captures and may raise the <see cref="Captured"/> event.
    /// </summary>
    public bool IsCapturing { get; private set; } = false;
    /// <summary>
    /// The region this <see cref="ScreenCapture"/> captures.
    /// </summary>
    public Rectangle Region { get; private set; }
    /// <summary>
    /// Whether the configured capture <see cref="Region"/> is the entire primary screen.
    /// </summary>
    public bool IsCaptureRegionScreen {
        get {
            Rectangle rect = Screen.PrimaryScreen!.Bounds;
            return new Rectangle((int)(rect.Left * ResolutionScales[0]), (int)(rect.Top * ResolutionScales[0]), (int)(rect.Right * ResolutionScales[0]), (int)(rect.Bottom * ResolutionScales[0])) == Region;
        }
    }

    /// <summary>
    /// Causes this <see cref="ScreenCapture"/> to create captures and raise the <see cref="Captured"/> event.
    /// </summary>
    public void Start()
    {
        IsCapturing = true;

        Timer.Change(0, 50);
    }

    /// <summary>
    /// Causes this <see cref="ScreenCapture"/> to no longer create captures and raise the <see cref="Captured"/> event.
    /// </summary>
    public void Stop()
    {
        IsCapturing = false;

        Timer.Change(Timeout.Infinite, 50);
    }

    /// <summary>
    /// Instantiates a new <see cref="ScreenCapture"/> with the capture <see cref="Region"/> set to the entire primary screen and a <see cref="Predicate"/> that allows all created captures.
    /// </summary>
    public ScreenCapture()
    {
        Rectangle rect = Screen.PrimaryScreen!.Bounds;
        ScaleCoordinates(Screen.AllScreens.ToList().IndexOf(Screen.PrimaryScreen), false, ref rect);
        Predicate = () => true;
        InitTimer();
    }

    /// <summary>
    /// Instantiates a new <see cref="ScreenCapture"/> with the capture <see cref="Region"/> set to the entire primary screen and a passed <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">The <see cref="Predicate"/> that is checked whenever a capture would occur. If this returns <c>false</c>, the capture is discarded.</param>
    public ScreenCapture(Func<bool> predicate)
    {
        Rectangle rect = Screen.PrimaryScreen!.Bounds;
        ScaleCoordinates(Screen.AllScreens.ToList().IndexOf(Screen.PrimaryScreen), false, ref rect);
        Predicate = predicate;
        InitTimer();
    }

    /// <summary>
    /// Instantiates a new <see cref="ScreenCapture"/> with the capture <see cref="Region"/> set to the passed <paramref name="region"/> and a <see cref="Predicate"/> that allows all created captures.
    /// </summary>
    /// <param name="region">The region this <see cref="ScreenCapture"/> captures.</param>
    public ScreenCapture(Rectangle region)
    {
        Region = region;
        Predicate = () => true;
        InitTimer();
    }

    /// <summary>
    /// Instantiates a new <see cref="ScreenCapture"/> with the capture <see cref="Region"/> set to the passed <paramref name="region"/> and a passed <paramref name="predicate"/>.
    /// </summary>
    /// <param name="region">The region this <see cref="ScreenCapture"/> captures.</param>
    /// <param name="predicate">The <see cref="Predicate"/> that is checked whenever a capture would occur. If this returns <c>false</c>, the capture is discarded.</param>
    public ScreenCapture(Rectangle region, Func<bool> predicate)
    {
        Region = region;
        Predicate = predicate;
        InitTimer();
    }

    /// <summary>
    /// Instantiates a new <see cref="ScreenCapture"/> with the capture <see cref="Region"/> created from a series of passed coordinates and a <see cref="Predicate"/> that allows all created captures.
    /// </summary>
    /// <param name="x1">The x-coordinate of the top-left point of the region.</param>
    /// <param name="y1">The y-coordinate of the top-left point of the region.</param>
    /// <param name="x2">The x-coordinate of the bottom-right point of the region.</param>
    /// <param name="y2">The y-coordinate of the bottom-right point of the region.</param>
    public ScreenCapture(int x1, int y1, int x2, int y2)
    {
        Region = new(x1, y1, x2 - x1, y2 - y1);
        Predicate = () => true;
        InitTimer();
    }

    /// <summary>
    /// Instantiates a new <see cref="ScreenCapture"/> with the capture <see cref="Region"/> created from a series of passed coordinates and a passed <paramref name="predicate"/>.
    /// </summary>
    /// <param name="x1">The x-coordinate of the top-left point of the region.</param>
    /// <param name="y1">The y-coordinate of the top-left point of the region.</param>
    /// <param name="x2">The x-coordinate of the bottom-right point of the region.</param>
    /// <param name="y2">The y-coordinate of the bottom-right point of the region.</param>
    /// <param name="predicate">The <see cref="Predicate"/> that is checked whenever a capture would occur. If this returns <c>false</c>, the capture is discarded.</param>
    public ScreenCapture(int x1, int y1, int x2, int y2, Func<bool> predicate)
    {
        Region = new(x1, y1, x2 - x1, y2 - y1);
        Predicate = predicate;
        InitTimer();
    }

    /// <summary>
    /// Instantiates a new <see cref="ScreenCapture"/> with the capture <see cref="Region"/> set to a specific screen and a <see cref="Predicate"/> that allows all created captures.
    /// </summary>
    /// <param name="monitor">The number of the monitor to capture.</param>
    public ScreenCapture(int monitor)
    {
        Rectangle rect = Screen.AllScreens[monitor].Bounds;
        ScaleCoordinates(monitor, false, ref rect);
        Predicate = () => true;
        InitTimer();
    }

    /// <summary>
    /// Instantiates a new <see cref="ScreenCapture"/> with the capture <see cref="Region"/> set to a specific screen and a passed <paramref name="predicate"/>.
    /// </summary>
    /// <param name="monitor">The number of the monitor to capture.</param>
    /// <param name="predicate">The <see cref="Predicate"/> that is checked whenever a capture would occur. If this returns <c>false</c>, the capture is discarded.</param>
    public ScreenCapture(int monitor, Func<bool> predicate)
    {
        Rectangle rect = Screen.AllScreens[monitor].Bounds;
        ScaleCoordinates(monitor, false, ref rect);
        Predicate = predicate;
        InitTimer();
    }

    /// <summary>
    /// Initializes the <see cref="Timer"/> that controls when captures are made.
    /// </summary>
    private void InitTimer()
    {
        Timer = new(info =>
        {
            ScreenCapture src = (ScreenCapture)info!;
            if (src.IsCapturing && src.Predicate())
            {
                DateTime captureTime = DateTime.Now;
                RaiseEvent(new(Capture(), captureTime));
            }
        }, this, Timeout.Infinite, 50);
    }

    /// <summary>
    /// Event raise wrapper.
    /// </summary>
    /// <param name="e">The <see cref="ScreenCaptureEventArgs"/> passed when raising the event.</param>
    protected virtual void RaiseEvent(ScreenCaptureEventArgs e)
    {
        EventHandler<ScreenCaptureEventArgs> @event = Captured!;
        @event?.Invoke(this, e);
    }

    /// <summary>
    /// Destructor. Ensures that the <see cref="Timer"/> no longer causes captures to be made when this <see cref="ScreenCapture"/> dies.
    /// </summary>
    ~ScreenCapture()
    {
        Stop();
    }
}

/// <summary>
/// Event args that are passed when raising a <see cref="ScreenCapture.Captured"/> event.
/// </summary>
public class ScreenCaptureEventArgs : EventArgs
{
    /// <summary>
    /// The created <see cref="System.Drawing.Bitmap"/> capture.
    /// </summary>
    public Bitmap Bitmap { get; init; }
    /// <summary>
    /// When the capture was created.
    /// </summary>
    public DateTime CaptureTime { get; init; }
    /// <summary>
    /// Instantiates <see cref="ScreenCaptureEventArgs"/> with the passed <paramref name="bitmap"/> and <paramref name="timestamp"/>.
    /// </summary>
    /// <param name="bitmap"></param>
    /// <param name="timestamp"></param>
    public ScreenCaptureEventArgs(Bitmap bitmap, DateTime timestamp)
    {
        Bitmap = bitmap;
        CaptureTime = timestamp;
    }
}
