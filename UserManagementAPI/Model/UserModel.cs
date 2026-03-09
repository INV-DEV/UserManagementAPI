using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.Model
{
    public class UserModel
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
    }
}
