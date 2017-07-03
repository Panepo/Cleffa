using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media.Imaging;

using ZXing;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;

namespace Cleffa
{
    class barcodeIdentifier
    {
        // =================================================================================
        // global variables
        // =================================================================================
        
        private Mat inputMat = new Mat();
        private Bitmap inputBitmap;
        private Mat outputMat = new Mat();

        public BarcodeReader reader = null;
        private BarcodeFormat format;
        private Result result = null;
        private ResultPoint[] point = null;
 
        // =================================================================================
        // constructor
        // =================================================================================
        public barcodeIdentifier()
        {
            reader = new BarcodeReader();
            format = new BarcodeFormat();
        }

        // =================================================================================
        // input & parameter settings
        // =================================================================================
        public void setInputMat(Mat input)
        {
            inputMat = input;
            inputBitmap = input.Bitmap;
            result = null;
            point = null;
        }

        public void setInputBitmap(Bitmap input)
        {
            Image<Bgr, Byte> temp = new Image<Bgr, byte>(input);

            inputMat = temp.Mat;
            inputBitmap = input;
            result = null;
            point = null;
        }

        // =================================================================================
        // calculations
        // =================================================================================
        public bool genDecode()
        {
            if (inputBitmap != null)
            {
                result = reader.Decode(inputBitmap);

                if (result != null)
                {
                    format = result.BarcodeFormat;
                    point = result.ResultPoints;
                    return true;
                }

                return false;
            }
            else
                return false;
        }

        public bool genPaint()
        {
            Point outLeft = new Point();
            Point outRight = new Point();

            if (point != null && inputMat != null)
            {
                int pointLeft = 999999;
                int pointRight = 0;
                int idxLeft = 0;
                int idxRight = 0;

                for (int i = 0; i < point.Length; i += 1)
                {
                    if (point[i].X < pointLeft)
                    {
                        pointLeft = (int)point[i].X;
                        idxLeft = i;
                    }

                    if (point[i].X > pointRight)
                    {
                        pointRight = (int)point[i].X;
                        idxRight = i;
                    }
                }


                outLeft.X = (int)point[idxLeft].X;
                outLeft.Y = (int)point[idxLeft].Y;

                outRight.X = (int)point[idxRight].X;
                outRight.Y = (int)point[idxRight].Y;

                outputMat = inputMat.Clone();
                CvInvoke.Line(outputMat, outLeft, outRight, new Bgr(Color.Cyan).MCvScalar, 10);

                return true;
            }
            else
                return false;
        }


        // =================================================================================
        // get parameter settings
        // =================================================================================
        public string getResultString()
        {
            if (result != null)
                return result.ToString().Trim();
            else
                return null;
        }

        public string getResultType()
        {
            if (result != null)
                return format.ToString();
            else
                return null;
        }

        public Mat getResultMat()
        {
            if (result != null && outputMat != null)
                return outputMat;
            else
                return null;
        }

        public Bitmap getResultBitmap()
        {
            if (result != null && outputMat != null)
                return outputMat.Bitmap;
            else
                return null;
        }

        public BitmapSource getResultBitmapSource()
        {
            if (result != null && outputMat != null)
                return formatCoversion.ToBitmapSource(outputMat);
            else
                return null;
        }
    }
}
