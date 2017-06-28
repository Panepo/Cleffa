using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using ZXing;
using System.IO;
using System.Windows.Media.Imaging;
using Emgu.CV;
using System.Runtime.InteropServices;

namespace Cleffa
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        static public Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);
        static public BitmapSource ToBitmapSource(IImage image)
        {
            using (Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap();

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr);
                return bs;
            }
        }


        static public string decode(Bitmap image)
        {
            BarcodeReader reader = new BarcodeReader { AutoRotate = true };
            reader.Options.TryHarder = true;

            Result result = reader.Decode(image);

            if (result != null)
            {
                string decoded = result.ToString().Trim();
                return decoded;
            }
            else
            {
                return null;
            }
        }
    }
}
