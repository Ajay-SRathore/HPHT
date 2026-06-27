using HPHT.Data;
using HPHT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.IO.Compression;

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
            return GenerateZip(
    data,
    "Issued_Stones_Report");
            //  return GenerateExcel(data, "Issued_Stones_Report");
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
                .OrderBy(x => x.IssueDate)
                .ToListAsync();

            return GenerateZip(data, "Pending_Return_Report");
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
                .OrderBy(x => x.RETURNDATE)
                .ToListAsync();

            return GenerateZip(data, "Returned_Stones_Report");
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
                .OrderBy(x => x.RepeatDate)
                .ToListAsync();

            return GenerateRepeatExcel(
                data,
                "Repeated_Stones_Report");
        }

        private FileResult GenerateZip(
    List<Issues> data,
    string reportName)
        {
            ExcelPackage.LicenseContext =
                LicenseContext.NonCommercial;

            using var zipStream =
                new MemoryStream();

            using (var archive =
                new ZipArchive(
                    zipStream,
                    ZipArchiveMode.Create,
                    true))
            {
                var clientGroups =
                    data.GroupBy(x =>
                        x.Client?.Name ?? "Unknown");

                foreach (var group in clientGroups)
                {
                    using var package =
                        new ExcelPackage();

                    var ws =
                        package.Workbook.Worksheets
                            .Add("Report");
                    ws.Cells["A1"].Value = "Client Name";
                    ws.Cells["B1"].Value = group.Key;

                    ws.Cells["A2"].Value = "Number Of Stones";
                    ws.Cells["B2"].Value = group.Count();

                    ws.Cells["A3"].Value = "Total Issue Weight";
                    ws.Cells["B3"].Value =
                        group.Sum(x => x.IssueWeight ?? 0);

                    ws.Cells["A1:B3"].Style.Font.Bold = true;

                    // HEADER

                    ws.Cells["A1"].Value = "Client Name";
                    ws.Cells["B1"].Value = group.Key;

                    ws.Cells["A2"].Value = "Number Of Stones";
                    ws.Cells["B2"].Value = group.Count();

                    ws.Cells["A3"].Value = "Total Issue Weight";
                    ws.Cells["B3"].Value =
                        group.Sum(x => x.IssueWeight ?? 0);

                    ws.Cells["A1:B3"].Style.Font.Bold = true;

                    // TABLE HEADER

                    int headerRow = 5;


                    ws.Cells[headerRow, 1].Value = "ClientId";
                    ws.Cells[headerRow, 2].Value = "Client Name";
                    ws.Cells[headerRow, 3].Value = "Issue Weight";
                    ws.Cells[headerRow, 4].Value = "Issue Date";
                    ws.Cells[headerRow, 5].Value = "Return Date";
                    ws.Cells[headerRow, 6].Value = "Return Weight";
                    ws.Cells[headerRow, 7].Value = "Remarks";

                    using (var range =
                        ws.Cells[headerRow, 1, headerRow, 8])
                    {
                        range.Style.Font.Bold = true;
                    }

                    int row = 6;

                    foreach (var item in group)
                    {
                        ws.Cells[row, 1].Value = item.ClientId;

                        ws.Cells[row, 2].Value = item.Client?.Name;
                        ws.Cells[row, 3].Value =
    item.IssueWeight;

                        ws.Cells[row, 4].Value =
                            item.IssueDate;

                        ws.Cells[row, 4]
                            .Style.Numberformat.Format =
                            "dd-MM-yyyy";

                        ws.Cells[row, 5].Value =
                            item.RETURNDATE;

                        ws.Cells[row, 5]
                            .Style.Numberformat.Format =
                            "dd-MM-yyyy";

                        ws.Cells[row, 6].Value =
                            item.RETURNWEIGHT;

                        ws.Cells[row, 7].Value =
                            item.Remarks;

                        row++;
                    }

                    ws.Cells.AutoFitColumns();

                    var excelBytes =
                        package.GetAsByteArray();

                    string safeFileName =
                        string.Join("_",
                            group.Key.Split(
                                Path.GetInvalidFileNameChars()));

                    var entry =
                        archive.CreateEntry(
                            $"{safeFileName}.xlsx");

                    using var entryStream =
                        entry.Open();

                    entryStream.Write(
                        excelBytes,
                        0,
                        excelBytes.Length);
                }
            }

            zipStream.Position = 0;

            return File(
                zipStream.ToArray(),
                "application/zip",
                $"{reportName}.zip");
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

    }
}
