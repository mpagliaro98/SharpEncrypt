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
        //private byte[] loadedFile;
        //private string filename = "";
        private FileEncryptor fileEncryptor = new FileEncryptor();

        public MainWindow()
        {
            InitializeComponent();
            UpdateUI();
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                // TODO: WORK WITH FOLDERS AND FILES
                fileEncryptor = new FileEncryptor(openFileDialog.FileName);
                UpdateUI();
            }
        }

        private void Reset()
        {
            fileEncryptor = new FileEncryptor();
            textboxPassword.Text = "";
            progressBar.Value = progressBar.Minimum;
            UpdateUI();
        }

        private void UpdateUI()
        {
            labelName.Content = "Selected: " + fileEncryptor.Filepath;
            btnEncrypt.IsEnabled = fileEncryptor.Filepath.Length > 0;
            btnDecrypt.IsEnabled = fileEncryptor.Filepath.Length > 0;
        }

        private void btnEncrypt_Click(object sender, RoutedEventArgs e)
        {
            string password = textboxPassword.Text.Trim();
            if (password.Length <= 0)
            {
                MessageBox.Show("A password is required.");
                return;
            }

            var result = fileEncryptor.EncryptFile(password, checkboxEncryptFilename.IsChecked.Value);
            if (!result)
            {
                MessageBox.Show("Something went wrong.");
                return;
            }

            MessageBox.Show("File successfully encrypted.");
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

            var result = fileEncryptor.DecryptFile(password);
            if (!result)
            {
                MessageBox.Show(fileEncryptor.Message);
                return;
            }

            MessageBox.Show("File successfully decrypted.");
            Reset();
        }
    }
}
