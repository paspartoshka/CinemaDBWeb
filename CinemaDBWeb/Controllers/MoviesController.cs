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

            var movie = await _context.Movies
                .Include(m => m.Countries)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
            {
                return NotFound();
            }




            movie.Countries.Clear();
            await _context.SaveChangesAsync();


                var posterPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", movie.PosterFileName);
                if (System.IO.File.Exists(posterPath))
                {
                        System.IO.File.Delete(posterPath);   
                }
            


            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


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


            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "Name", movie.CompanyId);

            ViewData["DirectorId"] = new SelectList(_context.Directors
                .Include(d => d.Person), "DirectorId", "Person.Surname", movie.DirectorId);


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


            var movieToUpdate = await _context.Movies
                .Include(m => m.Countries)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movieToUpdate == null)
            {
                return NotFound();
            }


            movieToUpdate.Title = movie.Title;
            movieToUpdate.Length = movie.Length;
            movieToUpdate.LicPrice = movie.LicPrice;
            movieToUpdate.DaysIn = movie.DaysIn;
            movieToUpdate.AgeLimit = movie.AgeLimit;
            movieToUpdate.Rating = movie.Rating;
            movieToUpdate.ReleaseYear = movie.ReleaseYear;
            movieToUpdate.CompanyId = movie.CompanyId;
            movieToUpdate.DirectorId = movie.DirectorId;


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
                var postersDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");


                var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(poster.FileName)}";
                var newFilePath = Path.Combine(postersDirectory, newFileName);

                using (var stream = new FileStream(newFilePath, FileMode.Create))
                {
                    await poster.CopyToAsync(stream);
                }

           
                    var oldFilePath = Path.Combine(postersDirectory, movieToUpdate.PosterFileName);
                    if (System.IO.File.Exists(oldFilePath))
                    {

                            System.IO.File.Delete(oldFilePath);                
                   }
                


                movieToUpdate.PosterFileName = newFileName;
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