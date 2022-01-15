using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SharpEncrypt
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const int MAX_PATH_LENGTH = 260;

        private MainWindow mainWindow;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            mainWindow = new MainWindow();
            if (e.Args.Length == 1)
            {
                string filename = e.Args[0];
                var processes = Process.GetProcessesByName("SharpEncrypt");
                if (processes.Length > 1)
                {
                    SendToClientPipe(filename);
                    Environment.Exit(0);
                }
                else
                {
                    mainWindow.SetIncomingPath(filename);
                }
            }
            SetUpBackgroundPipe();
            mainWindow.Show();
        }

        private void SetUpBackgroundPipe()
        {
            var ts = new ThreadStart(CheckForCommunication);
            var backgroundThread = new Thread(ts);
            backgroundThread.Start();
        }

        private void SendToClientPipe(string filename)
        {
            using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("sharpencrypt-filename-pipe"))
            {
                Debug.WriteLine("[SERVER] Current TransmissionMode: {0}.", pipeServer.TransmissionMode);
                pipeServer.WaitForConnection();
                pipeServer.Write(Util.StringEncoding.GetBytes(filename), 0, filename.Length);
                Debug.WriteLine("[SERVER] Sent: " + filename);
            }
        }

        private void CheckForCommunication()
        {
            while (true)
            {
                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream("sharpencrypt-filename-pipe"))
                {
                    Debug.WriteLine("[CLIENT] Waiting for connection...");
                    pipeClient.Connect();
                    Debug.WriteLine("[CLIENT] Server found");
                    byte[] buffer = new byte[MAX_PATH_LENGTH];
                    pipeClient.Read(buffer, 0, MAX_PATH_LENGTH);
                    string filename = Util.StringEncoding.GetString(buffer).Replace("\0", String.Empty);
                    Debug.WriteLine("[CLIENT] Received: " + filename);
                    mainWindow.Dispatcher.Invoke(() => mainWindow.SetIncomingPath(filename));
                }
            }
        }
    }
}
