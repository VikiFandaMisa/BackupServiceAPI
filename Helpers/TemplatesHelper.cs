using System;
using System.Collections.Generic;
using System.Linq;
using NCrontab;

namespace BackupServiceAPI.Helpers {
    public static class TemplatesHelper {
        public static List<DateTime> GetSchedule(string cron, DateTime start, DateTime end) {
            var schedule = new List<DateTime>();
            var scheduleLength = Convert.ToInt32(AppSettings.Configuration["Jobs:ScheduleLength"]);
            var crontab = CrontabSchedule.Parse(cron);

            if (DateTime.Now > start)
                start = crontab.GetNextOccurrences(start, DateTime.Now).Last();

            for(int i = 0; i < scheduleLength; i++) {
                var add = crontab.GetNextOccurrence(start);
                if (add < end)
                    schedule.Add(add);
                else 
                    break;
                start = add;
            }
            
            return schedule;
        }
    }
}