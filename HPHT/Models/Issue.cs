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

        public decimal? RepeatWeight { get; set; }

        public DateTime? RepeatDate { get; set; }

        public string? RepeatBy { get; set; }
    }

}

