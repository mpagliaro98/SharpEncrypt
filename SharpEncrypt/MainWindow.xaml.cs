using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using Microsoft.Win32;

namespace SharpEncrypt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SharpEncryptModel model = new SharpEncryptModel();

        public MainWindow()
        {
            InitializeComponent();
            UpdateUI();
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                // TODO: WORK WITH FOLDERS AND FILES
                model.ClearFiles();
                foreach (string filename in openFileDialog.FileNames)
                {
                    model.AddFile(filename);
                }
                UpdateUI();
            }
        }

        private void Reset()
        {
            model.ClearFiles();
            textboxPassword.Text = "";
            progressBar.Value = progressBar.Minimum;
            UpdateUI();
        }

        private void UpdateUI()
        {
            listboxFiles.ClearItems();
            foreach (FileInfo file in model.Files)
            {
                TextBlock tb = new TextBlock()
                {
                    Text = file.FileName + " (" + string.Format("{0:0.00}", file.FileSizeMB) + "MB)",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness() { Left = 5, Right = 5 }
                };
                listboxFiles.AddItem(tb);
            }
            btnEncrypt.IsEnabled = model.NumFiles > 0;
            btnDecrypt.IsEnabled = model.NumFiles > 0;
        }

        private void btnEncrypt_Click(object sender, RoutedEventArgs e)
        {
            string password = textboxPassword.Text.Trim();
            if (password.Length <= 0)
            {
                MessageBox.Show("A password is required.");
                return;
            }

            BackgroundWorkerTracker worker = new BackgroundWorkerTracker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWorkEncrypt;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync(argument: new Tuple<string, bool>(password, checkboxEncryptFilename.IsChecked.Value));
            PrimaryWindow.IsEnabled = false;
        }

        private void worker_DoWorkEncrypt(object sender, DoWorkEventArgs e)
        {
            BackgroundWorkerTracker worker = sender as BackgroundWorkerTracker;
            Tuple<string, bool> args = e.Argument as Tuple<string, bool>;
            string password = args.Item1;
            bool encryptFilename = args.Item2;
            OutputBuffer buffer = new OutputBuffer(textblockOutput);
            WorkTracker tracker = new WorkTracker(worker);
            model.EncryptAllFiles(password, encryptFilename, tracker, buffer);
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Task complete.");
            PrimaryWindow.IsEnabled = true;
            Reset();
        }

        private void btnDecrypt_Click(object sender, RoutedEventArgs e)
        {
            string password = textboxPassword.Text.Trim();
            if (password.Length <= 0)
            {
                MessageBox.Show("A password is required.");
                return;
            }

            BackgroundWorkerTracker worker = new BackgroundWorkerTracker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWorkDecrypt;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync(argument: password);
            PrimaryWindow.IsEnabled = false;
        }

        private void worker_DoWorkDecrypt(object sender, DoWorkEventArgs e)
        {
            BackgroundWorkerTracker worker = sender as BackgroundWorkerTracker;
            string password = e.Argument as string;
            OutputBuffer buffer = new OutputBuffer(textblockOutput);
            WorkTracker tracker = new WorkTracker(worker);
            model.DecryptAllFiles(password, tracker, buffer);
        }
    }
}
