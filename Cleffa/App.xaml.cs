using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using ZXing;
namespace Cleffa
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
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
