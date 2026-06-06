using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HPHT.Data;
using HPHT.Models;
using OfficeOpenXml;
using Microsoft.AspNetCore.Authorization;


namespace HPHT.Controllers
{

    [Authorize(Roles = "Admin,User")]
    public class IssuesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IssuesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Issues
        public async Task<IActionResult> Index(
    int page = 1,
    int pageSize = 10,
    int? ccode = null,
    string? kaid = null,
    string? clientId = null)
        {
            var query = _context.Issues
    .AsNoTracking()
    .Include(x => x.Client)
    .AsQueryable();

            // CLIENT FILTER
            if (ccode.HasValue)
            {
                query = query.Where(x => x.ClientCode == ccode);
            }

            // KAID FILTER
            if (!string.IsNullOrWhiteSpace(kaid))
            {
                query = query.Where(x => x.KAID.Contains(kaid));
            }

            // CLIENT ID FILTER
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                query = query.Where(x => x.ClientId.Contains(clientId));
            }

            int totalRecords = await query.CountAsync();

            var issues = await query
    .OrderByDescending(x => x.IssueDate)
    .ThenByDescending(x => x.Id)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages =
                (int)Math.Ceiling((double)totalRecords / pageSize);

            ViewBag.PageSize = pageSize;
            ViewBag.KAID = kaid;
            ViewBag.ClientId = clientId;
            ViewBag.ccode = ccode;

            ViewBag.Clients = await _context.Clients
                .AsNoTracking()
                .Select(x => new SelectListItem
                {
                    Value = x.ClientCode.ToString(),
                    Text = x.Name
                })
                .ToListAsync();

            return View(issues);
        }
        public IActionResult Repeat()
        {
            return View();
        }

        [HttpPost("upload-excel")]
        public async Task<IActionResult> UploadExcel(IFormFile file, int ccode)
        {

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            var issues = new List<Issues>();

            for (int i = 2; i <= sheet.Dimension.Rows; i++)
            {
                var issue = new Issues
                {
                    SrNo = int.Parse(sheet.Cells[i, 1].Text),
                    LotNo = sheet.Cells[i, 2].Text,
                    PktNo = sheet.Cells[i, 3].Text,
                    PCS = int.Parse(sheet.Cells[i, 4].Text),
                    IssueWeight = decimal.Parse(sheet.Cells[i, 5].Text),
                    Shape = sheet.Cells[i, 6].Text,
                    Exp = sheet.Cells[i, 7].Text,
                    Price = decimal.Parse(sheet.Cells[i, 8].Text),
                    Remarks = sheet.Cells[i, 9].Text,
                    IssueDate = DateTime.Now,
                    IssuedBy = "admin",
                    ClientCode = ccode   // ✅ SAME CLIENT FOR ALL ROWS
                };

                issues.Add(issue);
            }

            _context.Issues.AddRange(issues);
            await _context.SaveChangesAsync();

            return Ok("Uploaded successfully");
        }

        // GET: Issues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var issue = await _context.Issues
                .FirstOrDefaultAsync(m => m.Id == id);
            if (issue == null)
            {
                return NotFound();
            }

            return View(issue);
        }
        //[HttpGet("clients")]
        //public async Task<IActionResult> GetClients()
        //{
        //    var data = await _context.Clients.ToListAsync();
        //    return Ok(data);
        //}

        // GET: Issues/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Issues/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SrNo,LotNo,PktNo,PCS,IssueWeight,Shape,Exp,Price,Remarks,IssueDate,IssuedBy")] Issues issue, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                _context.Add(issue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(issue);
        }

        // GET: Issues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
            {
                return NotFound();
            }
            return View(issue);
        }

        // POST: Issues/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SrNo,LotNo,PktNo,PCS,IssueWeight,Shape,Exp,Price,Remarks,IssueDate,IssuedBy")] Issues issue)
        {
            if (id != issue.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(issue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IssueExists(issue.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(issue);
        }

        // GET: Issues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var issue = await _context.Issues
                .FirstOrDefaultAsync(m => m.Id == id);
            if (issue == null)
            {
                return NotFound();
            }

            return View(issue);
        }

        // POST: Issues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue != null)
            {
                _context.Issues.Remove(issue);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IssueExists(int id)
        {
            return _context.Issues.Any(e => e.Id == id);
        }
        public IActionResult Return()
        {
            return View();
        }
    }
}
