using System.Threading.Tasks;
using CinemaDBWeb.Data;
using CinemaDBWeb.Models;
using Microsoft.AspNetCore.Mvc;

public class DirectorsController : Controller
{
    private readonly CinemaDBStorage _storage;

    public DirectorsController(CinemaDBStorage storage)
    {
        _storage = storage;
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string Name, string Surname)
    {
        _storage.AddDirector(Name, Surname);
        return RedirectToAction("Create", "Movies");
    }
}
