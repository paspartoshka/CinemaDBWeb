using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CinemaDBWeb.Models
{
    public class Country
    {
        public int CountryId { get; set; }
        [Required(ErrorMessage = "Название страны обязательно")]
        public string Name { get; set; }

        public ICollection<Movie> Movies { get; set; }

        public Country()
        {
            Movies = new List<Movie>();
        }
    }
}
