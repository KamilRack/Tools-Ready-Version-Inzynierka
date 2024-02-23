using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Narzedzia.Data;
using Narzedzia.Models;
using System.IO;
using OfficeOpenXml;

namespace Narzedzia.Controllers
{
    public class NarzedziaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NarzedziaController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult ExportToExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var awarie = _context.Awarie
                .Include(a => a.Narzedzie)
                .Include(a => a.Uzytkownicy)
                .ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Awarie");

                // Dodaj nagłówki
                worksheet.Cells[1, 1].Value = "ID Awarii";
                worksheet.Cells[1, 2].Value = "Nazwa Narzędzia";
                worksheet.Cells[1, 3].Value = "Opis Awarii";
                worksheet.Cells[1, 4].Value = "Numer Telefonu";
                worksheet.Cells[1, 5].Value = "Data Przyjęcia";
                worksheet.Cells[1, 6].Value = "Użytkownik Zgłaszający";
                worksheet.Cells[1, 7].Value = "Status Awarii";
                worksheet.Cells[1, 8].Value = "Notatka Techniczna";

                // Dodaj dane
                for (int i = 0; i < awarie.Count; i++)
                {
                    var awaria = awarie[i];

                    worksheet.Cells[i + 2, 1].Value = awaria.IdAwaria;
                    worksheet.Cells[i + 2, 2].Value = awaria.Narzedzie?.Nazwa;
                    worksheet.Cells[i + 2, 3].Value = awaria.DescriptionAwaria;
                    worksheet.Cells[i + 2, 4].Value = awaria.NumberAwaria;
                    worksheet.Cells[i + 2, 5].Value = awaria.DataPrzyjecia.ToString("dd.MM.yyyy");
                    worksheet.Cells[i + 2, 6].Value = awaria.Uzytkownicy?.Imie_Nazwisko;
                    worksheet.Cells[i + 2, 7].Value = awaria.Status.ToString();
                    worksheet.Cells[i + 2, 8].Value = awaria.NotatkaTechniczna;
                }

                // Zapisz plik
                var stream = new MemoryStream();
                package.SaveAs(stream);

