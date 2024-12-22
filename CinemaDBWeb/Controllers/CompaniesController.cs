using CinemaDBWeb.Data;
using CinemaDBWeb.Models;
using Microsoft.AspNetCore.Mvc;

public class CompaniesController : Controller
{
    private readonly CinemaDBStorage _storage;
    public CompaniesController(CinemaDBStorage storage)
    {
        _storage = storage;
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
            _storage.AddCompany(company);
            return RedirectToAction("Create", "Movies");
        }
        return View();
    }
}
