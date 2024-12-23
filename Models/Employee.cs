using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Hairdresser.Models
{
    public class Employee
    {
        [Key]
        public string EmployeeID { get; set; }
        public string FullName { get; set; }
        public string AvailableHours { get; set; } // Veritabanında JSON string olarak saklanabilir

        [NotMapped]
        public List<string> AvailableHoursList
        {
            get => string.IsNullOrEmpty(AvailableHours) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(AvailableHours);
            set => AvailableHours = JsonConvert.SerializeObject(value);
        }

        public int EmployeeServiceID { get; set; } // Expertise için ServiceId'ye bağlanacak
        
        public Service Service { get; set; } // Navigation Property       
        // Çalışanın aldığı randevular
        public ICollection<Appointment> Appointments { get; set; } // 1 Çalışan -> N Randevu
    }
}
