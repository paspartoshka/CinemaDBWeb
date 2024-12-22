using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CinemaDBWeb.Models
{
    public class Company
    {
        public int CompanyId { get; set; }
        [Required(ErrorMessage = "Название компании обязательно")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Год основания обязателен")]
        public int? Year { get; set; }

        public ICollection<Movie> Movies { get; set; }

        public Company()
        {
            Movies = new List<Movie>();
        }
    }
}
