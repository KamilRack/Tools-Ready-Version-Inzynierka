﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Narzedzia.Data;
using Narzedzia.Models;

namespace Narzedzia.Controllers
{
	[Authorize(Roles = "admin, nadzor")]
	public class KategorieController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KategorieController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Kategorie
        public async Task<IActionResult> Index()
        {
              return View(await _context.Kategorie.Include(n => n.Narzedzia).ToListAsync());
        }

        // GET: Kategorie/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Kategorie == null)
            {
                return NotFound();
            }

            var kategoria = await _context.Kategorie
                .FirstOrDefaultAsync(m => m.KategoriaId == id);
            if (kategoria == null)
            {
                return NotFound();
            }

            return View(kategoria);
        }

        // GET: Kategorie/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Kategorie/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("KategoriaId,NazwaKategorii,Active")] Kategoria kategoria)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kategoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(kategoria);
        }

        // GET: Kategorie/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Kategorie == null)
            {
                return NotFound();
            }

            var kategoria = await _context.Kategorie.FindAsync(id);
            if (kategoria == null)
            {
                return NotFound();
            }
            return View(kategoria);
        }

        // POST: Kategorie/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("KategoriaId,NazwaKategorii,Active")] Kategoria kategoria)
        {
            if (id != kategoria.KategoriaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kategoria);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KategoriaExists(kategoria.KategoriaId))
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
            return View(kategoria);
        }

        // GET: Kategorie/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Kategorie == null)
            {
                return NotFound();
            }

            var kategoria = await _context.Kategorie
                .FirstOrDefaultAsync(m => m.KategoriaId == id);
            if (kategoria == null)
            {
                return NotFound();
            }
            if (KategoriaNiepusta((int)id))
            {
                ViewBag.DeleteMessage = "Nie można usunąć wybranego elementu, gdyż posiada przypisane narzędzia.";
            }

            return View(kategoria);
        }

        // POST: Kategorie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Kategorie == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Kategorie'  is null.");
            }
            var kategoria = await _context.Kategorie.FindAsync(id);
            if (kategoria != null)
            {
                _context.Kategorie.Remove(kategoria);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KategoriaExists(int id)
        {
          return _context.Kategorie.Any(e => e.KategoriaId == id);
        }

        private bool KategoriaNiepusta(int id)
        {
            return (_context.Narzedzia?.Any(t => t.KategoriaId == id)).GetValueOrDefault();
        }
    }
}
