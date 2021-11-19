using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using MessageBox = System.Windows.MessageBox;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using System.Windows.Forms;
using System.ComponentModel;

namespace PKAD___Batch_Precinct_Cadence_Report
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BatchPrecinctRenderer batchPrecinctRenderer = null;
        private BatchPrecinctManager batchPrecinctManager = null;
        private int currentChartIndex = 0;
        string exportFolderPath = "";

        public MainWindow()
        {
            batchPrecinctRenderer = null;
            batchPrecinctManager = null;

            InitializeComponent();
            seeNext.Visibility = Visibility.Hidden;
            seePrevious.Visibility = Visibility.Hidden;

        }
        private void myCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            Render();
        }
        private void myCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Render();
        }
        void Render()
        {
            if (batchPrecinctRenderer == null)
            {
                batchPrecinctRenderer = new BatchPrecinctRenderer((int)myCanvas.ActualWidth, (int)myCanvas.ActualHeight);
            }

            int printerCount = batchPrecinctRenderer.getPrintersCount();


            if ( printerCount > 70)
            {

                seeNext.Visibility = Visibility.Visible;
                seePrevious.Visibility = Visibility.Visible;
                seePrevious.IsEnabled = false;

            }

            batchPrecinctRenderer.setRenderSize((int)myCanvas.ActualWidth, (int)myCanvas.ActualHeight);
            batchPrecinctRenderer.draw(currentChartIndex);
            myImage.Source = BmpImageFromBmp(batchPrecinctRenderer.getBmp());
        }

        private void btnImportCSV_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";


            if (openFileDialog.ShowDialog() == true)
            {
                if (batchPrecinctManager == null) batchPrecinctManager = new BatchPrecinctManager(openFileDialog.FileName);
                else batchPrecinctManager.setInputfile(openFileDialog.FileName);

                List<BatchPrecinctData> data = batchPrecinctManager.readData();
                csvFilepath.Text = openFileDialog.FileName;
                //csvFilepath.Width = 1000;
                if (data != null)
                {
                    Dictionary<string, int> printers = new Dictionary<string, int>();
                    Dictionary<string, int> left_info_dic = new Dictionary<string, int>();
                    List<BatchPrecinctData> sorted = new List<BatchPrecinctData>();
                    sorted = data.OrderByDescending(i => i.ied).ThenByDescending(i => i.got).ToList();

                    foreach (var item in sorted)
                    {
                        if (string.IsNullOrEmpty(item.printer_id)) continue;
                        if (item.printer_id == "0") continue;

                        if (printers.ContainsKey(item.printer_id))
                        {
                            printers[item.printer_id]++;
                        }
                        else printers[item.printer_id] = 1;

                    }

                    foreach (var item in data)
                    {
                        if (string.IsNullOrEmpty(item.left_info)) continue;
                        string[] words = item.left_info.Split(' ');
                        if (words.Length != 2) continue;
                        if (!Regex.IsMatch(words[1], @"^[a-zA-Z]+$")) continue;

                        if (left_info_dic.ContainsKey(words[1]))
                        {
                            left_info_dic[words[1]]++;
                        }
                        else left_info_dic[words[1]] = 1;

                    }
                    batchPrecinctRenderer.setChatData(sorted, printers, left_info_dic);

                    currentChartIndex = 0;
                    seeNext.Visibility = Visibility.Hidden;
                    seePrevious.Visibility = Visibility.Hidden;

                    Render();

                }
                else
                {
                    string msg = batchPrecinctManager.getLastException();

                    if (msg == "System.IO.IOException")
                        MessageBox.Show("The file is open by another process", "Error");
                    else if (msg == "CsvHelper.TypeConversion.TypeConverterException")
                    {
                        MessageBox.Show("The file format is invalid, Please check your csv file again.", "Error");
                    }
                    else if (msg == "CsvHelper.HeaderValidationException")
                    {
                        MessageBox.Show("CSV Header is not correct, please make sure you are using the correct CSV file", "Error");
                    }
                }

            }
        }
        private void btnExportChart_Click(object sender, RoutedEventArgs e)
        {


            if (batchPrecinctRenderer == null)
            {
                batchPrecinctRenderer = new BatchPrecinctRenderer((int)myCanvas.ActualWidth, (int)myCanvas.ActualHeight);
            }
            if (batchPrecinctRenderer.getPrintersCount() > 0)
            {

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Image file (*.png)|*.png";
                //saveFileDialog.Filter = "Image file (*.png)|*.png|PDF file (*.pdf)|*.pdf";
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filename = saveFileDialog.FileName;
                    exportFolderPath = saveFileDialog.FileName;

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.WorkerReportsProgress = true;
                    worker.DoWork += worker_DoExport;
                    worker.ProgressChanged += worker_ProgressChanged;
                    worker.RunWorkerAsync();
                    worker.RunWorkerCompleted += worker_CompletedWork;

                }

            }


        }
        private void SaveBitmapImagetoFile(BitmapImage image, string filePath)
        {
            //PngBitmapEncoder encoder1 = new PngBitmapEncoder();
            //encoder1.Frames.Add(BitmapFrame.Create(image));

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            try
            {
                using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
            catch (Exception ex)
            {

            }


        }
        void worker_DoExport(object sender, DoWorkEventArgs e)
        {
            int chartCount = getChartCount();
            for (int index = 0; index < chartCount; index++)
            {
                batchPrecinctRenderer.draw(index);

                string filename = exportFolderPath.Substring(0, exportFolderPath.Length - 4) + "-" + index.ToString() + ".png";
                SaveBitmapImagetoFile(BmpImageFromBmp(batchPrecinctRenderer.getBmp()), filename);
            }

        }
        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }
        void worker_CompletedWork(object sender, RunWorkerCompletedEventArgs e)
        {
            string msg = "Exporting has been done\n";
            MessageBox.Show(msg);
        }

        private int getChartCount()
        {
            int printerCount = batchPrecinctRenderer.getPrintersCount();

            int chartCount = 1;
            chartCount = chartCount + printerCount / 70;
            if (printerCount % 70 == 0) chartCount--;
            return chartCount;
        }
        private void drawPreviousChart(object sender, RoutedEventArgs e)
        {

            int chartCount = getChartCount();

            currentChartIndex--;

            if (currentChartIndex == 0)
            {
                seePrevious.IsEnabled = false;
                seeNext.IsEnabled = true;
            }
            else if (currentChartIndex == chartCount - 1)
            {
                seePrevious.IsEnabled = true;
                seeNext.IsEnabled = false;
            }
            else
            {
                seePrevious.IsEnabled = true;
                seeNext.IsEnabled = true;
            }
            batchPrecinctRenderer.draw(currentChartIndex);
            myImage.Source = BmpImageFromBmp(batchPrecinctRenderer.getBmp());
        }
        private void drawNextChart(object sender, RoutedEventArgs e)
        {

            int chartCount = getChartCount();
            currentChartIndex++;

            if (currentChartIndex == 0)
            {
                seePrevious.IsEnabled = false;
                seeNext.IsEnabled = true;
            }
            else if (currentChartIndex == chartCount - 1)
            {
                seePrevious.IsEnabled = true;
                seeNext.IsEnabled = false;
            }
            else
            {
                seePrevious.IsEnabled = true;
                seeNext.IsEnabled = true;
            }
            batchPrecinctRenderer.draw(currentChartIndex);
            myImage.Source = BmpImageFromBmp(batchPrecinctRenderer.getBmp());
        }


        private void SaveControlImage(FrameworkElement control, string filename)
        {
            RenderTargetBitmap rtb = (RenderTargetBitmap)CreateBitmapFromControl(control);
            // Make a PNG encoder.
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            // Save the file.
            using (FileStream fs = new FileStream(filename,
                FileMode.Create, FileAccess.Write, FileShare.None))
            {
                encoder.Save(fs);
            }
        }
        public BitmapSource CreateBitmapFromControl(FrameworkElement element)
        {
            // Get the size of the Visual and its descendants.
            Rect rect = VisualTreeHelper.GetDescendantBounds(element);

            // Make a DrawingVisual to make a screen
            // representation of the control.
            DrawingVisual dv = new DrawingVisual();

            // Fill a rectangle the same size as the control
            // with a brush containing images of the control.
            using (DrawingContext ctx = dv.RenderOpen())
            {
                VisualBrush brush = new VisualBrush(element);
                ctx.DrawRectangle(brush, null, new Rect(rect.Size));
            }

            // Make a bitmap and draw on it.
            int width = (int)element.ActualWidth;
            int height = (int)element.ActualHeight;
            RenderTargetBitmap rtb = new RenderTargetBitmap(
                width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);
            return rtb;
        }

        private BitmapImage BmpImageFromBmp(Bitmap bmp)
        {
            using (var memory = new System.IO.MemoryStream())
            {
                bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

    }
}
