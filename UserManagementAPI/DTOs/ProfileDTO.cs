namespace UserManagementAPI.DTOs
{
    public class ProfileDTO
    {
        public Guid Id { get; set; }
        public string Email { get; set; }

        public string Name { get; set; }

        public List<string> Roles { get; set; }
    }
}
