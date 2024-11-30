namespace Hairdresser.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } // "Onaylandı", "Beklemede", "İptal Edildi"
        public double TotalPrice { get; set; }
    }
}
