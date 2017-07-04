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

        private BarcodeReader reader = null;
        private BarcodeFormat format;
        private Result result = null;
        private ResultPoint[] point = null;

        private string[] format1d = { "CODABAR", "CODE_128", "CODE_39", "CODE_93", "EAN_13", "EAN_8", "ITF", "RSS_14", "RSS_EXPANDED", "UPC_A", "UPC_E", "UPC_EAN_EXTENSION" };
        private string[] format2d = { "AZTEC", "DATA_MATRIX", "MAXICODE", "QR_CODE"};
        private string[] formatOther = { "PDF_417"};

        // =================================================================================
        // constructor
        // =================================================================================
        public barcodeIdentifier()
        {
            reader = new BarcodeReader();
            format = new BarcodeFormat();
        }

        // =================================================================================
        // input & options settings
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

        public void setDecoderOption(string option, int value)
        {
            if (reader != null)
            {
                if (option == "TryHarder")
                {
                    if (value >= 1)
                        reader.Options.TryHarder = true;
                    else
                        reader.Options.TryHarder = false;
                }
                else if (option == "AutoRotate")
                {
                    if (value >= 1)
                        reader.AutoRotate = true;
                    else
                        reader.AutoRotate = false;
                }
            }
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
            if (point != null && inputMat != null)
            {
                string type = format.ToString();

                foreach (string x in format1d)
                {
                    if (type.Contains(x))
                    {
                        Point outLeft = new Point();
                        Point outRight = new Point();

                        outLeft.X = (int)point[0].X;
                        outLeft.Y = (int)point[0].Y;
                        outRight.X = (int)point[1].X;
                        outRight.Y = (int)point[1].Y;

                        Mat overlay = new Mat();
                        overlay = inputMat.Clone();
                        Point rectPoint = new Point(outLeft.X - 20, outLeft.Y - 20);
                        Size rectSize = new Size((outRight.X - outLeft.X) + 40, 40);
                        Rectangle rect = new Rectangle(rectPoint, rectSize);
                        CvInvoke.Rectangle(overlay, rect, new Bgr(Color.Cyan).MCvScalar, -1);
                        CvInvoke.AddWeighted(inputMat, 0.7, overlay, 0.3, 0, outputMat);
                        CvInvoke.Rectangle(outputMat, rect, new Bgr(Color.Cyan).MCvScalar, 2);
                    }
                }
                return true;
            }
            else
                return false;
        }

        // =================================================================================
        // get result
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
