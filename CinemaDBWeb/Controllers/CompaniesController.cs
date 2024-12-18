using CinemaDBWeb.Data;
using CinemaDBWeb.Models;
using Microsoft.AspNetCore.Mvc;

public class CompaniesController : Controller
{
    private readonly CinemaDBContext _context;
    public CompaniesController(CinemaDBContext context)
    {
        _context = context;
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Year")] Company company)
    {
        if (ModelState.IsValid)
        {
            _context.Add(company);
            await _context.SaveChangesAsync();

            return RedirectToAction("Create", "Movies");
        }
        return View(company);
    }
}
