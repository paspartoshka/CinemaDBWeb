using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using CinemaDBWeb.Data;
using CinemaDBWeb.Models;

namespace CinemaDBWeb.Controllers
{
    public class HallsController : Controller
    {
        private readonly CinemaDBContext _context;

        public HallsController(CinemaDBContext context)
        {
            _context = context;
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

                _context.Add(hall);
                await _context.SaveChangesAsync();
                // После создания зала можно сделать редирект назад на создание сеанса или на список залов:
                return RedirectToAction("Create", "Sessions");
            return View(hall);
        }
    }
}
