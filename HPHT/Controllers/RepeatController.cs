using HPHT.Data;
using HPHT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace HPHT.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class RepeatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RepeatController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        [Route("api/repeat/upload-preview")]
        public async Task<IActionResult> UploadPreview(IFormFile file)
        {
            ExcelPackage.LicenseContext =
                LicenseContext.NonCommercial;

            using var stream = new MemoryStream();

            await file.CopyToAsync(stream);

            using var package = new ExcelPackage(stream);

            var ws = package.Workbook.Worksheets[0];

            List<RepeatUploadVM> preview = new();

            for (int row = 2; row <= ws.Dimension.Rows; row++)
            {
                var kaid = ws.Cells[row, 1].Text.Trim();

                if (string.IsNullOrWhiteSpace(kaid))
                    continue;

                var item = new RepeatUploadVM
                {
                    KAID = kaid,

                    RepeatIssueDate =
                        DateTime.TryParse(ws.Cells[row, 2].Text, out var issueDate)
                            ? issueDate
                            : null,

                    RepeatIssueWeight =
                        decimal.TryParse(ws.Cells[row, 3].Text, out var issueWeight)
                            ? issueWeight
                            : null,

                    RepeatReturnDate =
                        DateTime.TryParse(ws.Cells[row, 4].Text, out var returnDate)
                            ? returnDate
                            : null,

                    RepeatReturnWeight =
                        decimal.TryParse(ws.Cells[row, 5].Text, out var returnWeight)
                            ? returnWeight
                            : null,

                    Remarks = ws.Cells[row, 6].Text
                };

                var issue = await _context.Issues
                    .FirstOrDefaultAsync(x => x.KAID == item.KAID);

                if (issue == null)
                {
                    item.Action = "Invalid";
                    item.Status = "KAID not found";
                    preview.Add(item);
                    continue;
                }

                if (!issue.IsReturned)
                {
                    item.Action = "Invalid";
                    item.Status = "Original Issue not returned";
                    preview.Add(item);
                    continue;
                }

                if (!item.RepeatIssueDate.HasValue)
                {
                    item.Action = "Invalid";
                    item.Status = "Repeat Issue Date required";
                    preview.Add(item);
                    continue;
                }

                if (!item.RepeatIssueWeight.HasValue)
                {
                    item.Action = "Invalid";
                    item.Status = "Repeat Issue Weight required";
                    preview.Add(item);
                    continue;
                }
                if (!item.RepeatReturnDate.HasValue &&
    item.RepeatReturnWeight.HasValue)
                {
                    item.Action = "Invalid";
                    item.Status = "Repeat Return Date required";
                    preview.Add(item);
                    continue;
                }

                if (item.RepeatReturnDate.HasValue &&
                    !item.RepeatReturnWeight.HasValue)
                {
                    item.Action = "Invalid";
                    item.Status = "Repeat Return Weight required";
                    preview.Add(item);
                    continue;
                }

                if (item.RepeatReturnDate.HasValue &&
                    item.RepeatReturnDate < item.RepeatIssueDate)
                {
                    item.Action = "Invalid";
                    item.Status = "Return Date cannot be before Issue Date";
                    preview.Add(item);
                    continue;
                }

                var existingPendingRepeat =
    await _context.RepeatHistories
        .FirstOrDefaultAsync(x =>
            x.IssueId == issue.Id &&
            x.RepeatIssueDate == item.RepeatIssueDate &&
            !x.IsReturned);

                if (existingPendingRepeat != null)
                {
                    if (item.RepeatReturnDate.HasValue)
                    {
                        item.Action = "Update Repeat Return";
                        item.Status = "Ready";

                        preview.Add(item);
                        continue;
                    }

                    item.Action = "Invalid";
                    item.Status = "Pending Repeat already exists";

                    preview.Add(item);
                    continue;
                }
                bool duplicateRepeat =
    await _context.RepeatHistories.AnyAsync(x =>
        x.IssueId == issue.Id &&
        x.RepeatIssueDate == item.RepeatIssueDate);

                if (duplicateRepeat)
                {
                    item.Action = "Invalid";
                    item.Status = "Repeat already exists for this Issue Date";
                    preview.Add(item);
                    continue;
                }

                if (item.RepeatReturnDate.HasValue &&
                    !item.RepeatReturnWeight.HasValue)
                {
                    item.Action = "Invalid";
                    item.Status = "Repeat Return Weight required";
                    preview.Add(item);
                    continue;
                }

                item.Action =
    item.RepeatReturnDate.HasValue
        ? "Repeat Issue & Return"
        : "Repeat Issue";

                item.Status = "Ready";

                preview.Add(item);
            }

            return Ok(preview);
        }
        [HttpPost]
        [Route("api/repeat/save")]
        public async Task<IActionResult> Save([FromBody] List<RepeatUploadVM> rows)
        {
            int saved = 0;

            List<string> failed = new();

            using var transaction =
    await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var row in rows)
                {
                    try
                {
                    var issue = await _context.Issues
                        .FirstOrDefaultAsync(x => x.KAID == row.KAID);

                    if (issue == null)
                    {
                        failed.Add($"{row.KAID} - KAID not found");
                        continue;
                    }

                    if (!issue.IsReturned)
                    {
                        failed.Add($"{row.KAID} - Original Issue not returned");
                        continue;
                    }

                    if (!row.RepeatIssueDate.HasValue)
                    {
                        failed.Add($"{row.KAID} - Repeat Issue Date required");
                        continue;
                    }

                    if (!row.RepeatIssueWeight.HasValue)
                    {
                        failed.Add($"{row.KAID} - Repeat Issue Weight required");
                        continue;
                    }
                    if (row.RepeatIssueWeight <= 0)
                    {
                        failed.Add($"{row.KAID} - Invalid Repeat Issue Weight");
                        continue;
                    }

                    if (!row.RepeatReturnDate.HasValue &&
                        row.RepeatReturnWeight.HasValue)
                    {
                        failed.Add($"{row.KAID} - Repeat Return Date required");
                        continue;
                    }

                    if (row.RepeatReturnDate.HasValue &&
                        !row.RepeatReturnWeight.HasValue)
                    {
                        failed.Add($"{row.KAID} - Repeat Return Weight required");
                        continue;
                    }

                    if (row.RepeatReturnDate.HasValue &&
                        row.RepeatReturnWeight <= 0)
                    {
                        failed.Add($"{row.KAID} - Invalid Repeat Return Weight");
                        continue;
                    }

                    if (row.RepeatReturnDate.HasValue &&
                        row.RepeatReturnDate < row.RepeatIssueDate)
                    {
                        failed.Add($"{row.KAID} - Return Date cannot be before Issue Date");
                        continue;
                    }


                        // Check whether this upload is for an existing pending repeat
                        var existingPendingRepeat =
                            await _context.RepeatHistories
                                .FirstOrDefaultAsync(x =>
                                    x.IssueId == issue.Id &&
                                    x.RepeatIssueDate == row.RepeatIssueDate &&
                                    !x.IsReturned);

                        // If this is only a Repeat Issue upload,
                        // do not allow another pending repeat.
                        if (!row.RepeatReturnDate.HasValue &&
                            existingPendingRepeat != null)
                        {
                            failed.Add($"{row.KAID} - Pending Repeat already exists");
                            continue;
                        }
                        // Repeat Return upload
                        // Update existing pending repeat
                        if (row.RepeatReturnDate.HasValue &&
                            existingPendingRepeat != null)
                        {
                            existingPendingRepeat.RepeatReturnDate =
                                row.RepeatReturnDate;

                            existingPendingRepeat.RepeatReturnWeight =
                                row.RepeatReturnWeight;

                            existingPendingRepeat.IsReturned = true;

                            existingPendingRepeat.Remarks = "Issue & Return";

                            existingPendingRepeat.ModifiedBy =
                                User.Identity?.Name;

                            existingPendingRepeat.ModifiedDate =
                                DateTime.Now;

                            saved++;

                            continue;
                        }
                        bool duplicateRepeat =
        await _context.RepeatHistories.AnyAsync(x =>
            x.IssueId == issue.Id &&
            x.RepeatIssueDate == row.RepeatIssueDate);

                        if (duplicateRepeat)
                        {
                            failed.Add($"{row.KAID} - Repeat already exists for this Issue Date");
                            continue;
                        }

                        if (row.RepeatReturnDate.HasValue &&
                        !row.RepeatReturnWeight.HasValue)
                    {
                        failed.Add($"{row.KAID} - Repeat Return Weight required");
                        continue;
                    }

                        var repeat = new RepeatHistory
                        {
                            IssueId = issue.Id,

                            KAID = issue.KAID,
                            ClientId = issue.ClientId,
                            ClientCode = issue.ClientCode,
                            RepeatReturnDate= row.RepeatReturnDate,
                            RepeatReturnWeight=row.RepeatReturnWeight,
                            ModifiedBy = User.Identity?.Name,
                            ModifiedDate = DateTime.Now,
                            RepeatNo =
            (await _context.RepeatHistories
                .Where(x => x.IssueId == issue.Id)
                .MaxAsync(x => (int?)x.RepeatNo) ?? 0) + 1,

                            RepeatIssueDate = row.RepeatIssueDate.Value,
                            RepeatIssueWeight = row.RepeatIssueWeight.Value,

                            Remarks = row.RepeatReturnDate.HasValue
            ? "Issue & Return"
            : "Issue but not returned",

                            CreatedBy = User.Identity?.Name,
                            CreatedDate = DateTime.Now
                        };

                        _context.RepeatHistories.Add(repeat);

                    issue.IsRepeat = true;

                        issue.RepeatCount =
    await _context.RepeatHistories
    .CountAsync(x => x.IssueId == issue.Id);

                        issue.ModifiedBy =
                        User.Identity?.Name;

                    issue.ModifiedDate =
                        DateTime.Now;

                    saved++;
                }
                catch (Exception ex)
                {
                    failed.Add($"{row.KAID} - {ex.Message}");
                }
            }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return Ok(new
            {
                Success = saved,
                Failed = failed.Count,
                Errors = failed
            });
        }


    }
}