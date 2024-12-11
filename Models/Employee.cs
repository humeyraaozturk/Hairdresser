using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hairdresser.Models
{
    public class Employee
    {
        [Key]
        public string EmployeeID { get; set; }
        public string FullName { get; set; }
        public string AvailableHours { get; set; }
        public int EmployeeServiceID { get; set; } // Expertise için ServiceId'ye bağlanacak
        
        public Service Service { get; set; } // Navigation Property       
        // Çalışanın aldığı randevular
        public ICollection<Appointment> Appointments { get; set; } // 1 Çalışan -> N Randevu
    }
}
