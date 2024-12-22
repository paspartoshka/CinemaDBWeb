using System.Threading.Tasks;
using CinemaDBWeb.Data;
using CinemaDBWeb.Models;
using Microsoft.AspNetCore.Mvc;

public class CountriesController : Controller
{
    private readonly CinemaDBStorage _storage;

    public CountriesController(CinemaDBStorage storage)
    {
        _storage = storage;
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
            _storage.AddCountry(country);
            return RedirectToAction("Create", "Movies");
        }
        return View();
    }
}
