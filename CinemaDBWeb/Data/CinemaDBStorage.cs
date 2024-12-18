using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CinemaDBWeb.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;

namespace CinemaDBWeb.Data
{
    public class CinemaDBStorage
    {
        private readonly CinemaDBContext _context;

        public CinemaDBStorage(CinemaDBContext cinemaContext)
        {
            _context = cinemaContext;
        }


        // ФИЛЬМЫ /////////////////////////////////////////////////////////////////

        public void AddMovie(Movie movie, int[] countryIds, string posterFileName)
        {
            foreach (var countryId in countryIds)
            {
                var country = _context.Countries.Find(countryId);
                if (country != null)
                {
                    movie.Countries.Add(country);
                }
            }

            movie.PosterFileName = posterFileName;

            _context.Add(movie);
            _context.SaveChanges();
        }
        public Movie GetMovie(int id)
        {
            return _context.Movies
            .Include(m => m.Company)
            .Include(m => m.Director)
                .ThenInclude(d => d.Person)
            .Include(m => m.Countries)
            .FirstOrDefault(m => m.MovieId == id);
        }
        public Movie GetMovieWithCountries(int movieId)
        {
            return _context.Movies
            .Include(m => m.Countries)
            .FirstOrDefault(m => m.MovieId == movieId);
        }

        public void DeleteMovie(int movieId)
        {
            var movie = GetMovieWithCountries(movieId);

            // Очистка связанных сущностей (например, стран)
            movie.Countries.Clear();
            _context.SaveChanges();

            // Удаление постера
            var posterPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", movie.PosterFileName);
            if (System.IO.File.Exists(posterPath))
            {
                System.IO.File.Delete(posterPath);
            }


            _context.Movies.Remove(movie);
            _context.SaveChanges();
        }

        public IEnumerable<Movie> GetAllMovies()
        {
            return _context.Movies
                .Include(m => m.Company)
                .Include(m => m.Director)
                    .ThenInclude(d => d.Person)
                .Include(m => m.Countries)
                .ToList();
        }


        // Добавление стран к фильму
        public void AddCountriesToMovie(Movie movie, int[] countryIds)
        {
            foreach (var countryId in countryIds)
            {
                var country = _context.Countries.FirstOrDefault(c => c.CountryId == countryId);
                if (country != null)
                {
                    movie.Countries.Add(country);
                }
            }
            _context.SaveChanges();
        }

        // Сохранение постера
        public string SavePoster(IFormFile poster)
        {
            if (poster == null || poster.Length == 0)
            {
                return null;
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(poster.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                poster.CopyTo(stream);
            }

            return fileName;
        }

        // Удаление постера
        public void DeletePoster(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public IEnumerable<Country> GetCountriesByIds(int[] countryIds)
        {
            return _context.Countries
                .Where(c => countryIds.Contains(c.CountryId))
                .ToList();
        }


        // Обновление фильма
        public bool UpdateMovie(Movie movie, int[] countryIds, IFormFile poster)
        {
            var movieToUpdate = GetMovie(movie.MovieId);
            if (movieToUpdate == null) return false;

            movieToUpdate.Title = movie.Title;
            movieToUpdate.Length = movie.Length;
            movieToUpdate.LicPrice = movie.LicPrice;
            movieToUpdate.DaysIn = movie.DaysIn;
            movieToUpdate.AgeLimit = movie.AgeLimit;
            movieToUpdate.Rating = movie.Rating;
            movieToUpdate.ReleaseYear = movie.ReleaseYear;
            movieToUpdate.CompanyId = movie.CompanyId;
            movieToUpdate.DirectorId = movie.DirectorId;

            // Обновление стран
            movieToUpdate.Countries.Clear();
            var countries = GetCountriesByIds(countryIds);
            foreach (var country in countries)
            {
                movieToUpdate.Countries.Add(country);
            }

            // Обработка изображения постера
            if (poster != null && poster.Length > 0)
            {
                var postersDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(poster.FileName)}";
                var newFilePath = Path.Combine(postersDirectory, newFileName);

                using (var stream = new FileStream(newFilePath, FileMode.Create))
                {
                    poster.CopyTo(stream);
                }

                // Удаление старого постера
                if (!string.IsNullOrEmpty(movieToUpdate.PosterFileName))
                {
                    var oldFilePath = Path.Combine(postersDirectory, movieToUpdate.PosterFileName);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }


                movieToUpdate.PosterFileName = newFileName;
            }
            _context.SaveChanges();
            return true;
        }

        // Метод для получения всех компаний
        public SelectList GetCompaniesSelectList()
        {
            return new SelectList(_context.Companies, "CompanyId", "Name");
        }

        // Метод для получения списка директоров
        public SelectList GetDirectorsSelectList()
        {
            return new SelectList(_context.Directors.Include(d => d.Person), "DirectorId", "Person.Surname");
        }

        // Метод для получения списка стран
        public MultiSelectList GetCountriesMultiSelectList()
        {
            return new MultiSelectList(_context.Countries, "CountryId", "Name");
        }



        // КОМПАНИИ /////////////////////////////////

        public void AddCompany(Company company)
        {
            _context.Add(company);
            _context.SaveChanges();
        }

        // СТРАНЫ /////////////////////////////////

        public void AddCountry(Country country)
        {
            _context.Add(country);
            _context.SaveChanges();
        }

        // РЕЖИСЁРЫ ///////////////////////////

        public void AddDirector(string name, string surname)
        {
            var person = new Person { Name = name, Surname = surname, Job = "Director" };
            _context.Add(person);
            _context.SaveChanges();
            var director = new Director { PersonId = person.PersonId };
            _context.Add(director);
            _context.SaveChanges();
        }


        // СЕАНСЫ ////////////////////////////

        public SelectList GetMovies()
        {
            return new SelectList(_context.Movies, "MovieId", "Title");
        }


        public SelectList GetHalls()
        {
            return new SelectList(_context.Halls, "HallId", "HallType");
        }

        public IEnumerable<Hall> GetHalls2()
        {
            return _context.Halls.ToList(); // Возвращаем список объектов Hall
        }
        public IEnumerable<Session> GetSessions()
        {
            return _context.Sessions
             .Include(s => s.Movie)
             .Include(s => s.Hall)
             .ToList();
        }

        public Session GetSession(int id)
        {
            return _context.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .FirstOrDefault(s => s.SessionId == id);
        }

        public void AddSession(Session session)
        {
            _context.Sessions.Add(session);
        }

        public void UpdateSession(Session session)
        {
            _context.Sessions.Update(session);
        }

        public void RemoveSession(int id)
        {
            var session = _context.Sessions.Find(id);
            _context.Sessions.Remove(session);
        }

        public IEnumerable<Ticket> GetTicketsBySession(int sessionId)
        {
            return _context.Tickets.Where(t => t.SessionId == sessionId).ToList();
        }

        public void AddTicket(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
        }
        public void UpdateTicket(Ticket ticket)
        {
            _context.Tickets.Update(ticket);
        }
        public void RemoveTickets(IEnumerable<Ticket> tickets)
        {
            _context.Tickets.RemoveRange(tickets);

        }

        public void BuyTicket(int ticketId)
        {
            var ticket = _context.Tickets.FirstOrDefault(t => t.TicketId == ticketId);
            ticket.isSold = true;
            _context.SaveChanges();
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
        
        public bool SessionExists(int sessionId)
        {
            return _context.Sessions.Any(e => e.SessionId == sessionId);
        }


        // ЗАЛЫ //////////////

        public void AddHall(Hall hall)
        {
            _context.Add(hall);
            _context.SaveChanges();
        }

        
    }
}
