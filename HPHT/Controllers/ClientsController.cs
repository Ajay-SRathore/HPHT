using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HPHT.Data;
using HPHT.Models;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml;

namespace HPHT.Controllers
{
    [Authorize(Roles = "Admin")]


    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }
       [HttpPost]
[Route("api/clients/upload")]
        public async Task<IActionResult> Upload(
            IFormFile file)
        {
            if (file == null)
                return BadRequest("No file selected");

            ExcelPackage.LicenseContext =
                LicenseContext.NonCommercial;

            using var stream =
                new MemoryStream();

            await file.CopyToAsync(stream);

            using var package =
                new ExcelPackage(stream);

            var sheet =
                package.Workbook.Worksheets[0];

            List<Clients> clients =
                new();

            for (int row = 2;
                 row <= sheet.Dimension.Rows;
                 row++)
            {
                string name =
                    sheet.Cells[row, 1].Text;

                string codeText =
                    sheet.Cells[row, 2].Text;

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                int code =
                    int.Parse(codeText);

                bool exists =
                    _context.Clients.Any(
                        x => x.ClientCode == code);

                if (exists)
                    continue;

                clients.Add(
                    new Clients
                    {
                        ClientCode = code,
                        Name = name
                    });
            }

            await _context.Clients
                .AddRangeAsync(clients);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                inserted = clients.Count
            });
        }

        // GET: Clients
        public async Task<IActionResult> Index()
        {
            return View(await _context.Clients.ToListAsync());
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clients = await _context.Clients
                .FirstOrDefaultAsync(m => m.ClientCode == id);
            if (clients == null)
            {
                return NotFound();
            }

            return View(clients);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Clients clients)
        {
            if (ModelState.IsValid)
            {
                _context.Add(clients);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(clients);
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clients = await _context.Clients.FindAsync(id);
            if (clients == null)
            {
                return NotFound();
            }
            return View(clients);
        }

        // POST: Clients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Clients clients)
        {
            if (id != clients.ClientCode)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(clients);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientsExists(clients.ClientCode))
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
            return View(clients);
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clients = await _context.Clients
                .FirstOrDefaultAsync(m => m.ClientCode == id);
            if (clients == null)
            {
                return NotFound();
            }

            return View(clients);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var clients = await _context.Clients.FindAsync(id);
            if (clients != null)
            {
                _context.Clients.Remove(clients);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClientsExists(int? id)
        {
            return _context.Clients.Any(e => e.ClientCode == id);
        }
    }
}
