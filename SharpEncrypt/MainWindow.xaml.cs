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

            string output = "";
            foreach (FileEncryptor fileEncryptor in fileEncryptors)
            {
                output += System.IO.Path.GetFileName(fileEncryptor.Filepath) + ": ";
                var result = fileEncryptor.EncryptFile(password, checkboxEncryptFilename.IsChecked.Value);
                if (result)
                    output += "Successfully encrypted.";
                else
                    output += "Something went wrong.";
                output += "\n";
            }
            MessageBox.Show(output);

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

            string output = "";
            foreach (FileEncryptor fileEncryptor in fileEncryptors)
            {
                output += System.IO.Path.GetFileName(fileEncryptor.Filepath) + ": ";
                var result = fileEncryptor.DecryptFile(password);
                if (result)
                    output += "Successfully decrypted.";
                else
                    output += fileEncryptor.Message;
                output += "\n";
            }
            MessageBox.Show(output);

            Reset();
        }
    }
}
