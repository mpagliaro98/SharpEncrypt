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
        private readonly OutputBuffer buffer;

        public OutputBuffer OutputBuffer
        {
            get { return buffer; }
        }

        public WorkTracker(IWorkTracker tracker, OutputBuffer buffer)
        {
            this.tracker = tracker;
            this.buffer = buffer;
        }

        public void ReportProgress(double progress)
        {
            tracker.TrackProgress(progress);
        }
    }
}
