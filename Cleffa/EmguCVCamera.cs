using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace Cleffa
{
    class EmguCVCamera
    {
        private Capture capture;
        DispatcherTimer timer;

        public EmguCVCamera(EventHandler myEventHandler)
        {
            capture = new Capture();

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(myEventHandler);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        public void stopTimer()
        {
            if (timer.IsEnabled)
                timer.Stop();
        }

        public void startTimer()
        {
            if (!timer.IsEnabled)
                timer.Start();
        }

        public Image<Bgr, Byte> frameCapture()
        {
            return capture.QueryFrame().ToImage<Bgr, Byte>();
        }
    }
}
