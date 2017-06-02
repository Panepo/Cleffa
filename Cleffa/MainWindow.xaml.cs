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

namespace Cleffa
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() 
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BitmapImage image = new BitmapImage(new Uri(open.FileName));

                if (image != null)
                {
                    imageBox1.Source = image;
                    string decoded = App.decode(image);
                    if (decoded != null)
                    {
                        text1.Text = decoded;
                    }
                    else
                    {
                        text1.Text = "Code analysis failed";
                    }
                }
                else
                {
                    text1.Text = "Load image failed";
                }
            }
        }
    }
}
