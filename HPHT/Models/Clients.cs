namespace HPHT.Models
{
    public class Clients
    {
        public int ClientCode { get; set; }

        public string Name { get; set; } = string.Empty;

        

        public ICollection<Issues>? Issues { get; set; }
    }
}
