using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace BackupServiceAPI.Models {

    [NotMapped]
    public class Time {
        public int Hours { get; set; }
        public int Minutes { get; set; }
    }

    [NotMapped]
    public class Period {
        public bool PeriodMode { get; set; }
        public int Unit { get; set; }
        public int Value { get; set; }
        public int[] Days { get; set; }
        public Time Time { get; set; }
        public static Period FromJson(string s) {
            return JsonSerializer.Deserialize<Period>(s);
        }

        public string ToJson() {
            return JsonSerializer.Serialize(this, typeof(Period));
        }

        public string GetCron() {
            if (PeriodMode)
                return PeriodCron();
            else
                return WeekCron();
        }

        private string PeriodCron() {
            var cron = "";
            if (Unit == 1) {
                cron = "*/" + Value + " * * * *";
            }
            else if (Unit == 2) {
                cron = "0 */" + Value + " * * *";
            }
            else if (Unit == 3) {
                cron = "0 0 " + "*/" + Value + " * *";
            }
            else if (Unit == 4) {
                cron = "0 0 1 " + "*/" + Value + " *";
            }
            return cron;
        }

        private string WeekCron() {
            string cron;
            cron = Time.Minutes + " " + Time.Hours + " * * ";
            foreach (var day in Days) {
                cron += day + ",";
            }
            cron = cron[0..^1];
            return cron;
        }
    }
}
