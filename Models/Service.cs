namespace Hairdresser.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; } // "Saç Kesimi, Saç Boyası, Saç Bakımı, Fön, Maşa, Ombre, Makyaj, Nail Art"
        public double Price { get; set; } // Ücret
        public int Duration { get; set; } // Süre (dakika)
    }
}
