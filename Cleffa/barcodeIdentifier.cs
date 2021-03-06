﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;

using ZXing;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace Cleffa
{
    class barcodeIdentifier
    {
        // =================================================================================
        // global variables
        // =================================================================================
        private Mat inputMat = new Mat();
        private Mat outputMat = new Mat();

        private BarcodeReader reader = null;
        private BarcodeFormat format;
        private Result result = null;
        private ResultPoint[] point = null;

        private bool modeHarder = false;
        private bool modeRotate = false;

        private string[] formatUsage = { "Retail", "Industrial", "QR_Code", "ID_License", "2D" };
        private string[] format1d = { "CODABAR", "CODE_128", "CODE_39", "CODE_93", "EAN_13", "EAN_8", "ITF", "UPC_A", "UPC_E", "UPC_EAN_EXTENSION" };
        private string[] format2d = { "AZTEC", "QR_CODE"  };
        private string[] format1dx = { "RSS_14", "RSS_EXPANDED" }; // more that 2 barcode points
        private string[] formatTest = { "DATA_MATRIX", "PDF_417" }; // no points

        private int areaScanExtend;

        private bool enableHaar = false;
        private static readonly CascadeClassifier classifier = new CascadeClassifier("barcode_0.2_2000_10.xml");
        private Rectangle[] rectangles;
        private Rectangle rectROI = new Rectangle();
        private bool rectFlag = false;

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
            result = null;
            point = null;
        }

        public void setInputBitmap(Bitmap input)
        {
            Image<Bgr, Byte> temp = new Image<Bgr, byte>(input);

            inputMat = temp.Mat;
            result = null;
            point = null;
        }

        public void setDecoderOption(string option, int value)
        {
            if (reader != null)
            {
                switch (option)
                {
                    case "TryHarder":
                        if (value >= 1)
                            modeHarder = true;
                        else
                            modeHarder = false;
                        break;
                    case "AutoRotate":
                        if (value >= 1)
                            modeRotate = true;
                        else
                            modeRotate = false;
                        break;
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
                    case "Test":
                        reader.Options.PossibleFormats = new List<BarcodeFormat>();
                        reader.Options.PossibleFormats.Add(BarcodeFormat.CODE_39);
                        reader.Options.PossibleFormats.Add(BarcodeFormat.EAN_13);
                        break;
                    case "Single":
                        reader.Options.PossibleFormats = new List<BarcodeFormat>();
                        reader.Options.PossibleFormats.Add(BarcodeFormat.EAN_13);
                        break;
                }
            }
        }

        // =================================================================================
        // calculations
        // =================================================================================
        public bool genDecode()
        {
            if (inputMat != null)
            {
                Mat input = new Mat();
                Mat inputGray = new Mat();
                input = inputMat.Clone();

                if (enableHaar)
                {
                    CvInvoke.CvtColor(input, inputGray, ColorConversion.Bgra2Gray);
                    CvInvoke.CLAHE(inputGray, 3.0, new Size(8, 8), inputGray);
                    rectangles = classifier.DetectMultiScale(inputGray, 1.3, 5);

                    rectFlag = false;

                    if (rectangles != null)
                    {

                        int xmin = rectangles[0].X;
                        int ymin = rectangles[0].Y;
                        int xmax = rectangles[0].X + rectangles[0].Width;
                        int ymax = rectangles[0].Y + rectangles[0].Height;

                        foreach (Rectangle rect in rectangles)
                        {
                            if (rect.X < xmin) xmin = rect.X;
                            if (rect.Y < ymin) ymin = rect.Y;
                            if (rect.X > xmax) xmax = rect.X;
                            if (rect.Y > ymax) ymax = rect.Y;
                        }

                        Rectangle rectCrop = new Rectangle(xmin, ymin, (xmax - xmin), (ymax - ymin));
                        Mat inputCrop = new Mat(inputGray, rectCrop);
                        rectROI = rectCrop;

                        reader.Options.TryHarder = false;
                        reader.AutoRotate = false;

                        Bitmap inputBitmap = inputCrop.Bitmap;
                        result = reader.Decode(inputBitmap);

                        if (result != null)
                        {
                            format = result.BarcodeFormat;
                            point = result.ResultPoints;
                            rectFlag = true;
                            return true;
                        }
                        else
                        {
                            if (modeHarder)
                            {
                                reader.Options.TryHarder = true;
                                result = reader.Decode(inputBitmap);

                                if (result != null)
                                {
                                    format = result.BarcodeFormat;
                                    point = result.ResultPoints;
                                    rectFlag = true;
                                    return true;
                                }
                                else
                                {
                                    CvInvoke.CvtColor(input, input, ColorConversion.Bgra2Gray);
                                    CvInvoke.CLAHE(input, 3.0, new Size(8, 8), input);
                                    inputBitmap = input.Bitmap;
                                    result = reader.Decode(inputBitmap);

                                    if (result != null)
                                    {
                                        format = result.BarcodeFormat;
                                        point = result.ResultPoints;
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    reader.Options.TryHarder = false;
                    reader.AutoRotate = false;

                    CvInvoke.CvtColor(input, input, ColorConversion.Bgra2Gray);

                    double minGray = 0, maxGray = 0;
                    Point maxp = new Point(0, 0);
                    Point minp = new Point(0, 0);
                    CvInvoke.MinMaxLoc(input, ref minGray, ref maxGray, ref minp, ref maxp);

                    input.ConvertTo(input, 0, 255 / (maxGray - minGray), -1 * minGray);

                    Bitmap inputBitmap = input.Bitmap;
                    result = reader.Decode(inputBitmap);

                    if (result != null)
                    {
                        format = result.BarcodeFormat;
                        point = result.ResultPoints;
                        rectFlag = true;
                        return true;
                    }
                    else
                    {
                        if (modeHarder)
                        {
                            reader.Options.TryHarder = true;
                            result = reader.Decode(inputBitmap);

                            if (result != null)
                            {
                                format = result.BarcodeFormat;
                                point = result.ResultPoints;
                                rectFlag = true;
                                return true;
                            }
                            else
                            {
                                //CvInvoke.CvtColor(input, input, ColorConversion.Bgra2Gray);
                                CvInvoke.CLAHE(input, 3.0, new Size(8, 8), input);
                                inputBitmap = input.Bitmap;
                                result = reader.Decode(inputBitmap);

                                if (result != null)
                                {
                                    format = result.BarcodeFormat;
                                    point = result.ResultPoints;
                                    return true;
                                }
                            }
                        }
                    }
                }
            

                return false;
            }
            else
                return false;
        }

        public bool genPaint()
        {
            if (point.Length >= 1 && inputMat != null)
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

                        if (rectFlag)
                        {
                            if (enableHaar)
                                CvInvoke.Rectangle(overlay, rectROI, new Bgr(Color.Yellow).MCvScalar, 15);

                            CvInvoke.Rectangle(overlay, rect, new Bgr(Color.LightGreen).MCvScalar, 15);
                            
                            CvInvoke.AddWeighted(inputMat, 0.7, overlay, 0.3, 0, outputMat);

                            if (enableHaar)
                                CvInvoke.Rectangle(outputMat, rectROI, new Bgr(Color.Yellow).MCvScalar, 2);

                            CvInvoke.Rectangle(outputMat, rect, new Bgr(Color.LightGreen).MCvScalar, 2);
                            
                        }
                        else
                        {
                            if (enableHaar)
                                CvInvoke.Rectangle(overlay, rectROI, new Bgr(Color.Yellow).MCvScalar, 15);
                            CvInvoke.Rectangle(overlay, rect, new Bgr(Color.Cyan).MCvScalar, 15);
                            
                            CvInvoke.AddWeighted(inputMat, 0.7, overlay, 0.3, 0, outputMat);

                            if (enableHaar)
                                CvInvoke.Rectangle(outputMat, rectROI, new Bgr(Color.Yellow).MCvScalar, 2);

                            CvInvoke.Rectangle(outputMat, rect, new Bgr(Color.Cyan).MCvScalar, 2);
                            
                        }

                        return true;
                    }
                }

                foreach (string x in format1dx)
                {
                    if (type.Contains(x))
                    {
                        Point outLeft = new Point((int)point[0].X, (int)point[0].Y);
                        Point outRight = new Point((int)point[0].X, (int)point[0].Y);

                        foreach (ResultPoint pts in point)
                        {
                            if ((int)pts.X < outLeft.X)
                            {
                                outLeft.X = (int)pts.X;
                                outLeft.Y = (int)pts.Y;
                            }

                            if ((int)pts.X > outRight.X)
                            {
                                outRight.X = (int)pts.X;
                                outRight.Y = (int)pts.Y;
                            }
                        }

                        areaScanExtend = 20;
                        Mat overlay = new Mat();
                        overlay = inputMat.Clone();
                        Point rectPoint = new Point(outLeft.X - areaScanExtend * 2, outLeft.Y - areaScanExtend);
                        Size rectSize = new Size((outRight.X - outLeft.X) + areaScanExtend * 4, areaScanExtend * 2);
                        Rectangle rect = new Rectangle(rectPoint, rectSize);
                        CvInvoke.Rectangle(overlay, rect, new Bgr(Color.Cyan).MCvScalar, 15);
                        CvInvoke.AddWeighted(inputMat, 0.7, overlay, 0.3, 0, outputMat);
                        CvInvoke.Rectangle(outputMat, rect, new Bgr(Color.Cyan).MCvScalar, 2);

                        CvInvoke.Rectangle(outputMat, rectROI, new Bgr(Color.Yellow).MCvScalar, 3);
                        return true;
                    }
                }

                foreach (string x in format2d)
                {
                    if (type.Contains(x) && point.Length <= 4)
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

                foreach (string x in formatTest)
                {
                    if (type.Contains(x))
                    {
                        Mat overlay = new Mat();
                        overlay = inputMat.Clone();

                        for (int i = 0; i < point.Length; i += 1)
                        {
                            Point temPoint = new Point((int)point[i].X, (int)point[i].Y);
                            CvInvoke.Circle(overlay, temPoint, 20, new Bgr(Color.Cyan).MCvScalar, 15);
                        }

                        CvInvoke.AddWeighted(inputMat, 0.7, overlay, 0.3, 0, outputMat);

                        for (int i = 0; i < point.Length; i += 1)
                        {
                            Point temPoint = new Point((int)point[i].X, (int)point[i].Y);
                            CvInvoke.Circle(outputMat, temPoint, 20, new Bgr(Color.Cyan).MCvScalar, 2);
                        }

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
