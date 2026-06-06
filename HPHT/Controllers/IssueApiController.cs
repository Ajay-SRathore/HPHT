using HPHT.Data;
using HPHT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Text.Json;

namespace HPHT.Controllers
{
    [Authorize(Roles = "Admin,User")]
    [Route("api/issues")]
    [ApiController]
    public class IssuesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public IssuesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("clients")]
        public async Task<IActionResult> GetClients()
         {
            var clients = await _context.Clients
                    .Select(c => new
                    {
                        clientCode = c.ClientCode,
                        name = c.Name
                     
                    }).ToListAsync();
            return Ok(clients);
        }

        [HttpPost("upload-excel")]
        public async Task<IActionResult> UploadExcel(IFormFile file, int ccode)
        {
            // your upload logic
            return Ok();
        }
        [HttpPost("upload-preview")]
        public async Task<IActionResult> UploadPreview(IFormFile file)
        {
            var list = new List<object>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            for (int i = 2; i <= sheet.Dimension.Rows; i++)
            {
                list.Add(new
                {
                    srNo = sheet.Cells[i, 1].Text,
                    ClientName= sheet.Cells[i, 2 ].Text,
                    ClientCode = sheet.Cells[i, 3].Text,
                    KAID = sheet.Cells[i, 4].Text,
                    ClientId= sheet.Cells[i, 5].Text,
                    pcs= sheet.Cells[i, 6].Text,
                    issueWeight = sheet.Cells[i, 7].Text,                    
                    shape = sheet.Cells[i, 8].Text,
                    issueDate = sheet.Cells[i, 9].Text,
                    roughType = sheet.Cells[i, 10].Text,
                    returnDate = sheet.Cells[i, 11].Text,                    
                    returnWeight=  sheet.Cells[i, 12].Text,
                    remarks = sheet.Cells[i, 13].Text,

                });
            }

            return Ok(list);  // ✅ only preview
        }

       


        [HttpPost("save-issues")]
        public async Task<IActionResult> SaveIssues(
    [FromForm] string issues,
    [FromForm] List<IFormFile> files)
        {
            var issueList =
                JsonSerializer.Deserialize<List<Issues>>(
                    issues,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            string uploadPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/uploads/issues");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);


            // ============================================
            // SUCCESS / FAILED LIST
            // ============================================

            List<Issues> successList =
                new List<Issues>();

            List<dynamic> failedData =
                new List<dynamic>();


            // ============================================
            // VALIDATION + PROCESSING
            // ============================================

            foreach (var item in issueList)
            {
                try
                {
                    // CHECK DUPLICATE CLIENTID
                    bool exists = await _context.Issues
                        .AnyAsync(x => x.ClientId == item.ClientId);

                    if (exists)
                    {
                        failedData.Add(new
                        {
                            ClientId = item.ClientId,
                            Error = "ClientId already exists"
                        });

                        continue;
                    }

                
                    item.IssueDate = DateTime.Now;
                    item.IssuedBy = User.Identity?.Name;

                    item.CreatedDate = DateTime.Now;
                    item.CreatedBy = User.Identity?.Name;

                    item.IsReturned = false;

                   // item.ClientCode = ccode;


                    // IMAGE UPLOAD
                    if (files != null &&
                        files.Count > 0 &&
                        files[0] != null)
                    {
                        var file = files[0];

                        string fileName =
                            Guid.NewGuid() +
                            Path.GetExtension(file.FileName);

                        string filePath =
                            Path.Combine(uploadPath, fileName);

                        using (var stream =
                            new FileStream(
                                filePath,
                                FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        item.ImagePath =
                            "/uploads/issues/" + fileName;
                    }

                    successList.Add(item);
                }
                catch (Exception ex)
                {
                    failedData.Add(new
                    {
                        ClientId = item.ClientId,
                        Error = ex.Message
                    });
                }
            }


            // ============================================
            // SAVE SUCCESS RECORDS
            // ============================================

            await _context.Issues
                .AddRangeAsync(successList);

            await _context.SaveChangesAsync();


            // ============================================
            // GENERATE KAID
            // ============================================

            foreach (var item in successList)
            {
                item.KAID =
                    $"{item.ClientCode}KHT-{item.Id:D3}";
            }

            await _context.SaveChangesAsync();


            // ============================================
            // GENERATE EXCEL
            // ============================================
            
            // ============================================
            // NO SUCCESS RECORDS
            // ============================================

            if (successList.Count == 0)
            {
                Response.Headers.Add(
                    "failed-records",
                    JsonSerializer.Serialize(failedData));

                return BadRequest(
                    "No valid records found");
            }

            ExcelPackage.LicenseContext =
                LicenseContext.NonCommercial;

            using var package =
                new ExcelPackage();

            var ws =
                package.Workbook
                .Worksheets.Add("Issues");


            // HEADER
            ws.Cells[1, 1].Value = "SRNO";
            ws.Cells[1, 2].Value = "CLIENTNAME";
            ws.Cells[1, 3].Value = "CLIENT CODE";
            ws.Cells[1, 4].Value = "KA ID";
            ws.Cells[1, 5].Value = "CLIENTID";
            ws.Cells[1, 6].Value = "PCS";
            ws.Cells[1, 7].Value = "ISSUE WEIGHT";
            ws.Cells[1, 8].Value = "SHAPE";
            ws.Cells[1, 9].Value = "ISSUE DATE";
            ws.Cells[1, 10].Value = "ROUGH TYPE";
            ws.Cells[1, 11].Value = "RETURN DATE";
            ws.Cells[1, 12].Value = "RETURN WEIGHT";
            ws.Cells[1, 13].Value = "REMARKS";


            // HEADER STYLE
            using (var range =
                ws.Cells[1, 1, 1, 13])
            {
                range.Style.Font.Bold = true;
            }


            int row = 2;

            foreach (var item in successList)
            {
                var client =
                    await _context.Clients
                    .FirstOrDefaultAsync(
                        x => x.ClientCode ==
                             item.ClientCode);

                ws.Cells[row, 1].Value =
                    item.SrNo;

                ws.Cells[row, 2].Value =
                    client?.Name;

                ws.Cells[row, 3].Value =
                    item.ClientCode;

                ws.Cells[row, 4].Value =
                    item.KAID;

                ws.Cells[row, 5].Value =
                    item.ClientId;

                ws.Cells[row, 6].Value =
                    item.PCS;

                ws.Cells[row, 7].Value =
                    item.IssueWeight;

                ws.Cells[row, 8].Value =
                    item.Shape;

                ws.Cells[row, 9].Value =
                    item.IssueDate;

                ws.Cells[row, 9]
                    .Style.Numberformat.Format =
                    "dd-MM-yyyy";
                ws.Cells[row, 10].Value =
                    item.ROUGHTYPE;

                

                //ws.Cells[row, 11].Value =
                //    item.RETURNDATE;

                //ws.Cells[row, 11]
                //    .Style.Numberformat.Format =
                //    "dd-MM-yyyy";

                //ws.Cells[row, 12].Value =
                //    item.RETURNWEIGHT;

                ws.Cells[row, 13].Value =
                    item.Remarks;

                row++;
            }

            ws.Cells.AutoFitColumns();


            // ============================================
            // SAVE STREAM
            // ============================================

            var successStream =
                new MemoryStream();

            package.SaveAs(successStream);

            successStream.Position = 0;


            // ============================================
            // SEND FAILED RECORDS IN HEADER
            // ============================================

            Response.Headers.Add(
                "failed-records",
                JsonSerializer.Serialize(failedData));


            // ============================================
            // DOWNLOAD EXCEL
            // ============================================

            return File(
                successStream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"HPHT_Issue_{DateTime.Now:ddMMMyyyy}.xlsx");
        }

        [HttpGet("repeat-search")]
        public async Task<IActionResult> RepeatSearch(
    int? clientCode,
    string? kaid)
        {
            var query = _context.Issues.AsQueryable();

            if (clientCode.HasValue)
                query = query.Where(x => x.ClientCode == clientCode);

            if (!string.IsNullOrWhiteSpace(kaid))
                query = query.Where(x => x.KAID.Contains(kaid));

            var result = await query
                .Select(x => new
                {
                    x.Id,
                    x.KAID,
                    x.CLIENTNAME,
                    x.Shape,          // ADD
                    x.IssueWeight,    // ADD
                    x.RETURNWEIGHT,
                    x.IsReturned,
                    x.RepeatWeight,
                    x.IsRepeat,
                    x.RepeatCount
                })
                .ToListAsync();

            return Ok(result);
        }
        [HttpPost("save-repeat")]
        public async Task<IActionResult> SaveRepeat(
     [FromBody]
    List<RepeatVM> model)
        {
            int updated = 0;

            List<string> failed =
                new();

            foreach (var row in model)
            {
                var issue = await _context.Issues
                    .FirstOrDefaultAsync(x => x.Id == row.Id);

                if (issue == null)
                {
                    failed.Add($"{row.KAID} - Stone not found");
                    continue;
                }

                if (!issue.IsReturned)
                {
                    failed.Add($"{row.KAID} - Stone not yet returned");
                    continue;
                }

                if (!row.IsRepeat)
                    continue;

                issue.IsRepeat = true;

                issue.RepeatCount++;

                issue.RepeatWeight = row.RepeatWeight;

                issue.RepeatDate = DateTime.Now;
                issue.RepeatBy = User.Identity?.Name;

                updated++;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                updatedCount = updated,
                failed
            });
        }
        [HttpPost("save-returns")]
        public async Task<IActionResult> SaveReturns([FromForm] string returns)
        {
            try
            {
                var returnList =
                    System.Text.Json.JsonSerializer.Deserialize<List<Issues>>(
                        returns,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (returnList == null || !returnList.Any())
                {
                    return BadRequest("No return data found");
                }

                int updatedCount = 0;

                List<string> failedList =
                    new List<string>();


                // ============================================
                // SUCCESS RECORDS FOR EXCEL
                // ============================================

                List<Issues> updatedRecords =
                    new List<Issues>();


                foreach (var item in returnList)
                {
                    if (string.IsNullOrWhiteSpace(item.KAID))
                    {
                        failedList.Add("Empty KAID");
                        continue;
                    }

                    var existingIssue =
                        await _context.Issues
                        .FirstOrDefaultAsync(
                            x => x.KAID == item.KAID);

                    // ============================================
                    // KAID NOT FOUND
                    // ============================================

                    if (existingIssue == null)
                    {
                        failedList.Add(item.KAID);
                        continue;
                    }

                    // ============================================
                    // UPDATE RETURN DATA
                    // ============================================

                    existingIssue.RETURNDATE =
                        DateTime.Now;

                    existingIssue.RETURNWEIGHT =
                        item.RETURNWEIGHT;

                    existingIssue.Remarks =
                        item.Remarks;
                    existingIssue.ReturnedBy = User.Identity?.Name;
                    existingIssue.ReturnedOn = DateTime.Now;
                    existingIssue.IsReturned = true;

                    updatedRecords.Add(existingIssue);

                    updatedCount++;
                }

                await _context.SaveChangesAsync();


                // ============================================
                // IF NO VALID RECORDS
                // ============================================

                if (updatedRecords.Count == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "KAID does not exist in Issue table",
                        failedList = failedList
                    });
                }


                // ============================================
                // GENERATE EXCEL
                // ============================================

                ExcelPackage.LicenseContext =
                    LicenseContext.NonCommercial;

                using var package =
                    new ExcelPackage();

                var ws =
                    package.Workbook
                    .Worksheets.Add("Returns");


                // HEADER
                ws.Cells[1, 1].Value = "SRNO";
                ws.Cells[1, 2].Value = "CLIENT CODE";
                ws.Cells[1, 3].Value = "KAID";
                ws.Cells[1, 4].Value = "CLIENTID";
                ws.Cells[1, 5].Value = "RETURN DATE";
                ws.Cells[1, 6].Value = "RETURN WEIGHT";
                ws.Cells[1, 7].Value = "REMARKS";


                using (var range =
                    ws.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                }

                int row = 2;

                foreach (var item in updatedRecords)
                {
                    ws.Cells[row, 1].Value =
                        item.SrNo;

                    ws.Cells[row, 2].Value =
                        item.ClientCode;

                    ws.Cells[row, 3].Value =
                        item.KAID;

                    ws.Cells[row, 4].Value =
                        item.ClientId;

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


                // ============================================
                // FAILED KAID HEADER
                // ============================================

                Response.Headers.Add(
                    "Failed-Records",
                    JsonSerializer.Serialize(failedList));


                // ============================================
                // RETURN EXCEL FILE
                // ============================================

                using var stream =
                    new MemoryStream();

                package.SaveAs(stream);

                stream.Position = 0;

                return File(
               stream.ToArray(),
               "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
               $"HPHT_Issue_Return_{DateTime.Now:ddMMMyyyy}.xlsx");
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }
    }

}


