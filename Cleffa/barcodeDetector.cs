using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;

namespace Cleffa
{
    public class barcodeDetector
    {
        public Image<Bgr, Byte> imageInput = null;
        public Image<Gray, Byte> imageGray = null;

        public barcodeDetector()
        {

        }

        //public void setInput(Image<Bgr, Byte> input)
        //{
         //   imageInput = new Image<Bgr, Byte>(input);
        //}

        public bool genOutput()
        {
            if (imageInput == null) return false;


            CvInvoke.CvtColor(imageInput, imageGray, ColorConversion.Bgr2Gray);
            return true;
        }
    }
}
