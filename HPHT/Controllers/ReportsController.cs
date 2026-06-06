using HPHT.Data;
using HPHT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace HPHT.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Clients = await _context.Clients
                .Select(x => new SelectListItem
                {
                    Value = x.ClientCode.ToString(),
                    Text = x.Name
                })
                .ToListAsync();

            return View();
        }

        // ==========================
        // ISSUED STONES REPORT
        // ==========================
        public async Task<IActionResult> IssuedReport(
            int? ccode,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.Issues
                .Include(x => x.Client)
                .AsQueryable();

            if (ccode.HasValue)
            {
                query = query.Where(x => x.ClientCode == ccode);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.IssueDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.IssueDate <= toDate.Value);
            }

            var data = await query
                .OrderByDescending(x => x.IssueDate)
                .ToListAsync();

            return GenerateExcel(data, "Issued_Stones_Report");
        }

        // ==========================
        // PENDING RETURN REPORT
        // ==========================
        public async Task<IActionResult> PendingReturnReport(
            int? ccode,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.Issues
                .Include(x => x.Client)
                .Where(x => x.RETURNDATE == null)
                .AsQueryable();

            if (ccode.HasValue)
            {
                query = query.Where(x => x.ClientCode == ccode);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.IssueDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.IssueDate <= toDate.Value);
            }

            var data = await query
                .OrderByDescending(x => x.IssueDate)
                .ToListAsync();

            return GenerateExcel(data, "Pending_Return_Report");
        }

        // ==========================
        // RETURN REPORT
        // ==========================
        public async Task<IActionResult> ReturnReport(
            int? ccode,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.Issues
                .Include(x => x.Client)
                .Where(x => x.RETURNDATE != null)
                .AsQueryable();

            if (ccode.HasValue)
            {
                query = query.Where(x => x.ClientCode == ccode);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.RETURNDATE >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.RETURNDATE <= toDate.Value);
            }

            var data = await query
                .OrderByDescending(x => x.RETURNDATE)
                .ToListAsync();

            return GenerateExcel(data, "Returned_Stones_Report");
        }

        public async Task<IActionResult> RepeatedReport(
    int? ccode,
    DateTime? fromDate,
    DateTime? toDate)
        {
            var query = _context.Issues
                .Include(x => x.Client)
                .Where(x => x.IsRepeat)
                .AsQueryable();

            if (ccode.HasValue)
            {
                query = query.Where(x => x.ClientCode == ccode);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.RepeatDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.RepeatDate <= toDate.Value);
            }

            var data = await query
                .OrderByDescending(x => x.RepeatDate)
                .ToListAsync();

            return GenerateRepeatExcel(
                data,
                "Repeated_Stones_Report");
        }
        private FileResult GenerateRepeatExcel(
    List<Issues> data,
    string fileName)
        {
            ExcelPackage.LicenseContext =
                LicenseContext.NonCommercial;

            using var package = new ExcelPackage();

            var ws =
                package.Workbook.Worksheets
                    .Add("Repeat Report");

            ws.Cells[1, 1].Value = "KAID";
            ws.Cells[1, 2].Value = "Client";
            ws.Cells[1, 3].Value = "Issue Weight";
            ws.Cells[1, 4].Value = "Return Weight";
            ws.Cells[1, 5].Value = "Repeat Weight";
            ws.Cells[1, 6].Value = "Repeat Date";
            ws.Cells[1, 7].Value = "Repeat Count";

            int row = 2;

            foreach (var item in data)
            {
                ws.Cells[row, 1].Value = item.KAID;
                ws.Cells[row, 2].Value = item.Client?.Name;
                ws.Cells[row, 3].Value = item.IssueWeight;
                ws.Cells[row, 4].Value = item.RETURNWEIGHT;
                ws.Cells[row, 5].Value = item.RepeatWeight;
                ws.Cells[row, 6].Value = item.RepeatDate;
                ws.Cells[row, 6].Style.Numberformat.Format =
                    "dd-MM-yyyy";
                ws.Cells[row, 7].Value = item.RepeatCount;

                row++;
            }

            ws.Cells.AutoFitColumns();

            var stream = new MemoryStream();

            package.SaveAs(stream);

            stream.Position = 0;

            return File(
                stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{fileName}.xlsx");
        }
        // ==========================
        // EXCEL GENERATOR
        // ==========================
        private FileResult GenerateExcel(List<Issues> data, string fileName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();

            var ws = package.Workbook.Worksheets.Add("Report");

            // HEADER
            ws.Cells[1, 1].Value = "KAID";
            ws.Cells[1, 2].Value = "ClientId";
            ws.Cells[1, 3].Value = "Client Name";
            ws.Cells[1, 4].Value = "Lot No";
            ws.Cells[1, 5].Value = "Pkt No";
            ws.Cells[1, 6].Value = "PCS";
            ws.Cells[1, 7].Value = "Issue Weight";
            ws.Cells[1, 8].Value = "Issue Date";
            ws.Cells[1, 9].Value = "Return Date";
            ws.Cells[1, 10].Value = "Return Weight";
            ws.Cells[1, 11].Value = "Price";
            ws.Cells[1, 12].Value = "Remarks";

            int row = 2;

            foreach (var item in data)
            {
                ws.Cells[row, 1].Value = item.KAID;
                ws.Cells[row, 2].Value = item.ClientId;
                ws.Cells[row, 3].Value = item.Client?.Name;
                ws.Cells[row, 4].Value = item.LotNo;
                ws.Cells[row, 5].Value = item.PktNo;
                ws.Cells[row, 6].Value = item.PCS;
                ws.Cells[row, 7].Value = item.IssueWeight;
                ws.Cells[row, 8].Value = item.IssueDate;
                ws.Cells[row, 8].Style.Numberformat.Format = "dd-MM-yyyy";
                ws.Cells[row, 9].Value = item.RETURNDATE;
                ws.Cells[row, 9].Style.Numberformat.Format = "dd-MM-yyyy";
                ws.Cells[row, 10].Value = item.RETURNWEIGHT;
                ws.Cells[row, 11].Value = item.Price;
                ws.Cells[row, 12].Value = item.Remarks;

                row++;
            }

            ws.Cells.AutoFitColumns();

            var stream = new MemoryStream();

            package.SaveAs(stream);

            stream.Position = 0;

            return File(
                stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{fileName}.xlsx");
        }
    }
}
