using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaDBWeb.Data;
using CinemaDBWeb.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.Identity.Client.Extensions.Msal;

namespace CinemaDBWeb.Controllers
{
    public class MoviesController : Controller
    {
        private readonly CinemaDBStorage _storage;

        public MoviesController(CinemaDBStorage storage)
        {
            _storage = storage;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            var movies = _storage.GetAllMovies();
            return View(movies);
        }

        public IActionResult Create()
        {
            ViewData["CompanyId"] = _storage.GetCompaniesSelectList();
            ViewData["DirectorId"] = _storage.GetDirectorsSelectList();
            ViewData["CountryIds"] = _storage.GetCountriesMultiSelectList();
            return View();
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Length,LicPrice,DaysIn,AgeLimit,Rating,ReleaseYear,CompanyId,DirectorId")] Movie movie, int[] CountryIds, IFormFile? poster)
        {

            if (CountryIds == null || CountryIds.Length == 0)
            {
                ModelState.AddModelError("CountryIds", "Пожалуйста, выберите хотя бы одну страну.");
            }

            if (poster == null || poster.Length == 0)
            {
                ModelState.AddModelError("poster", "Пожалуйста, загрузите постер.");
            }
            if (ModelState.IsValid)
            {
                var posterFileName = _storage.SavePoster(poster);
                _storage.AddMovie(movie, CountryIds, posterFileName);
                return RedirectToAction(nameof(Index));
            }

            ViewData["CompanyId"] = _storage.GetCompaniesSelectList();
            ViewData["DirectorId"] = _storage.GetDirectorsSelectList();
            ViewData["CountryIds"] = _storage.GetCountriesMultiSelectList();
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var movie = _storage.GetMovie(id.Value);
            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _storage.DeleteMovie(id);
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(int? id)
        {
            var movie = _storage.GetMovie(id.Value);
            ViewData["CompanyId"] = _storage.GetCompaniesSelectList();
            ViewData["DirectorId"] = _storage.GetDirectorsSelectList();
            ViewData["CountryIds"] = _storage.GetCountriesMultiSelectList();
            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MovieId,Title,Length,LicPrice,DaysIn,AgeLimit,Rating,ReleaseYear,CompanyId,DirectorId")] Movie movie, int[] CountryIds, IFormFile? poster)
        {
            if (CountryIds == null || CountryIds.Length == 0)
            {
                ModelState.AddModelError("CountryIds", "Пожалуйста, выберите хотя бы одну страну.");
            }

            if (poster == null || poster.Length == 0)
            {
                ModelState.AddModelError("poster", "Пожалуйста, загрузите постер.");
            }
            if (ModelState.IsValid)
            {
                var movieToUpdate = _storage.GetMovie(id);
                bool updateSuccess = _storage.UpdateMovie(movie, CountryIds, poster);
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = _storage.GetCompaniesSelectList();
            ViewData["DirectorId"] = _storage.GetDirectorsSelectList();
            ViewData["CountryIds"] = _storage.GetCountriesMultiSelectList();
            return View(movie);
        }
    }
}