using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shuttle.Scheduling.FileDataAccess
{
    public class ScheduleStorageItem
    {
        public string Id { get; set; }
        public string CronString { get; set; }
        public string QueueConnectionString { get; set; }
        public long LastRunUtc { get; set; }
    }
}
