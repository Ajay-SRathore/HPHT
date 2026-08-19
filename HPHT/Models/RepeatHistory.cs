using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HPHT.Models
{
    public class RepeatHistory
    {
        [Key]
        public int Id { get; set; }

        public int IssueId { get; set; }


        [ForeignKey(nameof(IssueId))]
        public Issues? Issue { get; set; }

        public string? KAID { get; set; }

        public int? ClientCode { get; set; }

        public string? ClientId { get; set; }

        public int RepeatNo { get; set; }

        public DateTime RepeatIssueDate { get; set; }

        public decimal RepeatIssueWeight { get; set; }

        public DateTime? RepeatReturnDate { get; set; }

        public decimal? RepeatReturnWeight { get; set; }

        public bool IsReturned { get; set; }

        public string? Remarks { get; set; }

        public DateTime CreatedDate { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string? ModifiedBy { get; set; }


    }
}