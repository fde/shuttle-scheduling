using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Shuttle.Scheduling.FileDataAccess
{
    public class ScheduleRepository : IScheduleRepository
    {
        #region IScheduleRepository Members

        private static ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        public IEnumerable<Schedule> All()
        {
            locker.EnterReadLock();
            var result = UnsafeReadSchedule();
            locker.ExitReadLock();
            return result.Select(r => new Schedule(r.Id, r.QueueConnectionString, new Core.Infrastructure.CronExpression(r.CronString), DateTime.FromFileTimeUtc(r.LastRunUtc))).ToList(); ;
        }

        private static List<ScheduleStorageItem> UnsafeReadSchedule()
        {
            var content = File.Exists("schedule.json") ? File.ReadAllText("schedule.json") : "";
            var result = JsonConvert.DeserializeObject<List<ScheduleStorageItem>>(content) ?? new List<ScheduleStorageItem>();

            return result;
        }

        public void SaveNextNotification(Schedule schedule)
        {
            locker.EnterUpgradeableReadLock();
            var items = UnsafeReadSchedule();
            locker.EnterWriteLock();

            var currentItem = items.FirstOrDefault(i => i.Id == schedule.Id);
            if (currentItem != null) items.Remove(currentItem);
            items.Add(new ScheduleStorageItem()
            {
                Id = schedule.Id,
                CronString = schedule.CronExpression.Expression,
                LastRunUtc = schedule.NextNotification.ToFileTimeUtc(),
                QueueConnectionString = schedule.InboxWorkQueueUri
            });

            File.WriteAllText("schedule.json", JsonConvert.SerializeObject(items));
            locker.ExitWriteLock();
            locker.ExitUpgradeableReadLock();
        }

        #endregion
    }
}
