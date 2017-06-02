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

namespace Cleffa
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        static private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
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

        static public string decode(BitmapImage inputImg)
        {
            Bitmap image = BitmapImage2Bitmap(inputImg);

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
