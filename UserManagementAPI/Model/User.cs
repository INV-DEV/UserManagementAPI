using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Xml;
using System.Xml.Linq;

namespace UserManagementAPI.Model
{
    //[Index(nameof(Email), Name = "IX_Unique_Email", IsUnique = true)]
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        
        [Required]
        public required DateTime DateOfBirth { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }

        //[Required]
        //[StringLength(100)]
        //public string Password { get; set; }
        //public ICollection<UserRole> UserRoles { get; set; } // Navigation property for many-to-many relationship with Role
    }
}