                // Zwróć plik
                var fileName = "Awarie.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                stream.Position = 0;
                return File(stream, contentType, fileName);
            }
        }
        // GET: Narzedzia
        [Authorize(Roles = "admin, nadzor")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Narzedzia.Include(n => n.Kategorie).Include(n => n.Producenci).Include(n => n.Uzytkownicy);
            return View(await applicationDbContext.ToListAsync());
        }

        [Authorize(Roles = "admin, nadzor, pracownik")]

        public async Task<IActionResult> ListGraphic()
        {
            var applicationDbContext = _context.Narzedzia.Include(n => n.Kategorie).Include(n => n.Producenci).Include(n => n.Uzytkownicy);
            return View(await applicationDbContext.ToListAsync());
        }
        [Authorize(Roles = "admin, nadzor")]
        // GET: Narzedzia/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Narzedzia == null)
            {
                return NotFound();
            }

            var narzedzie = await _context.Narzedzia
                .Include(n => n.Kategorie)
                .Include(n => n.Producenci)
                .Include(n => n.Uzytkownicy)
                .FirstOrDefaultAsync(m => m.NarzedzieId == id);
            ViewBag.Status = narzedzie.Status;
            if (narzedzie == null)
            {
                return NotFound();
            }

            return View(narzedzie);
        }

        // GET: Narzedzia/Create
        public IActionResult Create()
        {
            ViewData["KategoriaId"] = new SelectList(_context.Kategorie.Where(c => c.Active == true), "KategoriaId", "NazwaKategorii");
            ViewData["ProducentId"] = new SelectList(_context.Producenci.Where(c => c.Active == true), "ProducentId", "NazwaProducenta");
            ViewData["UzytkownikId"] = new SelectList(_context.Uzytkownicy, "Id", "Id");
            return View();
        }

        // POST: Narzedzia/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NarzedzieId,ProducentId,KategoriaId,DataPrzyjecia,UzytkownikId,NumerNarzedzia,Nazwa,Status,Uwagi,ZdjecieFileName")] Narzedzie narzedzie, IFormFile ZdjecieFileName)
        {
            if (ModelState.IsValid)
            {
                // Tworzenie i zapisanie pliku graficznego
                if (ZdjecieFileName != null && ZdjecieFileName.Length > 0)
                {
                    // Wygeneruj unikalny identyfikator dla nazwy pliku na serwerze (np. GUID)
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ZdjecieFileName.FileName);
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/narzedziagraphic", uniqueFileName);

                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await ZdjecieFileName.CopyToAsync(stream);
                    }

                    // Przypisz nazwę pliku do właściwości ZdjecieFileName modelu Narzedzie
                    narzedzie.ZdjecieFileName = uniqueFileName;
                }
                else
                {
                    // Jeśli nie przesłano pliku, przypisz domyślną nazwę pliku
                    narzedzie.ZdjecieFileName = "052ffaa7-6567-4203-84e9-79118520d54d_test2.jpg"; // Zmodyfikuj nazwę pliku według własnych potrzeb
                }
                narzedzie.DataPrzyjecia = DateTime.Now;

                _context.Add(narzedzie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Jeśli walidacja nie powiodła się, ponownie zwróć widok Create z błędami
            ViewData["KategoriaId"] = new SelectList(_context.Kategorie, "KategoriaId", "NazwaKategorii", narzedzie.KategoriaId);
            ViewData["ProducentId"] = new SelectList(_context.Producenci, "ProducentId", "NazwaProducenta", narzedzie.ProducentId);
            ViewData["UzytkownikId"] = new SelectList(_context.Uzytkownicy, "Id", "Id", narzedzie.UzytkownikId);
            return View(narzedzie);
        }

        // GET: Narzedzia/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var narzedzie = await _context.Narzedzia
                .Include(n => n.Uzytkownicy)
                .FirstOrDefaultAsync(m => m.NarzedzieId == id);

            if (narzedzie == null)
            {
                return NotFound();
            }

            ViewData["KategoriaId"] = new SelectList(_context.Kategorie, "KategoriaId", "NazwaKategorii", narzedzie.KategoriaId);
            ViewData["ProducentId"] = new SelectList(_context.Producenci, "ProducentId", "NazwaProducenta", narzedzie.ProducentId);

            if (narzedzie.UzytkownikId == null)
            {
                ViewData["UzytkownikId"] = new SelectList(_context.Uzytkownicy, "Id", "Email");
            }
            else
            {
                ViewData["UzytkownikId"] = new SelectList(_context.Uzytkownicy, "Id", "Email", narzedzie.UzytkownikId);
                ViewData["Obecny"] = narzedzie.Uzytkownicy?.Email;
                ViewData["ObecnyId"] = narzedzie.UzytkownikId;
            }

            return View(narzedzie);
        }

        // POST: Narzedzia/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NarzedzieId,ProducentId,KategoriaId,DataPrzyjecia,NumerNarzedzia,Nazwa,Status,UzytkownikId,Uwagi,ZdjecieFileName")] Narzedzie narzedzie, IFormFile ZdjecieFileName, string obecny, bool usunZdjecie = false)
        {
            if (id != narzedzie.NarzedzieId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingNarzedzie = await _context.Narzedzia
                        .FirstOrDefaultAsync(m => m.NarzedzieId == id);

                    if (existingNarzedzie == null)
                    {
                        return NotFound();
                    }

                    // Usunięcie zdjęcia, jeśli użytkownik zaznaczył odpowiednią opcję
                    if (usunZdjecie && !string.IsNullOrEmpty(existingNarzedzie.ZdjecieFileName))
                    {
                        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/narzedziagraphic", existingNarzedzie.ZdjecieFileName);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }

                        existingNarzedzie.ZdjecieFileName = null;
                    }

                    // Jeśli użytkownik przesyła nowe zdjęcie, zaktualizuj je
                    if (ZdjecieFileName != null && ZdjecieFileName.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ZdjecieFileName.FileName);
                        string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/narzedziagraphic", uniqueFileName);

                        using (var stream = new FileStream(uploadPath, FileMode.Create))
                        {
                            await ZdjecieFileName.CopyToAsync(stream);
                        }

                        existingNarzedzie.ZdjecieFileName = uniqueFileName;
                    }

                    // Aktualizacja pozostałych pól narzędzia
                    existingNarzedzie.ProducentId = narzedzie.ProducentId;
                    existingNarzedzie.KategoriaId = narzedzie.KategoriaId;
                    existingNarzedzie.DataPrzyjecia = narzedzie.DataPrzyjecia;
                    existingNarzedzie.NumerNarzedzia = narzedzie.NumerNarzedzia;
                    existingNarzedzie.Nazwa = narzedzie.Nazwa;
                    existingNarzedzie.Status = narzedzie.Status;
                    existingNarzedzie.Uwagi = narzedzie.Uwagi;
                    existingNarzedzie.UzytkownikId = narzedzie.UzytkownikId; // Ustawienie UzytkownikId na nową wartość

                    _context.Update(existingNarzedzie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NarzedzieExists(narzedzie.NarzedzieId))
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

            // Inicjalizacja SelectList
            ViewData["KategoriaId"] = new SelectList(_context.Kategorie, "KategoriaId", "NazwaKategorii", narzedzie.KategoriaId);
            ViewData["ProducentId"] = new SelectList(_context.Producenci, "ProducentId", "NazwaProducenta", narzedzie.ProducentId);
            ViewData["UzytkownikId"] = new SelectList(_context.Uzytkownicy, "Id", "Id", narzedzie.UzytkownikId);

            if (narzedzie.UzytkownikId == null)
            {
                ViewData["UzytkownikId"] = new SelectList(_context.Uzytkownicy, "Id", "Email");
            }
            else
            {
                ViewData["UzytkownikId"] = new SelectList(_context.Uzytkownicy, "Id", "Email", narzedzie.UzytkownikId);
                ViewData["Obecny"] = narzedzie.Uzytkownicy?.Email;
                ViewData["ObecnyId"] = narzedzie.UzytkownikId;
            }

            return View(narzedzie);
        }

        // GET: Narzedzia/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Narzedzia == null)
            {
                return NotFound();
            }

            var narzedzie = await _context.Narzedzia
                .Include(n => n.Kategorie)
                .Include(n => n.Producenci)
                .Include(n => n.Uzytkownicy)
                .FirstOrDefaultAsync(m => m.NarzedzieId == id);
            if (narzedzie == null)
            {
                return NotFound();
            }
            if (!NarzedzieZlikwidowane((int)id))
            {
                ViewBag.DeleteMessage = "Nie można usunąć wybranego elementu, gdyż nie został wcześniej zlikwidowany (zmiana statusu).";
            }

            return View(narzedzie);
        }

        // POST: Narzedzia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Narzedzia == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Narzedzia'  is null.");
            }
            var narzedzie = await _context.Narzedzia.FindAsync(id);
            if (narzedzie != null)
            {
                _context.Narzedzia.Remove(narzedzie);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NarzedzieExists(int id)
        {
          return _context.Narzedzia.Any(e => e.NarzedzieId == id);
        }

        private bool NarzedzieZlikwidowane(int id)
        {
            var status =  _context.Narzedzia.FirstOrDefault(x => x.NarzedzieId == id).Status;
            if (status == Status.zlikwidowane) { return true; }
            return false;
        }
    }
}
