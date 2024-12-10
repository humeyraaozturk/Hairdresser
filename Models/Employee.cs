using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hairdresser.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        public string FullName { get; set; }
        //public string Expertise { get; set; } // "Saç Kesimi, Saç Boyası, Saç Bakımı, Profesyonel Makyaj, Nail Art, Cilt Bakımı"
        public string AvailableHours { get; set; }
        public int ServiceId { get; set; } // Expertise için ServiceId'ye bağlanacak

        // Uzmanlık alanı
        [ForeignKey("ServiceId")]
        public Service Service { get; set; } // Navigation Property
    }
}
