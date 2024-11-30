namespace Hairdresser.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Expertise { get; set; } // "Saç Kesimi, Saç Boyası, Saç Bakımı, Fön, Maşa, Ombre, Makyaj, Nail Art"
        public string AvailableHours { get; set; } 
    }
}
