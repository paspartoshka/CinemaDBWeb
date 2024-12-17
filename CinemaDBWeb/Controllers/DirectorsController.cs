using System.Threading.Tasks;
using CinemaDBWeb.Data;
using CinemaDBWeb.Models;
using Microsoft.AspNetCore.Mvc;

public class DirectorsController : Controller
{
    private readonly CinemaDBContext _context;

    public DirectorsController(CinemaDBContext context)
    {
        _context = context;
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string Name, string Surname)
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Surname))
        {
            ModelState.AddModelError("", "Имя и фамилия обязательны");
            return View();
        }

        var person = new Person { Name = Name, Surname = Surname, Job = "Director" };
        _context.Persons.Add(person);
        await _context.SaveChangesAsync();

        var director = new Director { PersonId = person.PersonId };
        _context.Directors.Add(director);
        await _context.SaveChangesAsync();

        return RedirectToAction("Create", "Movies");
    }
}
