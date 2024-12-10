using System.ComponentModel.DataAnnotations;

namespace Hairdresser.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } // "Saç Kesimi, Saç Boyası, Saç Bakımı, Profesyonel Makyaj, Nail Art, Cilt Bakımı"
        public double Price { get; set; } // Ücret
        public int Duration { get; set; } // Süre (dakika)

        // Bu hizmetle ilişkili çalışanlar
        public ICollection<Employee> Employees { get; set; }
    }
}
