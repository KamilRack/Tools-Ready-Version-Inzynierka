using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Narzedzia.Data;
using Narzedzia.Helpers;
using Narzedzia.Models;
using Narzedzia.Models.ViewModels;
using Narzedzia.Controllers;

namespace Narzedzia.Controllers
{
    public class CalendarViewController : Controller
    {
        private readonly ILogger<CalendarViewController> _logger;
        private readonly IDAL _idal;
        private readonly IDAL _dal;



        public CalendarViewController(ILogger<CalendarViewController> logger, IDAL idal, IDAL dal)
        {
            _logger = logger;
            _idal = idal;
            _dal = dal;
        }
        public IActionResult CalendarVW()
        {
            var viewModel = new EventsViewModel();

             // Pobierz listę narzędzi z IDAL
    var narzedziaList = _idal.GetNarzedzias();

            // Konwertuj listę narzędzi na listę SelectListItem
            viewModel.Narzedzie = narzedziaList.Select(n => new SelectListItem { Value = n.NarzedzieId.ToString(), Text = n.Nazwa }).ToList();

            // Pobierz listę narzędzi z IDAL
            var wydzialylist = _idal.GetWydzials();

            // Konwertuj listę narzędzi na listę SelectListItem
            viewModel.Wydzial = wydzialylist.Select(n => new SelectListItem { Value = n.WydzialId.ToString(), Text = n.NazwaWydzialu }).ToList();

            // Pobierz listę narzędzi z IDAL
            var stanowiskalist = _idal.GetStanowiskos();

            // Konwertuj listę narzędzi na listę SelectListItem
            viewModel.Stanowisko = stanowiskalist.Select(n => new SelectListItem { Value = n.StanowiskoId.ToString(), Text = n.NazwaStanowiska }).ToList();



            ViewData["Resources"] = JSONListHelper.GetResourceListJSONString(_idal.GetStanowiskos(), _idal.GetWydzials(), _idal.GetNarzedzias());
            ViewData["Events"] = JSONListHelper.GetEventListJSONString(_idal.GetEvents());
/*            return View();
*/            return View(viewModel);

        }

        // Dodaj tę akcję, która będzie obsługiwać przekierowanie po dodaniu nowego wydarzenia
        public IActionResult CreateEventAndRedirectToCalendarVW(EventsViewModel vm, IFormCollection form)
        {
            try
            {
                _dal.CreateEvent(form);
                TempData["Alert"] = "Success! You created a new event for: " + form["Event.Name"];
                return RedirectToAction("CalendarVW");
            }
            catch (Exception ex)
            {
                ViewData["Alert"] = "An error occurred: " + ex.Message;
                return RedirectToAction("CalendarVW"); // Możesz dostosować to przekierowanie, jeśli to konieczne
            }
        }






    }
}
