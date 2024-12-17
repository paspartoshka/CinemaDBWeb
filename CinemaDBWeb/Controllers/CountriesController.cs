using System.Threading.Tasks;
using CinemaDBWeb.Data;
using CinemaDBWeb.Models;
using Microsoft.AspNetCore.Mvc;

public class CountriesController : Controller
{
    private readonly CinemaDBContext _context;

    public CountriesController(CinemaDBContext context)
    {
        _context = context;
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name")] Country country)
    {
        if (ModelState.IsValid)
        {
            _context.Add(country);
            await _context.SaveChangesAsync();
            return RedirectToAction("Create", "Movies");
        }
        return View(country);
    }
}
