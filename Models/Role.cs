namespace Hairdresser.Models
{
    public class Role
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }

        // Rolün ilişkili olduğu kullanıcılar
        public ICollection<User> Users { get; set; }
    }
}
