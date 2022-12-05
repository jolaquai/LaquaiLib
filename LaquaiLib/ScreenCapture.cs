using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Timer = System.Threading.Timer;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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

        public Func<bool> Predicate { get; set; }

        private Timer Timer;

        public bool IsCapturing { get; private set; } = false;
        public Rectangle Region { get; private set; }
        public bool IsCaptureRegionScreen => Screen.PrimaryScreen!.Bounds == Region;

        public void Start()
        {
            IsCapturing = true;

            Timer.Change(0, 50);
        }

        public void Stop()
        {
            IsCapturing = false;

            Timer.Change(Timeout.Infinite, 50);
        }

        public ScreenCapture()
        {
            Region = Screen.PrimaryScreen!.Bounds;
            Predicate = () => true;
            InitTimer();
        }
        
        public ScreenCapture(Func<bool> predicate)
        {
            Region = Screen.PrimaryScreen!.Bounds;
            Predicate = predicate;
            InitTimer();
        }

        public ScreenCapture(Rectangle region)
        {
            Region = region;
            Predicate = () => true;
            InitTimer();
        }
        
        public ScreenCapture(Rectangle region, Func<bool> predicate)
        {
            Region = region;
            Predicate = predicate;
            InitTimer();
        }

        public ScreenCapture(int x1, int y1, int x2, int y2)
        {
            Region = new(x1, y1, x2 - x1, y2 - y1);
            Predicate = () => true;
            InitTimer();
        }
        
        public ScreenCapture(int x1, int y1, int x2, int y2, Func<bool> predicate)
        {
            Region = new(x1, y1, x2 - x1, y2 - y1);
            Predicate = predicate;
            InitTimer();
        }
        
        public ScreenCapture(int monitor)
        {
            Region = Screen.AllScreens[monitor].Bounds;
            Predicate = () => true;
            InitTimer();
        }
        
        public ScreenCapture(int monitor, Func<bool> predicate)
        {
            Region = Screen.AllScreens[monitor].Bounds;
            Predicate = predicate;
            InitTimer();
        }

        private void InitTimer()
        {
            Timer = new(info =>
            {
                ScreenCapture src = (ScreenCapture)info;
                if (src.IsCapturing && src.Predicate())
                {
                    DateTime captureTime = DateTime.Now;
                    RaiseEvent(new(ScreenCapture.Capture(), captureTime));
                }
            }, this, Timeout.Infinite, 50);
        }

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
