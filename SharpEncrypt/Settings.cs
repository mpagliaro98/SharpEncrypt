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
        public const string VERSION_NUMBER = "1.1.0";
        private const string DIR_NAME = "settings";
        private const string SETTINGS_FILE = "settings.dat";

        private bool logToFile = false;
        private string pathToMasterKey = "";
        private byte[] masterKey;

        public bool LogToFile
        {
            get { return logToFile; }
            set
            {
                logToFile = value;
                SaveToFile();
            }
        }

        public string PathToMasterKey
        {
            get { return pathToMasterKey; }
            set
            {
                pathToMasterKey = value;
                SaveToFile();
                masterKey = null;
                if (MasterKeyFileExists())
                    LoadMasterKey();
            }
        }

        public byte[] MasterKey
        {
            get { return masterKey; }
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
            string[] parts = text.Split('\n');
            logToFile = bool.Parse(parts[0]);
            PathToMasterKey = parts.Length > 1 ? parts[1] : "";
            System.Diagnostics.Debug.WriteLine("Settings loaded - " + filename);
        }

        private void SaveToFile()
        {
            string text = logToFile.ToString() + "\n" + pathToMasterKey;
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

        public bool MasterKeyFileExists()
        {
            return pathToMasterKey != "" && File.Exists(pathToMasterKey);
        }

        private void LoadMasterKey()
        {
            masterKey = File.ReadAllBytes(pathToMasterKey);
            if (masterKey.Length != AesCryptographyService.DEFAULT_BLOCK_SIZE)
                throw new SharpEncryptException("Master key must be " + AesCryptographyService.DEFAULT_BLOCK_SIZE.ToString() + " bytes long, got " + masterKey.Length.ToString());
        }
    }
}
