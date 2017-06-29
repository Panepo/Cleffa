using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;

namespace Cleffa
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private EmguCVCamera webcam = null;
        private bool captureInProgress = false;
        private Image<Bgr, Byte> currentFrame = null;

        public MainWindow() 
        {
            InitializeComponent();          
        }

        private void barcodeDetect(Bitmap image)
        {  
            string decoded = App.decode(image);
            if (decoded != null)
            {
                text1.Text = "Code analysis successed";
                text2.Text = decoded;

                if (captureInProgress)
                {
                    button2.Content = "Camera Restart";
                    webcam.stopTimer();
                    captureInProgress = !captureInProgress;
                }
            }
            else
            {
                text1.Text = "Code analysis failed";
                text2.Text = String.Empty;
            }

        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            currentFrame = webcam.frameCapture();

            if (currentFrame != null)
            {
                imageBox1.Source = FormatCoversion.ToBitmapSource(currentFrame);
                barcodeDetect(currentFrame.ToBitmap());
            }   
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (captureInProgress)
            {
                button2.Content = "Camera Restart";
                webcam.stopTimer();
                captureInProgress = !captureInProgress;
            }

            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BitmapImage input = new BitmapImage(new Uri(open.FileName));

                if (input != null)
                {
                    imageBox1.Source = input;
                    currentFrame = null;
                    Bitmap image = FormatCoversion.BitmapImage2Bitmap(input);
                    barcodeDetect(image);
                }
                else
                {
                    text1.Text = "Load image failed";
                    text2.Text = String.Empty;
                }
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (webcam == null)
            {
                webcam = new EmguCVCamera(ProcessFrame);
            }

            if (captureInProgress)
            {  
                button2.Content = "Camera Restart";
                webcam.stopTimer();
            }
            else
            {
                button2.Content = "Camera Stop";
                webcam.startTimer();
            }
            captureInProgress = !captureInProgress;
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (currentFrame == null)
            {
                text1.Text = "There is no image to save.";
            }
            else
            {
                if (captureInProgress)
                {
                    button2.Content = "Camera Restart";
                    webcam.stopTimer();
                    captureInProgress = !captureInProgress;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
                saveFileDialog.Filter = "PNG Image File (*.PNG)|*.PNG|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FileStream stream = new FileStream(saveFileDialog.FileName, FileMode.Create);
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Interlace = PngInterlaceOption.Off;
                    encoder.Frames.Add(BitmapFrame.Create(FormatCoversion.ToBitmapSource(currentFrame)));
                    encoder.Save(stream);
                    stream.Close();
                    text1.Text = "The image was saved to your computer.";
                }
            }
        }
    }
}
