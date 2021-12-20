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
        private byte[] loadedFile;
        private string filename = "";

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
                loadedFile = File.ReadAllBytes(openFileDialog.FileName);
                filename = openFileDialog.FileName;
                UpdateUI();
            }
        }

        private void Reset()
        {
            loadedFile = null;
            filename = "";
            textboxPassword.Text = "";
            progressBar.Value = progressBar.Minimum;
            UpdateUI();
        }

        private void UpdateUI()
        {
            labelName.Content = "Selected: " + filename;
            btnEncrypt.IsEnabled = filename.Length > 0;
            btnDecrypt.IsEnabled = filename.Length > 0;
        }

        private void btnEncrypt_Click(object sender, RoutedEventArgs e)
        {
            if (textboxPassword.Text.Trim().Length <= 0)
            {
                MessageBox.Show("A password is required.");
                return;
            }

            byte[] result = new byte[loadedFile.Length % AesCryptographyService.BLOCK_SIZE == 0 ? loadedFile.Length :
                loadedFile.Length + (AesCryptographyService.BLOCK_SIZE - (loadedFile.Length % AesCryptographyService.BLOCK_SIZE))];
            var aes = new AesCryptographyService(textboxPassword.Text.Trim());
            for (int i = 0; i < loadedFile.Length; i += AesCryptographyService.BLOCK_SIZE)
            {
                progressBar.Value = (i / loadedFile.Length) * 100;
                byte[] current = new byte[AesCryptographyService.BLOCK_SIZE];
                for (int j = 0; j < AesCryptographyService.BLOCK_SIZE; ++j)
                {
                    if (i + j < loadedFile.Length)
                        current[j] = loadedFile[i + j];
                    else
                        current[j] = 0;  // pad final block with zeroes
                }
                byte[] encrypted = aes.Encrypt(current);
                for (int j = 0; j < AesCryptographyService.BLOCK_SIZE; ++j)
                {
                    if (i + j < result.Length)
                        result[i + j] = encrypted[j];
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("WARNING: Final block is not " + AesCryptographyService.BLOCK_SIZE.ToString() + " bytes.");
                        break;
                    }
                }
            }

            progressBar.Value = progressBar.Maximum;
            File.WriteAllBytes(filename, loadedFile);
            MessageBox.Show("File successfully encrypted.");
            Reset();
        }

        private void btnDecrypt_Click(object sender, RoutedEventArgs e)
        {
            if (textboxPassword.Text.Trim().Length <= 0)
            {
                MessageBox.Show("A password is required.");
                return;
            }

            byte[] result = new byte[loadedFile.Length % AesCryptographyService.BLOCK_SIZE == 0 ? loadedFile.Length :
                loadedFile.Length + (AesCryptographyService.BLOCK_SIZE - (loadedFile.Length % AesCryptographyService.BLOCK_SIZE))];
            var aes = new AesCryptographyService(textboxPassword.Text.Trim());
            for (int i = 0; i < loadedFile.Length; i += AesCryptographyService.BLOCK_SIZE)
            {
                progressBar.Value = (i / loadedFile.Length) * 100;
                byte[] current = new byte[AesCryptographyService.BLOCK_SIZE];
                for (int j = 0; j < AesCryptographyService.BLOCK_SIZE; ++j)
                {
                    if (i + j < loadedFile.Length)
                        current[j] = loadedFile[i + j];
                    else
                        current[j] = 0;
                }
                byte[] encrypted = aes.Decrypt(current);
                for (int j = 0; j < AesCryptographyService.BLOCK_SIZE; ++j)
                {
                    if (i + j < result.Length)
                        result[i + j] = encrypted[j];
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("WARNING: Final block is not " + AesCryptographyService.BLOCK_SIZE.ToString() + " bytes.");
                        break;
                    }
                }
            }

            progressBar.Value = progressBar.Maximum;
            File.WriteAllBytes(filename, loadedFile);
            MessageBox.Show("File successfully decrypted.");
            Reset();
        }
    }
}
