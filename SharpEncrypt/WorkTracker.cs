using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpEncrypt
{
    public class WorkTracker
    {
        private readonly IWorkTracker tracker;

        public WorkTracker(IWorkTracker tracker)
        {
            this.tracker = tracker;
        }

        public void ReportProgress(double progress)
        {
            tracker.TrackProgress(progress);
        }
    }
}
