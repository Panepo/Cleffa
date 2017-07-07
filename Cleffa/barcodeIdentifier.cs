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
using Emgu.CV.Util;

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

        private string[] formatUsage = { "Retail", "Industrial", "QR_Code", "ID_License", "2D" };
        private string[] format1d = { "CODABAR", "CODE_128", "CODE_39", "CODE_93", "EAN_13", "EAN_8", "ITF", "RSS_14", "RSS_EXPANDED", "UPC_A", "UPC_E", "UPC_EAN_EXTENSION" };
        private string[] format2d = { "AZTEC", "DATA_MATRIX", "MAXICODE", "QR_CODE" };

        private int areaScanExtend;

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

        public void setDecoderFormat(string option)
        {
            if (reader != null)
            {
                switch (option)
                {
                    case "Retail":
                        reader.Options.PossibleFormats = new List<BarcodeFormat>();
                        reader.Options.PossibleFormats.Add(BarcodeFormat.EAN_8);
                        reader.Options.PossibleFormats.Add(BarcodeFormat.EAN_13);
                        reader.Options.PossibleFormats.Add(BarcodeFormat.UPC_A);
                        reader.Options.PossibleFormats.Add(BarcodeFormat.UPC_E);
                        break;
                    case "Industrial":
                        reader.Options.PossibleFormats = new List<BarcodeFormat>();
                        reader.Options.PossibleFormats.Add(BarcodeFormat.CODE_39);
                        reader.Options.PossibleFormats.Add(BarcodeFormat.CODE_128);
                        break;
                    case "QR_Code":
                        reader.Options.PossibleFormats = new List<BarcodeFormat>();
                        reader.Options.PossibleFormats.Add(BarcodeFormat.QR_CODE);
                        break;
                    case "ID_License":
                        reader.Options.PossibleFormats = new List<BarcodeFormat>();
                        reader.Options.PossibleFormats.Add(BarcodeFormat.PDF_417);
                        break;
                    case "2D":
                        reader.Options.PossibleFormats = new List<BarcodeFormat>();
                        reader.Options.PossibleFormats.Add(BarcodeFormat.QR_CODE);
                        reader.Options.PossibleFormats.Add(BarcodeFormat.AZTEC);
                        reader.Options.PossibleFormats.Add(BarcodeFormat.DATA_MATRIX);
                        break;
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
                        Point outLeft = new Point((int)point[0].X, (int)point[0].Y);
                        Point outRight = new Point((int)point[1].X, (int)point[1].Y);
                        areaScanExtend = 20;

                        Mat overlay = new Mat();
                        overlay = inputMat.Clone();
                        Point rectPoint = new Point(outLeft.X - areaScanExtend, outLeft.Y - areaScanExtend);
                        Size rectSize = new Size((outRight.X - outLeft.X) + areaScanExtend*2, areaScanExtend*2);
                        Rectangle rect = new Rectangle(rectPoint, rectSize);
                        CvInvoke.Rectangle(overlay, rect, new Bgr(Color.Cyan).MCvScalar, 15);
                        CvInvoke.AddWeighted(inputMat, 0.7, overlay, 0.3, 0, outputMat);
                        CvInvoke.Rectangle(outputMat, rect, new Bgr(Color.Cyan).MCvScalar, 2);
                        return true;
                    }
                }

                foreach (string x in format2d)
                {
                    if (type.Contains(x))
                    {
                        PointF[] outpt = new PointF[4];

                        for (int i = 0; i < point.Length; i += 1)
                            outpt[i] = new PointF(point[i].X, point[i].Y);

                        if (point.Length == 3)
                        {
                            float xdiv = outpt[0].X - outpt[1].X;
                            float ydiv = outpt[0].Y - outpt[1].Y;

                            outpt[3] = new PointF(point[2].X + xdiv, point[2].Y + ydiv);
                        }

                        RotatedRect rrec = CvInvoke.MinAreaRect(outpt);
                        PointF[] pointfs = rrec.GetVertices();

                        Mat overlay = new Mat();
                        overlay = inputMat.Clone();

                        for (int j = 0; j < pointfs.Length; j += 1)
                            CvInvoke.Line(overlay, new Point((int)pointfs[j].X, (int)pointfs[j].Y), new Point((int)pointfs[(j + 1) % 4].X, (int)pointfs[(j + 1) % 4].Y), new Bgr(Color.Cyan).MCvScalar, 15);

                        CvInvoke.AddWeighted(inputMat, 0.7, overlay, 0.3, 0, outputMat);

                        for (int j = 0; j < pointfs.Length; j += 1)
                            CvInvoke.Line(outputMat, new Point((int)pointfs[j].X, (int)pointfs[j].Y), new Point((int)pointfs[(j + 1) % 4].X, (int)pointfs[(j + 1) % 4].Y), new Bgr(Color.Cyan).MCvScalar, 2);


                        //CvInvoke.Circle(outputMat, outLU, 20, new Bgr(Color.Green).MCvScalar, 2);
                        //CvInvoke.Circle(outputMat, outRD, 20, new Bgr(Color.Blue).MCvScalar, 2);
                        return true;
                    }
                }
                return false;
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
