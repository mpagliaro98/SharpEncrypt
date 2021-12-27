using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpEncrypt
{
    public class BackgroundWorkerTracker : BackgroundWorker, IWorkTracker
    {
        public void TrackProgress(double progress)
        {
            ReportProgress(Convert.ToInt32(progress));
        }
    }
}
