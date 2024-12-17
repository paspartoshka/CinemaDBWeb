using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaDBWeb.Data;
using CinemaDBWeb.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CinemaDBWeb.Controllers
{
    public class MoviesController : Controller
    {
        private readonly CinemaDBContext _context;

        public MoviesController(CinemaDBContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            var movies = _context.Movies
                .Include(m => m.Company)
                .Include(m => m.Director)
                    .ThenInclude(d => d.Person) 
                .Include(m => m.Countries);
            return View(await movies.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "Name");
            ViewData["DirectorId"] = new SelectList(_context.Directors.Include(d => d.Person), "DirectorId", "Person.Surname");
            ViewData["CountryIds"] = new MultiSelectList(_context.Countries, "CountryId", "Name");
            return View();
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Length,LicPrice,DaysIn,AgeLimit,Rating,ReleaseYear,CompanyId,DirectorId")] Movie movie, int[] CountryIds, IFormFile poster)
        { 
                foreach (var countryId in CountryIds)
                {
                    var country = await _context.Countries.FindAsync(countryId);
                    if (country != null)
                    {
                        movie.Countries.Add(country);
                    }
                }
                if (poster != null && poster.Length > 0)
                {
                    // Генерируем уникальное имя файла
                    var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(poster.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await poster.CopyToAsync(stream);
                    }

                    movie.PosterFileName = fileName;
                }

            _context.Add(movie);
                await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Company)
                .Include(m => m.Director)
                    .ThenInclude(d => d.Person)
                .Include(m => m.Countries)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Загружаем фильм вместе с привязанными странами
            var movie = await _context.Movies
                .Include(m => m.Countries)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
            {
                return NotFound();
            }

            // Очищаем коллекцию стран, чтобы убрать записи из таблицы связей
            movie.Countries.Clear();
            await _context.SaveChangesAsync(); // Сохраняем изменения (удаление связей)

            // Теперь удаляем сам фильм
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // Дополнительные действия (Edit, Details, Delete) можно добавить по необходимости

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Countries)
                .Include(m => m.Director)
                    .ThenInclude(d => d.Person)
                .Include(m => m.Company)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
            {
                return NotFound();
            }

            // Подготовка данных для выпадающих списков
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "Name", movie.CompanyId);
            // Для режиссеров: выбираем "Фамилию" через Person
            ViewData["DirectorId"] = new SelectList(_context.Directors
                .Include(d => d.Person), "DirectorId", "Person.Surname", movie.DirectorId);

            // Для стран: MultiSelectList с уже выбранными странами
            var selectedCountryIds = movie.Countries.Select(c => c.CountryId).ToList();
            ViewData["CountryIds"] = new MultiSelectList(_context.Countries, "CountryId", "Name", selectedCountryIds);

            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MovieId,Title,Length,LicPrice,DaysIn,AgeLimit,Rating,ReleaseYear,CompanyId,DirectorId")] Movie movie, int[] CountryIds, IFormFile poster)
        {
            if (id != movie.MovieId)
            {
                return NotFound();
            }

            // Загружаем исходный объект из БД с его связанными коллекциями
            var movieToUpdate = await _context.Movies
                .Include(m => m.Countries)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movieToUpdate == null)
            {
                return NotFound();
            }

            // Обновляем простые поля
            movieToUpdate.Title = movie.Title;
            movieToUpdate.Length = movie.Length;
            movieToUpdate.LicPrice = movie.LicPrice;
            movieToUpdate.DaysIn = movie.DaysIn;
            movieToUpdate.AgeLimit = movie.AgeLimit;
            movieToUpdate.Rating = movie.Rating;
            movieToUpdate.ReleaseYear = movie.ReleaseYear;
            movieToUpdate.CompanyId = movie.CompanyId;
            movieToUpdate.DirectorId = movie.DirectorId;

            // Перезаписываем связанные страны
            movieToUpdate.Countries.Clear();
            foreach (var countryId in CountryIds)
            {
                var country = await _context.Countries.FindAsync(countryId);
                if (country != null)
                {
                    movieToUpdate.Countries.Add(country);
                }
            }

            if (poster != null && poster.Length > 0)
            {
                // Если ранее был постер - можно при необходимости удалить старый файл
                if (!string.IsNullOrEmpty(movieToUpdate.PosterFileName))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", movieToUpdate.PosterFileName);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(poster.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await poster.CopyToAsync(stream);
                }

                movieToUpdate.PosterFileName = fileName;
            }



            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(movie.MovieId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.MovieId == id);
        }
    }
}