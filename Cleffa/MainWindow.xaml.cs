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
using System.Diagnostics;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;

using DirectShowLib;

namespace Cleffa
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private cameraControl webcam = null;
        private bool captureInProgress = false;
        private Image<Bgr, Byte> currentFrame = null;
        private cameraDevice[] webcams;
        private int webcamDevice = 0;
        private barcodeDetector detect;
        private Stopwatch watch;

        public MainWindow() 
        {
            InitializeComponent();

            DsDevice[] systemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            webcams = new cameraDevice[systemCameras.Length];

            for (int i = 0; i < systemCameras.Length; i++)
            {
                webcams[i] = new cameraDevice(i, systemCameras[i].Name, systemCameras[i].ClassID);
                comboBox1.Items.Add(webcams[i].ToStringS());
            }

            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
                button2.IsEnabled = true;
            }

            detect = new barcodeDetector();
            watch = new Stopwatch();
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
            currentFrame = webcam.frameCapture().ToImage<Bgr, Byte>();

            if (currentFrame != null)
            {
                watch.Reset();
                watch.Start();
                imageBox1.Source = formatCoversion.ToBitmapSource(currentFrame);
                barcodeDetect(currentFrame.ToBitmap());

                //detect.setInput(currentFrame);

                //if (detect.genOutput() == true)
                //    imageBox1.Source = formatCoversion.ToBitmapSource(detect.imageGray);
                watch.Stop();
                text3.Text = watch.Elapsed.TotalMilliseconds.ToString() + "ms";
            }   
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (captureInProgress)
            {
                button2.Content = "Camera Restart";
                button3.IsEnabled = false;
                webcam.stopTimer();
                captureInProgress = !captureInProgress;
            }

            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                watch.Reset();
                watch.Start();
                BitmapImage input = new BitmapImage(new Uri(open.FileName));

                if (input != null)
                {
                    imageBox1.Source = input;
                    currentFrame = null;
                    Bitmap image = formatCoversion.BitmapImage2Bitmap(input);
                    barcodeDetect(image);
                    watch.Stop();
                    text3.Text = watch.Elapsed.TotalMilliseconds.ToString() + "ms";
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
                webcam = new cameraControl(webcamDevice, ProcessFrame);
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
                button3.IsEnabled = true;
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
                saveFileDialog.FileName = "Cleffa_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
                saveFileDialog.Filter = "PNG Image File (*.PNG)|*.PNG|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FileStream stream = new FileStream(saveFileDialog.FileName, FileMode.Create);
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Interlace = PngInterlaceOption.Off;
                    encoder.Frames.Add(BitmapFrame.Create(formatCoversion.ToBitmapSource(currentFrame)));
                    encoder.Save(stream);
                    stream.Close();
                    text1.Text = "The image was saved to your computer.";
                }
            }
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            webcamDevice = comboBox1.SelectedIndex;

            button2.Content = "Video Streaming";
            button3.IsEnabled = false;
            imageBox1.Source = null;

            if (captureInProgress)
            {
                webcam.stopTimer();
                captureInProgress = !captureInProgress;
            }

            if (webcam != null)
            { 
                webcam.releaseCamera();
                webcam = null;
            }
        }
    }
}
