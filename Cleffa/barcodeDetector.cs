using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;


namespace Cleffa
{
    public class barcodeDetector
    {
        public Mat imageInput;

        private Mat imageGray;
        private Size sizeBlur;
        private Point anchor;

        public barcodeDetector()
        {
            imageInput = new Mat();
            imageGray = new Mat();
            sizeBlur = new Size(3, 3);
            anchor = new Point(-1, -1);
        }

        public void setInput(Mat input)
        {
            imageInput = input;
        }

        public bool genOutput()
        {
            if (imageInput == null) return false;

            CvInvoke.CvtColor(imageInput, imageGray, ColorConversion.Bgr2Gray);
            CvInvoke.Blur(imageGray, imageGray, sizeBlur, anchor);


            return true;
        }
    }
}
