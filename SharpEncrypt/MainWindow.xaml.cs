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
        private List<FileEncryptor> fileEncryptors = new List<FileEncryptor>();

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
                fileEncryptors.Clear();
                foreach (string filename in openFileDialog.FileNames)
                {
                    fileEncryptors.Add(new FileEncryptor(filename));
                }
                UpdateUI();
            }
        }

        private void Reset()
        {
            fileEncryptors.Clear();
            textboxPassword.Text = "";
            progressBar.Value = progressBar.Minimum;
            UpdateUI();
        }

        private void UpdateUI()
        {
            labelName.Content = "Selected: " + string.Join(", ", fileEncryptors.Select(fe => System.IO.Path.GetFileName(fe.Filepath)));
            btnEncrypt.IsEnabled = fileEncryptors.Count > 0;
            btnDecrypt.IsEnabled = fileEncryptors.Count > 0;
        }

        private void btnEncrypt_Click(object sender, RoutedEventArgs e)
        {
            string password = textboxPassword.Text.Trim();
            if (password.Length <= 0)
            {
                MessageBox.Show("A password is required.");
                return;
            }

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWorkEncrypt;
            worker.ProgressChanged += worker_ProgressChangedEncrypt;
            worker.RunWorkerCompleted += worker_RunWorkerCompletedEncrypt;
            worker.RunWorkerAsync(argument: new Tuple<List<FileEncryptor>, string, bool>(fileEncryptors, password, checkboxEncryptFilename.IsChecked.Value));
            PrimaryWindow.IsEnabled = false;
        }

        private void worker_DoWorkEncrypt(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Tuple<List<FileEncryptor>, string, bool> args = e.Argument as Tuple<List<FileEncryptor>, string, bool>;
            List<FileEncryptor> fileEncryptors = args.Item1;
            string password = args.Item2;
            bool encryptFilename = args.Item3;

            string output = "";
            foreach (FileEncryptor fileEncryptor in fileEncryptors)
            {
                output += System.IO.Path.GetFileName(fileEncryptor.Filepath) + ": ";
                var result = fileEncryptor.EncryptFile(password, encryptFilename, worker);
                if (result)
                    output += "Successfully encrypted.";
                else
                    output += "Something went wrong.";
                output += "\n";
            }
            e.Result = output;
        }

        private void worker_ProgressChangedEncrypt(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void worker_RunWorkerCompletedEncrypt(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show(e.Result.ToString());
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

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWorkDecrypt;
            worker.ProgressChanged += worker_ProgressChangedDecrypt;
            worker.RunWorkerCompleted += worker_RunWorkerCompletedDecrypt;
            worker.RunWorkerAsync(argument: new Tuple<List<FileEncryptor>, string>(fileEncryptors, password));
            PrimaryWindow.IsEnabled = false;
        }

        private void worker_DoWorkDecrypt(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Tuple<List<FileEncryptor>, string> args = e.Argument as Tuple<List<FileEncryptor>, string>;
            List<FileEncryptor> fileEncryptors = args.Item1;
            string password = args.Item2;

            string output = "";
            foreach (FileEncryptor fileEncryptor in fileEncryptors)
            {
                output += System.IO.Path.GetFileName(fileEncryptor.Filepath) + ": ";
                var result = fileEncryptor.DecryptFile(password, worker);
                if (result)
                    output += "Successfully decrypted.";
                else
                    output += fileEncryptor.Message;
                output += "\n";
            }
            e.Result = output;
        }

        private void worker_ProgressChangedDecrypt(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void worker_RunWorkerCompletedDecrypt(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show(e.Result.ToString());
            PrimaryWindow.IsEnabled = true;
            Reset();
        }
    }
}
