using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;

namespace ASCIIArtWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            inputFile = "Select an image file to use for ASCII Art";
        }

        private string inputFile;
        public string InputFile
        {
            get { return inputFile; }
            set
            {
                if (value != inputFile)
                {
                    inputFile = value;
                    RaisePropertyChanged("InputFile");
                }
            }
        }

        async private void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files (*.jpg, *.png)|*.jpg;*.png;";
            ofd.Title = "Select an image file to convert";

            try
            {
                bool? bResult = ofd.ShowDialog();
                if (bResult.HasValue && (bool)bResult)
                {
                    InputFile = ofd.FileName;
                    await Task.Run(() => DoGenerate(InputFile));
                }
            }
            catch (Exception xx)
            {
                MessageBox.Show(string.Format("Fatal Error: {0}", xx.ToString()), "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DoGenerate(string inputFile)
        {
            int outputWidth = 100;

            FileInfo fi = new FileInfo(inputFile);
            if (!fi.Exists)
                throw new Exception(string.Format("File {0} not found", inputFile));
            string outputFile = Path.Combine(fi.DirectoryName, Path.GetFileNameWithoutExtension(inputFile) + ".txt");

            Bitmap bmInput = new Bitmap(inputFile);

            if (outputWidth > bmInput.Width)
                throw new Exception("Output width must be <= pixel width of image");

            // Generate the ASCII art
            AscArt.GenerateAsciiArt(bmInput, outputFile, outputWidth);

            // This will fire up default app for .txt files
            Process.Start(outputFile);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
