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
    }
}
