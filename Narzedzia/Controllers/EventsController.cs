using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Narzedzia.Data;
using Narzedzia.Models;
using Narzedzia.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Narzedzia.Controllers;
namespace Narzedzia.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDAL _dal;

        public EventsController(IDAL dal)
        {
            _dal = dal;
        }

        // GET: Events
        public IActionResult Index()
        {
            if (TempData["Alert"] != null)
            {
                ViewData["Alert"] = TempData["Alert"];
            }
            return View(_dal.GetEvents());
        }

       
        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @events = _dal.GetEvent((int)id);
            if (@events == null)
            {
                return NotFound();
            }

            return View(@events);
        }

        // GET: Events/Create
        public IActionResult Create()
        {

            var wydzials = _dal.GetWydzials();
            var stanowiskos = _dal.GetStanowiskos();
            var narzedzias = _dal.GetNarzedzias();
       
            return View(new EventsViewModel(wydzials, stanowiskos, narzedzias));

        }


        // POST: Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventsViewModel vm, IFormCollection form)
        {

            try
            {
                _dal.CreateEvent(form);
                TempData["Alert"] = "Success! You created a new event for: " + form["Event.Name"];
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["Alert"] = "An error occurred: " + ex.Message;
                return View(vm);
            }
        }

        // GET: Events/Create
        public IActionResult Create1()
        {

            var wydzials = _dal.GetWydzials();
            var stanowiskos = _dal.GetStanowiskos();
            var narzedzias = _dal.GetNarzedzias();

            return View(new EventsViewModel(wydzials, stanowiskos, narzedzias));

        }


        // POST: Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create1(EventsViewModel vm, IFormCollection form)
        {

            try
            {
                _dal.CreateEvent(form);
                TempData["Alert"] = "Success! You created a new event for: " + form["Event.Name"];
                return RedirectToAction("CreateEventAndRedirectToCalendarVW", "CalendarView");
            }
            catch (Exception ex)
            {
                ViewData["Alert"] = "An error occurred: " + ex.Message;
                return View(vm);
            }
        }
        // GET: Events/Edit/5
        public IActionResult Edit(int? id)
        {

            var wydzials = _dal.GetWydzials();
            var stanowiskos = _dal.GetStanowiskos();
            var narzedzias = _dal.GetNarzedzias();
            if (id == null)
            {
                return NotFound();
            }

            var @event = _dal.GetEvent((int)id);
            if (@event == null)
            {
                return NotFound();
            }
            /*            var vm = new EventsViewModel(@event, _dal.GetWydzials(), _dal.GetStanowiskos(), _dal.GetNarzedzias());
            */

            var vm = new EventsViewModel(@event, wydzials, stanowiskos, narzedzias);
            return View(vm);


        }


        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormCollection form)
        {
        
            try
                {
                    _dal.UpdateEvent(form);
                TempData["Alert"] = "Succes Modyfikacja zrobiona" + form["Events.NameCal"];
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
                {
                ViewData["Alert"] = "An Error Occured:" + ex.Message;
                var vm = new EventsViewModel(_dal.GetEvent(id), _dal.GetWydzials(), _dal.GetStanowiskos(),_dal.GetNarzedzias());
                return View(vm);

            }


        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @events = _dal.GetEvent((int)id);
            if (@events == null)
            {
                return NotFound();
            }

            return View(@events);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _dal.DeleteEvent(id);
            TempData["Alert"] = "You deleted avent";
            return RedirectToAction(nameof(Index));
        }

    }
}
