using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SharpEncrypt
{
    public class Settings
    {
        private const string DIR_NAME = "settings";
        private const string SETTINGS_FILE = "settings.dat";

        private bool logToFile = false;

        public bool LogToFile
        {
            get { return logToFile; }
            set
            {
                logToFile = value;
                SaveToFile();
            }
        }

        public Settings()
        {
            LoadFromFile();
        }

        private string GetFilename()
        {
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string result = Path.Combine(dir, DIR_NAME);
            result = Path.Combine(result, SETTINGS_FILE);
            return result;
        }

        private void LoadFromFile()
        {
            string filename = GetFilename();
            if (!File.Exists(filename))
                return;
            string text = File.ReadAllText(filename);
            logToFile = bool.Parse(text);
            System.Diagnostics.Debug.WriteLine("Settings loaded - " + filename);
        }

        private void SaveToFile()
        {
            string text = logToFile.ToString();
            string filename = GetFilename();
            if (!File.Exists(filename))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
                FileStream fs = File.Create(filename);
                fs.Close();
            }
            File.WriteAllText(filename, text);
            System.Diagnostics.Debug.WriteLine("Settings saved - " + filename);
        }
    }
}
