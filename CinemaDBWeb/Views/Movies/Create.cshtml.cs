using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CinemaDBWeb.Models;
using CinemaDBWeb.Data;

namespace CinemaDBWeb.Pages.Movies
{
    public class CreateModel : PageModel
    {
        private readonly CinemaDBContext _context;

        public CreateModel(CinemaDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Movie Movie { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                _context.Movies.Add(Movie);
                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            return Page();
        }
    }
}