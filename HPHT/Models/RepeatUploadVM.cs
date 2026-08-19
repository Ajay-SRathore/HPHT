namespace HPHT.Models
{
    public class RepeatUploadVM
    {
        public string? KAID { get; set; }

        public DateTime? RepeatIssueDate { get; set; }

        public decimal? RepeatIssueWeight { get; set; }

        public DateTime? RepeatReturnDate { get; set; }

        public decimal? RepeatReturnWeight { get; set; }

        public string? Remarks { get; set; }

        // Preview only
        public string? Action { get; set; }

        public string? Status { get; set; }
    }
}