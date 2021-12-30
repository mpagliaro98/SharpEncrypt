using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace SharpEncrypt
{
    public static class RegistryManager
    {
        private const string REG_ALL_ENCRYPT = "*\\shell\\SharpEncryptAllEnc";
        private const string REG_SENC_DECRYPT = "SystemFileAssociations\\.senc\\shell\\SharpEncryptDec";
        private const string REG_DIR_DECRYPT = "Directory\\shell\\SharpEncryptDirDec";
        private const string REG_DIR_ENCRYPT = "Directory\\shell\\SharpEncryptDirEnc";

        public static void CreateMenuItems()
        {
            CreateSubKey(REG_ALL_ENCRYPT, "Encrypt");
            CreateSubKeyCommand(REG_ALL_ENCRYPT, "\"" + Assembly.GetEntryAssembly().Location + "\" \"%1\"");
            CreateSubKey(REG_SENC_DECRYPT, "Decrypt");
            CreateSubKeyCommand(REG_SENC_DECRYPT, "\"" + Assembly.GetEntryAssembly().Location + "\" \"%1\"");
            CreateSubKey(REG_DIR_DECRYPT, "Decrypt");
            CreateSubKeyCommand(REG_DIR_DECRYPT, "\"" + Assembly.GetEntryAssembly().Location + "\" \"%1\"");
            CreateSubKey(REG_DIR_ENCRYPT, "Encrypt");
            CreateSubKeyCommand(REG_DIR_ENCRYPT, "\"" + Assembly.GetEntryAssembly().Location + "\" \"%1\"");
        }

        public static void DeleteMenuItems()
        {
            DeleteSubKeyCommand(REG_DIR_ENCRYPT);
            DeleteSubKey(REG_DIR_ENCRYPT);
            DeleteSubKeyCommand(REG_DIR_DECRYPT);
            DeleteSubKey(REG_DIR_DECRYPT);
            DeleteSubKeyCommand(REG_SENC_DECRYPT);
            DeleteSubKey(REG_SENC_DECRYPT);
            DeleteSubKeyCommand(REG_ALL_ENCRYPT);
            DeleteSubKey(REG_ALL_ENCRYPT);
        }

        public static bool DoMenuItemsExist()
        {
            return DoesSubKeyExist(REG_ALL_ENCRYPT) &&
                DoesSubKeyCommandExist(REG_ALL_ENCRYPT) &&
                DoesSubKeyExist(REG_SENC_DECRYPT) &&
                DoesSubKeyCommandExist(REG_SENC_DECRYPT) &&
                DoesSubKeyExist(REG_DIR_DECRYPT) &&
                DoesSubKeyCommandExist(REG_DIR_DECRYPT) &&
                DoesSubKeyExist(REG_DIR_ENCRYPT) &&
                DoesSubKeyCommandExist(REG_DIR_ENCRYPT);
        }

        private static void CreateSubKey(string subkey, string text)
        {
            RegistryKey regmenu = null;
            try
            {
                regmenu = Registry.ClassesRoot.CreateSubKey(subkey);
                if (regmenu != null)
                {
                    regmenu.SetValue("", text);
                    regmenu.SetValue("Icon", Assembly.GetEntryAssembly().Location);
                }
            }
            catch (Exception e)
            {
                if (regmenu != null) regmenu.Close();
                throw e;
            }
            if (regmenu != null) regmenu.Close();
        }

        private static void CreateSubKeyCommand(string subkey, string command)
        {
            CreateSubKey(subkey + "\\command", command);
        }

        private static void DeleteSubKey(string subkey)
        {
            RegistryKey reg = Registry.ClassesRoot.OpenSubKey(subkey);
            if (reg != null)
            {
                reg.Close();
                Registry.ClassesRoot.DeleteSubKey(subkey);
            }
        }

        private static void DeleteSubKeyCommand(string subkey)
        {
            DeleteSubKey(subkey + "\\command");
        }

        private static bool DoesSubKeyExist(string subkey)
        {
            RegistryKey reg = Registry.ClassesRoot.OpenSubKey(subkey);
            return reg != null;
        }

        private static bool DoesSubKeyCommandExist(string subkey)
        {
            return DoesSubKeyExist(subkey + "\\command");
        }
    }
}
