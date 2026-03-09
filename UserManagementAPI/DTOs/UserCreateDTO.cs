using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.DTOs
{
    public class UserCreateDTO
    {
        [Required(ErrorMessage = "Id is required.")]
        public int Id { get; set; }
        //[Required(ErrorMessage = "Order must contain at least one item.")]
        //[MinLength(1, ErrorMessage = "Order must have at least one item.")]
        //public List<OrderItemDTO> Items { get; set; } = new();
    }
}
