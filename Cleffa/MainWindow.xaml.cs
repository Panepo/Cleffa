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
        private Mat currentFrame = new Mat();
        private cameraDevice[] webcams;
        private int webcamDevice = 0;
        private barcodeIdentifier identify;
        private Stopwatch watch;

        public MainWindow() 
        {
            InitializeComponent();

            DsDevice[] systemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            webcams = new cameraDevice[systemCameras.Length];

            for (int i = 0; i < systemCameras.Length; i++)
            {
                webcams[i] = new cameraDevice(i, systemCameras[i].Name, systemCameras[i].ClassID);
                comboBoxDevice.Items.Add(webcams[i].ToStringS());
            }

            if (comboBoxDevice.Items.Count > 0)
            {
                comboBoxDevice.SelectedIndex = 0;
                buttonCamera.IsEnabled = true;
            }

            identify = new barcodeIdentifier();
            watch = new Stopwatch();
        }

        private void barcodeDetect(Bitmap image)
        {  
            identify.setInputBitmap(image);

            if (identify.genDecode())
            {
                textSystem.Text = "Code analysis successed";
                textBarcode.Text = identify.getResultString();
                textType.Text = identify.getResultType();

                if (identify.genPaint())
                    imageBoxDisp.Source = identify.getResultBitmapSource();


                if (captureInProgress)
                {
                    buttonCamera.Content = "Camera Restart";
                    webcam.stopTimer();
                    webcam.releaseCamera();
                    webcam = null;
                    captureInProgress = !captureInProgress;
                    sliderBrightness.IsEnabled = false;
                    sliderSharpness.IsEnabled = false;
                    sliderContrast.IsEnabled = false;
                }
            }
            else
            {
                textSystem.Text = "Code analysis failed";
                textBarcode.Text = String.Empty;
                textType.Text = String.Empty;
            }

        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            currentFrame = webcam.frameCapture();

            if (currentFrame != null)
            {
                watch.Reset();
                watch.Start();
                imageBoxDisp.Source = formatCoversion.ToBitmapSource(currentFrame);

                identify.reader.AutoRotate = false;
                identify.reader.Options.TryHarder = false;
                barcodeDetect(currentFrame.Bitmap);

                watch.Stop();
                textTime.Text = watch.Elapsed.TotalMilliseconds.ToString() + "ms";
            }   
        }

        private void buttonImage_Click(object sender, RoutedEventArgs e)
        {
            if (captureInProgress)
            {
                buttonCamera.Content = "Camera Restart";
                buttonSave.IsEnabled = false;
                webcam.stopTimer();
                webcam.releaseCamera();
                webcam = null;
                captureInProgress = !captureInProgress;
                sliderBrightness.IsEnabled = false;
                sliderSharpness.IsEnabled = false;
                sliderContrast.IsEnabled = false;
            }

            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                watch.Reset();
                watch.Start();
                BitmapImage input = new BitmapImage(new Uri(open.FileName));

                if (input != null)
                {
                    imageBoxDisp.Source = input;
                    currentFrame = null;
                    Bitmap image = formatCoversion.BitmapImage2Bitmap(input);

                    identify.reader.AutoRotate = true;
                    identify.reader.Options.TryHarder = true;
                    barcodeDetect(image);

                    watch.Stop();
                    textTime.Text = watch.Elapsed.TotalMilliseconds.ToString() + "ms";
                }
                else
                {
                    textSystem.Text = "Load image failed";
                    textBarcode.Text = String.Empty;
                    textTime.Text = String.Empty;
                }
            }
        }

        private void buttonCamera_Click(object sender, RoutedEventArgs e)
        {
            if (webcam == null)
            {
                webcam = new cameraControl(webcamDevice, ProcessFrame);

                sliderBrightness.IsEnabled = true;
                sliderSharpness.IsEnabled = true;
                sliderContrast.IsEnabled = true;
                sliderBrightness.Value = webcam.getParameter("Brightness");
                sliderSharpness.Value = webcam.getParameter("Sharpness");
                sliderContrast.Value = webcam.getParameter("Contrast");
            }

            if (captureInProgress)
            {
                buttonCamera.Content = "Camera Restart";
                webcam.stopTimer();
            }
            else
            {
                buttonCamera.Content = "Camera Stop";
                webcam.startTimer();
                buttonSave.IsEnabled = true;
            }
            captureInProgress = !captureInProgress;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (currentFrame == null)
            {
                textSystem.Text = "There is no image to save.";
            }
            else
            {
                if (captureInProgress)
                {
                    buttonCamera.Content = "Camera Restart";
                    webcam.stopTimer();
                    webcam.releaseCamera();
                    webcam = null;
                    captureInProgress = !captureInProgress;
                    sliderBrightness.IsEnabled = false;
                    sliderSharpness.IsEnabled = false;
                    sliderContrast.IsEnabled = false;
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
                    textSystem.Text = "The image was saved to your computer.";
                }
            }
        }

        private void comboBoxDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            webcamDevice = comboBoxDevice.SelectedIndex;

            buttonCamera.Content = "Video Streaming";
            buttonSave.IsEnabled = false;
            imageBoxDisp.Source = null;

            if (captureInProgress)
            {
                webcam.stopTimer();
                captureInProgress = !captureInProgress;
            }

            if (webcam != null)
            { 
                webcam.releaseCamera();
                webcam = null;
                sliderBrightness.IsEnabled = false;
                sliderSharpness.IsEnabled = false;
                sliderContrast.IsEnabled = false;
            }
        }

        private void sliderBrightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (webcam != null)
                webcam.setParameter("Brightness", sliderBrightness.Value);
        }

        private void sliderSharpness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (webcam != null)
                webcam.setParameter("Sharpness", sliderSharpness.Value);
        }

        private void sliderContrast_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (webcam != null)
                webcam.setParameter("Contrast", sliderContrast.Value);
        }
    }
}
