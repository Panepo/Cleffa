using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using ZXing;

namespace Cleffa
{
    class barcodeIdentifier
    {
        public Result result = null;

        private BarcodeReader reader = null;

        public barcodeIdentifier()
        {
            reader = new BarcodeReader { AutoRotate = true };
            reader.Options.TryHarder = true;
        }

        public void decode(Bitmap image)
        {
            result = reader.Decode(image);
        }

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
                return result.GetType().ToString().Trim();
            else
                return null;
        }

    }
}
