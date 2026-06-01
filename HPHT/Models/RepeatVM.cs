namespace HPHT.Models
{
    public class RepeatVM
    {
        public int Id { get; set; }

        public string? KAID { get; set; }

        public bool IsRepeat { get; set; }

        public decimal? RepeatWeight { get; set; }
    }
}