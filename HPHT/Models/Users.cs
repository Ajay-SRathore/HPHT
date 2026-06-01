namespace HPHT.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public int UserType { get; set; } = 1; // Always ADMIN
    }
}
