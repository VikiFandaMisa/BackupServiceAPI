using System;
using NCrontab;

namespace BackupServiceAPI.Helpers {
    public static class TemplatesHelper {
        public static DateTime[] GetSchedule(string cron) {
            var schedule = new DateTime[Convert.ToInt32(AppSettings.Configuration["Jobs:ScheduleLength"])];
            var crontab = CrontabSchedule.Parse(cron);
            DateTime from = DateTime.UtcNow;
            for (int i = 0; i < schedule.Length; i++)
            {
                schedule[i] = crontab.GetNextOccurrence(from);
                from = schedule[i];
            }
            return schedule;
        }
    }
}