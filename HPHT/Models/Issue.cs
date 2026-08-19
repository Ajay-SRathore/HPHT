using System.ComponentModel.DataAnnotations.Schema;

namespace HPHT.Models
{

    public class Issues
    {
        public int Id { get; set; }
        public int? SrNo { get; set; }
        public string? ImagePath { get; set; }
        public string? LotNo { get; set; }
        public string? PktNo { get; set; }
        public int? PCS { get; set; }
        public decimal? IssueWeight { get; set; }
        public string? Shape { get; set; }
        public string? Exp { get; set; }
        public decimal? Price { get; set; }
        public string? Remarks { get; set; }

        public DateTime? IssueDate { get; set; }
        public string? IssuedBy { get; set; }

        public int? ClientCode { get; set; }


        public Clients? Client { get; set; }

        public string? CLIENTNAME { get; set; }
        public string? KAID { get; set; }
        public string? ROUGHTYPE { get; set; }

        public DateTime? RETURNDATE { get; set; }
  
        public decimal? RETURNWEIGHT { get; set; }
        public int RepeatCount { get; set; }
        public string? ClientId { get; set; }
        public string? Recheck { get; set; }
        public bool IsReturned { get; set; }

        public bool IsRepeat { get; set; }

        

      
        public DateTime? RepeatReturnDate { get; set; }

        
        // Issue Audit
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }

        // Update Audit
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }

        // Return Audit
        public string? ReturnedBy { get; set; }
        public DateTime? ReturnedOn { get; set; }
        public ICollection<RepeatHistory> RepeatHistories
        {
            get;
            set;
        }
=
new List<RepeatHistory>();

    }

}

