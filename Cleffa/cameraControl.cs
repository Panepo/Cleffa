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

namespace Cleffa
{
    class cameraControl
    {
        private Capture capture;
        private DispatcherTimer timer;
        public int cameraDevice = 0;

        public cameraControl(int webcamDevice, EventHandler myEventHandler)
        {
            cameraDevice = webcamDevice;
            capture = new Capture(webcamDevice);
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 1920);
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 1080);

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(myEventHandler);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        public void stopTimer()
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
            }
        }

        public void startTimer()
        {
            if (!timer.IsEnabled)
            {
                timer.Start();
            }
        }

        public Mat frameCapture()
        {
            return capture.QueryFrame();
        }

        public void setDevice(int webcamDevice, EventHandler myEventHandler)
        {
            if (capture != null)
            {
                capture.Dispose();
            }

            capture = new Capture(webcamDevice);
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 1920);
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 1080);

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(myEventHandler);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        public void releaseCamera()
        {
            if (capture != null)
            {
                capture.Dispose();
            }
        }
    }
}
