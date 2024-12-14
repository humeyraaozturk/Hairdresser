using System.ComponentModel.DataAnnotations;

namespace Hairdresser.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentID { get; set; }
        public required int AppointmentServiceID { get; set; }
        public required string AppointmentEmployeeID { get; set; }
        public required string AppointmentUserID { get; set; } // Kullanıcı ID'si
        public DateTime AppointmentDate { get; set; }   //2024-12-15 14:30:00
        public string Status { get; set; } // "Onaylandı", "Beklemede", "İptal Edildi"
        public double TotalPrice { get; set; }      
        public Service Service { get; set; } // Appointment -> Service
        public Employee Employee { get; set; } // Appointment -> Employee
        public User User { get; set; } // Randevu -> Kullanıcı ilişkisi
    }
}
