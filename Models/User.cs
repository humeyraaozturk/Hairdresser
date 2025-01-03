﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hairdresser.Models
{
    public class User
    {
        [Key]
        [Required(ErrorMessage = "ID required")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "ID must be exactly 11 .")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "ID must be exactly 11 digits")]
        public string UserID { get; set; }

        [Required(ErrorMessage = "Full Name is required.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; }

        // Kullanıcının ilişkili rolü
        [Required(ErrorMessage = "Role selection is required.")]
        [ForeignKey("Role")]
        public int UserRoleID { get; set; }
        public Role Role { get; set; }

        // Kullanıcının aldığı randevular
        public ICollection<Appointment>? Appointments { get; set; } // Nullable, isteğe bağlı
    }
}
