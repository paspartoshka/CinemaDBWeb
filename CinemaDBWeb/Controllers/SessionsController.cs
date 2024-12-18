using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaDBWeb.Data;
using CinemaDBWeb.Models;

namespace CinemaDBWeb.Controllers
{
    public class SessionsController : Controller
    {
        private readonly CinemaDBStorage _storage;

        public SessionsController(CinemaDBStorage storage)
        {
            _storage = storage;
        }

        // GET: Sessions
        public async Task<IActionResult> Index()
        {
            var sessions = _storage.GetSessions();
            return View(sessions);
        }

        // GET: Sessions/Create
        public IActionResult Create()
        {
            ViewData["MovieId"] = _storage.GetMovies();
            ViewData["HallId"] = _storage.GetHalls();
            return View();
        }

        // POST: Sessions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Date,BasePrice,MovieId,HallId")] Session session)
        {
            _storage.AddSession(session);
            _storage.SaveChanges();
            CreateTicketsForSession(session);
            return RedirectToAction(nameof(Index));
        }

        private void CreateTicketsForSession(Session session)
        {
            var halls = _storage.GetHalls2();  // Преобразуем SelectList в список объектов Hall
            var hall = halls.FirstOrDefault(h => h.HallId == session.HallId);  // Находим нужный зал
            if (hall != null)
            {
                for (int r = 1; r <= hall.RowCount; r++)
                {
                    for (int s = 1; s <= hall.SeatCount; s++)
                    {
                        var ticket = new Ticket
                        {
                            SessionId = session.SessionId,
                            RowNumb = r,
                            SeatNumb = s,
                            Price = session.BasePrice * hall.PriceMult,
                            isSold = false
                        };
                        _storage.AddTicket(ticket);
                    }
                }
                _storage.SaveChanges();
            }
        }

        // GET: Sessions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var session = _storage.GetSession(id.Value);
            ViewData["MovieId"] = _storage.GetMovies();
            ViewData["HallId"] = _storage.GetHalls();
            return View(session);
        }

        // POST: Sessions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SessionId,Date,BasePrice,MovieId,HallId")] Session session)
        {
            _storage.UpdateSession(session);
            _storage.SaveChanges();
            UpdateTicketPrices(session);
            return RedirectToAction(nameof(Index));
        }
        private void UpdateTicketPrices(Session session)
        {
            var halls = _storage.GetHalls2();  // Преобразуем SelectList в список объектов Hall
            var hall = halls.FirstOrDefault(h => h.HallId == session.HallId);  // Находим нужный зал
            if (hall != null)
            {
                var tickets = _storage.GetTicketsBySession(session.SessionId);
                foreach (var ticket in tickets)
                {
                    ticket.Price = session.BasePrice * hall.PriceMult;
                    _storage.UpdateTicket(ticket);
                }
                _storage.SaveChanges();
            }
        }

        // GET: Sessions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var session = _storage.GetSession(id.Value);
            return View(session);
        }

        // POST: Sessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _storage.RemoveSession(id);
            _storage.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> BuyTicket(int ticketId)
        {
            _storage.BuyTicket(ticketId);
            return RedirectToAction(nameof(Index));
        }

        private bool SessionExists(int id)
        {
            return _storage.GetSessions().Any(e => e.SessionId == id);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var session = _storage.GetSession(id.Value);
            var tickets = _storage.GetTicketsBySession(id.Value);
            return View(session);
        }
    }
}
