namespace Hairdresser.Models
{
    public class Salon
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string WorkingHours { get; set; } // "09:00-18:00"
    }
}
