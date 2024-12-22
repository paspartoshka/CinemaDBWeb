using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using CinemaDBWeb.Data;
using CinemaDBWeb.Models;

namespace CinemaDBWeb.Controllers
{
    public class HallsController : Controller
    {
        private readonly CinemaDBStorage _storage;

        public HallsController(CinemaDBStorage storage)
        {
            _storage = storage;
        }

        // GET: Halls/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Halls/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HallType,RowCount,SeatCount,PriceMult,ProjType")] Hall hall)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
            if (ModelState.IsValid)
            {
                _storage.AddHall(hall);
                return RedirectToAction("Create", "Sessions");
            }
            return View();
        }
    }
}
