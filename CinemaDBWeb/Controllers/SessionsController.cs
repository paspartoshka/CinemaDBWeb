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
        private readonly CinemaDBContext _context;

        public SessionsController(CinemaDBContext context)
        {
            _context = context;
        }

        // GET: Sessions
        public async Task<IActionResult> Index()
        {
            var sessions = _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall);
            return View(await sessions.ToListAsync());
        }

        // GET: Sessions/Create
        public IActionResult Create()
        {
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "Title");
            ViewData["HallId"] = new SelectList(_context.Halls, "HallId", "HallType");
            return View();
        }

        // POST: Sessions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Date,BasePrice,MovieId,HallId")] Session session)
        {
           
                _context.Add(session);
                await _context.SaveChangesAsync();


                var hall = await _context.Halls.FirstOrDefaultAsync(h => h.HallId == session.HallId);
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
                            _context.Tickets.Add(ticket);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "Title", session.MovieId);
            ViewData["HallId"] = new SelectList(_context.Halls, "HallId", "HallType", session.HallId);
            return View(session);
        }

        // GET: Sessions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
                return NotFound();

            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "Title", session.MovieId);
            ViewData["HallId"] = new SelectList(_context.Halls, "HallId", "HallType", session.HallId);
            return View(session);
        }

        // POST: Sessions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SessionId,Date,BasePrice,MovieId,HallId")] Session session)
        {
            if (id != session.SessionId)
                return NotFound();

            try
            {

                _context.Update(session);
                await _context.SaveChangesAsync();


                var hall = await _context.Halls
                    .FirstOrDefaultAsync(h => h.HallId == session.HallId);

                if (hall != null)
                {

                    var tickets = await _context.Tickets
                        .Where(t => t.SessionId == session.SessionId && !t.isSold)
                        .ToListAsync();


                    foreach (var ticket in tickets)
                    {
                        ticket.Price = session.BasePrice * hall.PriceMult;
                    }


                    _context.UpdateRange(tickets);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
                {
                    if (!SessionExists(session.SessionId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            

            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "Title", session.MovieId);
            ViewData["HallId"] = new SelectList(_context.Halls, "HallId", "HallType", session.HallId);
            return View(session);
        }

        // GET: Sessions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var session = await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(m => m.SessionId == id);
            if (session == null)
                return NotFound();

            return View(session);
        }

        // POST: Sessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var session = await _context.Sessions
                .Include(s => s.Tickets)
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null)
                return NotFound();


            _context.Tickets.RemoveRange(session.Tickets);


            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> BuyTicket(int ticketId)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketId == ticketId);
            if (ticket == null || ticket.isSold)
            {
                return NotFound();
            }

            ticket.isSold = true;
            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }

        private bool SessionExists(int id)
        {
            return _context.Sessions.Any(e => e.SessionId == id);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var session = await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Tickets)
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null)
                return NotFound();

            return View(session);
        }
    }
}
