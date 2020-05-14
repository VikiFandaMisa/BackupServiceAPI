using System;
using System.Collections.Generic;
using System.Linq;
using NCrontab;

namespace BackupServiceAPI.Helpers {
    public static class TemplatesHelper {
        public static List<DateTime> GetSchedule(string cron, DateTime start, DateTime end) {
            var schedule = new List<DateTime>();
            var crontab = CrontabSchedule.Parse(cron);

            start = crontab.GetNextOccurrences(start, DateTime.Now).Last();

            schedule = crontab.GetNextOccurrences(start, end)
                .ToList()
                .GetRange(0, Convert.ToInt32(AppSettings.Configuration["Jobs:ScheduleLength"]));
            
            return schedule;
        }
    }
}