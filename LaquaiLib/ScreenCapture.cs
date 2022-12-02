using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LaquaiLib.ScreenCapture
{
    public class ScreenCapture
    {
        public static Bitmap Capture(int x1, int y1, int x2, int y2)
        {
            Rectangle region = new(x1, y1, x2 - x1, y2 - y1);
            Bitmap capture = new(region.Width, region.Height);
            Graphics cg = Graphics.FromImage(capture);
            cg.CopyFromScreen(region.Left, region.Top, 0, 0, region.Size);
            return capture;
        }

        public static Bitmap Capture(Rectangle region) => Capture(region.Left, region.Top, region.Right, region.Bottom);

        public static Bitmap Capture() => Capture(Screen.PrimaryScreen!.Bounds);

        public event EventHandler<ScreenCaptureEventArgs>? Captured;

        public bool IsCapturing { get; private set; } = false;
        public Rectangle Region { get; private set; }
        public bool IsCaptureRegionScreen => Screen.PrimaryScreen!.Bounds == Region;

        public void Start()
        {
            IsCapturing = true;

            // Find way to constantly capture
            new Thread(() =>
            {
                while (IsCapturing)
                {
                    DateTime captureTime = DateTime.Now;
                    RaiseEvent(new(ScreenCapture.Capture(), captureTime));
                }
            }).Start();
        }

        public void Stop()
        {
            IsCapturing = false;
        }

        public ScreenCapture() => Region = Screen.PrimaryScreen!.Bounds;

        public ScreenCapture(Rectangle region) => Region = region;

        public ScreenCapture(int x1, int y1, int x2, int y2) => Capture(new Rectangle(x1, y1, x2 - x1, y2 - y1));

        protected virtual void RaiseEvent(ScreenCaptureEventArgs e)
        {
            EventHandler<ScreenCaptureEventArgs> @event = Captured;
            @event?.Invoke(this, e);
        }

        ~ScreenCapture()
        {
            Stop();
        }
    }

    public class ScreenCaptureEventArgs : EventArgs
    {
        public Bitmap Bitmap { get; init; }
        public DateTime CaptureTime { get; init; }
        public ScreenCaptureEventArgs(Bitmap bitmap, DateTime when)
        {
            Bitmap = bitmap;
            CaptureTime = when;
        }
    }
}
